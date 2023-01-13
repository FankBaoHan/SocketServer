using ConsoleTables;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xinao.SocketServer.Database;
using Xinao.SocketServer.Database.Models;
using Xinao.SocketServer.Filter;
using Xinao.SocketServer.Info;
using Xinao.SocketServer.Session;
using Xinao.SocketServer.Utils;

namespace Xinao.SocketServer.Server
{
    public class ProtectServer : AppServer<ProtectSession, ProtectInfo>
    {
        public ProtectServer()
            : base(new DefaultReceiveFilterFactory<ProtectFilter, ProtectInfo>())
        {

        }

        protected override void OnStarted()
        {
            LogUtil.LogImportant($"【阴保】服务已启动");
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            LogUtil.LogError($"【阴保】服务已停止");
            base.OnStopped();
        }

        protected override void OnNewSessionConnected(ProtectSession session)
        {
            LogUtil.LogImportant($"【阴保】设备尝试连接->IP: {session.RemoteEndPoint.Address}:{session.RemoteEndPoint.Port} SessionId: {session.SessionID}");

            //连接建立即进行心跳监测
            //new Task(() => SendHeartBeat(session)).Start();

            base.OnNewSessionConnected(session);
        }

        protected override void OnSessionClosed(ProtectSession session, CloseReason reason)
        {
            LogUtil.LogError($"【阴保】设备已断连->IP: {session.RemoteEndPoint.Address} DtuCode: {session.DtuCode}  SessionId: {session.SessionID}");
            base.OnSessionClosed(session, reason);
        }

        protected override void ExecuteCommand(ProtectSession session, ProtectInfo requestInfo)
        {
            //未验证
            if (string.IsNullOrEmpty(session.DtuCode))
            {
                var dtuExisted = session.CheckDtu(requestInfo.ReadBuffer);

                if (!dtuExisted)
                {
                    LogUtil.LogError($"【阴保】设备序列号验证失败->ReceivedId: {Encoding.ASCII.GetString(requestInfo.ReadBuffer)} ");
                    try { session.Close(); } catch { }
                    return;
                }

                var device = session.Device;
                if (device == null)
                {
                    LogUtil.LogError($"【阴保】该Dtu下未配置阴保设备->ReceivedId: {Encoding.ASCII.GetString(requestInfo.ReadBuffer)} ");
                    try { session.Close(); } catch { }
                    return;
                }

                //序列号验证成功
                if (device != null)
                {
                    LogUtil.LogImportant($"【阴保】设备连接成功->DtuCode: {session.DtuCode} ");
                    new Task(() => SendCommand(session, requestInfo)).Start();
                    new Task(() => SendHeartBeat(session)).Start();
                }
            }
            //已验证 解析数据
            else
            {
                ReceiveCommand(session, requestInfo);
            }
        }

        /// <summary>
        /// 发送modbus读取指令
        /// </summary>
        protected void SendCommand(ProtectSession session, ProtectInfo requestInfo)
        {
            var firstTime = true;//首次连接时发送2遍（实际设备上电时有接收延迟，易粘包）

            //modbus读取指令
            while (session.Connected)
            {
                var device = session.Device;

                try
                {
                    var data = DeviceUtil.GetProtectCmd(1);//目前固定为1
                    session.Send(data, 0, data.Length);

                    if (firstTime)
                    {
                        firstTime = false;

                        Thread.Sleep(1000 * DeviceUtil.PROTECT_FIRST_TIME_SEND_INTERVAL);
                        session.Send(data, 0, data.Length);
                    }
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"【阴保】发送Modbus指令失败->DtuCode: {session.DtuCode} Reason: {e.Message}");

                    try { session.Close(); } catch { }

                    break;
                }

                /*dtu采样周期*/
                Thread.Sleep(1000 * session.Frequence);
            }
        }

        /// <summary>
        /// 发送心跳 如失败则断连
        /// </summary>
        protected void SendHeartBeat(ProtectSession session)
        {
            while (session.Connected)
            {
                try
                {
                    session.Send("Q");
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"【阴保】发送心跳失败->DtuCode: {session.DtuCode} Reason: {e.Message}");

                    try { session.Close(); } catch { }

                    break;
                }

                Thread.Sleep(DeviceUtil.PROTECT_HEARTBEAT_INTERVAL);
            }
        }

        /// <summary>
        /// 接收并解析modbus报文
        /// </summary>
        protected void ReceiveCommand(ProtectSession session, ProtectInfo requestInfo)
        {
            if (requestInfo.ReadBuffer.Length > DeviceUtil.PROTECT_MAX_DATA_LENGTH)
            {
                LogUtil.LogError($"【阴保】设备返回数据过长->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            if (!DeviceUtil.checkCrc16(requestInfo.ReadBuffer))
            {
                LogUtil.LogError($"【阴保】设备返回数据校验错误->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            if (requestInfo.ReadBuffer[1] != 0x03)
            {
                LogUtil.LogError($"【阴保】设备返回数据功能码错误->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            if (requestInfo.ReadBuffer[2] != 0x0E)
            {
                LogUtil.LogError($"【阴保】设备返回数据返回长度错误->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            var protectData = DeviceUtil.ParseProtect(requestInfo, session.DtuCode);
            var protectSettings = session.Device;

            StoreData(session, protectData, protectSettings);
        }

        /// <summary>
        /// 存采集数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        /// <param name=""></param>
        private void StoreData(ProtectSession session, ProtectInfo protectData, BaseProtectDevice protectSettings)
        {
            var configs = protectSettings?.configs;

            if (configs == null)
            {
                LogUtil.LogError($"【阴保】设备未配置任何参数->DtuName: {session.DtuName} DeviceName: {protectSettings.device_name}");
                try { session.Close(); } catch { }
                return;
            }

            var list = new List<ProtectDeviceData>();
            

            foreach (var config in configs)
            {
                float data;
                string comment, lowComment, highComment;

                switch(config.types)
                {
                    case 1:
                        data = protectData.ElectricPotential;
                        comment = "阴保电位";
                        lowComment = "过负";
                        highComment = "过正";
                        break;
                    case 2:
                        data = protectData.NaturePotential;
                        comment = "自然电位";
                        lowComment = "过负";
                        highComment = "过正";
                        break;
                    case 3:
                        data = protectData.DcCurrent;
                        comment = "直流电流";
                        lowComment = "过小";
                        highComment = "过大";
                        break;
                    case 4:
                        data = protectData.AcInterferenceVoltage;
                        comment = "交流干扰电压";
                        lowComment = "过小";
                        highComment = "过大";
                        break;
                    case 5:
                        data = protectData.AcStrayVoltage;
                        comment = "交流杂散电流";
                        lowComment = "过小";
                        highComment = "过大";
                        break;
                    default:
                        return;
                }

                var k = string.IsNullOrEmpty(config.k) ? 1 : float.Parse(config.k);
                var b = string.IsNullOrEmpty(config.b) ? 0 : float.Parse(config.b);

                data = data * k + b;

                var isAlarm = config.alarm_enable == true && (data >= config.high_limit || data <= config.low_limit);

                var pd = new ProtectDeviceData()
                {
                    id = Snowflake.GetUId(),
                    device_id = protectSettings.id,
                    gather_time = DateTime.Now,
                    gather_date = DateTime.Now.ToString("yyyy/MM/dd"),
                    device_name = protectSettings.device_name,
                    pipeline_name = session.PipelineName,
                    dtu_name = session.DtuName,
                    pipeline_id = session.PipelineId,
                    dtu_id = session.DtuId,
                    is_deal = false,
                    types = config.types,
                    low_limit = config.low_limit,
                    high_limit = config.high_limit,
                    actual_value = data,
                    is_alarm = isAlarm,
                    deleted = false,
                    alarm_comment = isAlarm?comment + ((data>=(float)config.high_limit)?highComment:lowComment):""
                };

                list.Add(pd);
            }

            try
            {
                DbContext.DbClient.Insertable(list).ExecuteCommand();
            }
            catch
            {
                LogUtil.LogError($"【阴保】写入ProtectDeviceData数据库错误->dtuId: {session.DtuId} dtuName: {session.DtuName}");
            }
        }

        /// <summary>
        /// 打印服务器状态至控制台
        /// </summary>
        public void PrintServerState()
        {
            var sessions = this.GetAllSessions();
            LogUtil.LogState($"【阴保】服务状态->连接总数: {sessions.Count()} ");

            var table = new ConsoleTable("No", "Dtu Code", "IP", "Start Time", "Frequency", "Refresh Time", "Dtu Name");

            int i = 1;
            foreach (var session in sessions)
            {
                table.AddRow(i++,
                    session.DtuCode,
                    $"{session.RemoteEndPoint.Address}:{session.RemoteEndPoint.Port} ",
                    session.StartTime,
                    session.Frequence,
                    session.LastTimeRefreshData,
                    session.DtuName);
            }

            table.Write(Format.Alternative);
        }
    }
}
