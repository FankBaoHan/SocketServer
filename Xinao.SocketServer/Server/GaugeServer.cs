using ConsoleTables;
using MySqlX.XDevAPI;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using static System.Collections.Specialized.BitVector32;

namespace Xinao.SocketServer.Server
{
    public class GaugeServer : AppServer<GaugeSession, GaugeInfo>
    {
        public GaugeServer()
            : base(new DefaultReceiveFilterFactory<GaugeFilter, GaugeInfo>())
        {

        }

        protected override void OnStarted()
        {
            LogUtil.LogImportant($"【沉降】服务已启动");
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            LogUtil.LogError($"【沉降】服务已停止");
            base.OnStopped();
        }

        protected override void OnNewSessionConnected(GaugeSession session)
        {
            LogUtil.LogImportant($"【沉降】设备尝试连接->IP: {session.RemoteEndPoint.Address}:{session.RemoteEndPoint.Port} SessionId: {session.SessionID}");

            //连接建立即进行心跳监测
            //new Task(() => SendHeartBeat(session)).Start();

            base.OnNewSessionConnected(session);
        }

        protected override void OnSessionClosed(GaugeSession session, CloseReason reason)
        {
            LogUtil.LogError($"【沉降】设备已断连->IP: {session.RemoteEndPoint.Address} DtuCode: {session.DtuCode} SessionId: {session.SessionID}");
            base.OnSessionClosed(session, reason);
        }

        protected override void ExecuteCommand(GaugeSession session, GaugeInfo requestInfo)
        {
            //未验证
            if (string.IsNullOrEmpty(session.DtuCode))
            {
                var dtuExisted = session.CheckDtu(requestInfo.ReadBuffer);

                if (!dtuExisted)
                {
                    LogUtil.LogError($"【沉降】设备序列号验证失败->ReceivedId: {Encoding.ASCII.GetString(requestInfo.ReadBuffer)} ");
                    try { session.Close(); } catch { }
                    return;
                }

                var gauges = session.Gauges;
                if (gauges.Count == 0)
                {
                    LogUtil.LogError($"【沉降】该Dtu下未配置沉降设备->ReceivedId: {Encoding.ASCII.GetString(requestInfo.ReadBuffer)} ");
                    try { session.Close(); } catch { }
                    return;
                }

                if (gauges.Count > 0)
                {
                    LogUtil.LogImportant($"【沉降】设备连接成功->DtuCode: {session.DtuCode} ");
                    new Task(()=>SendCommand(session, requestInfo)).Start();
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
        protected void SendCommand(GaugeSession session, GaugeInfo requestInfo)
        {
            var firstTime = true;//首次连接时发送2遍（实际设备上电时有接收延迟，易粘包）

            //modbus读取指令
            while (session.Connected)
            {
                var gauges = session.Gauges;

                foreach (var gauge in gauges)
                {
                    try
                    {
                        var data = DeviceUtil.GetGaugeCmd(gauge.gauge_code);
                        session.Send(data, 0, data.Length);

                        if (firstTime)
                        {
                            firstTime = false;

                            Thread.Sleep(1000 * DeviceUtil.GAUGE_FIRST_TIME_SEND_INTERVAL);
                            session.Send(data, 0, data.Length);
                        }
                    }
                    catch (Exception e)
                    {
                        LogUtil.LogError($"【沉降】发送Modbus指令失败->DtuCode: {session.DtuCode} Reason: {e.Message}");

                        try { session.Close(); } catch { }

                        break;
                    }

                    /*设备间采集间隔*/
                    Thread.Sleep(DeviceUtil.GAUGE_GATHER_BETWEEN_INTERVAL);
                }

                /*dtu采样周期*/
                Thread.Sleep(1000 * session.Frequence);
            }
        }

        /// <summary>
        /// 发送心跳 如失败则断连
        /// </summary>
        protected void SendHeartBeat(GaugeSession session)
        {
            while (session.Connected)
            {
                try
                {
                    session.Send("Q");
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"【沉降】发送心跳失败->DtuCode: {session.DtuCode} Reason: {e.Message}");

                    try { session.Close(); } catch { }

                    break;
                }

                Thread.Sleep(DeviceUtil.GAUGE_HEARTBEAT_INTERVAL);
            }
        }

        /// <summary>
        /// 接收并解析modbus报文
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        protected void ReceiveCommand(GaugeSession session, GaugeInfo requestInfo)
        {
            if (requestInfo.ReadBuffer.Length > DeviceUtil.GAUGE_MAX_DATA_LENGTH)
            {
                LogUtil.LogError($"【沉降】设备返回数据过长->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            if (!DeviceUtil.checkCrc16(requestInfo.ReadBuffer))
            {
                LogUtil.LogError($"【沉降】设备返回数据校验错误->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            if (requestInfo.ReadBuffer[1] != 0x04)
            {
                LogUtil.LogError($"【沉降】设备返回数据功能码错误->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            if (requestInfo.ReadBuffer[2] != 0x0C)
            {
                LogUtil.LogError($"【沉降】设备返回数据返回长度错误->DtuCode: {session.DtuCode} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            var gaugeData = DeviceUtil.ParseGauge(requestInfo, session.DtuCode);
            var gaugeSettings = session.Gauges.Where(g => g.gauge_code == gaugeData.SlaveId).FirstOrDefault();

            var isThisBaseGauge = session.Gauges
                .Where(g => g.is_base == 1 && g.gauge_code == gaugeData.SlaveId)
                .FirstOrDefault() != null;

            if (isThisBaseGauge)
                session.BenchmarkLevel = gaugeData.Level;

            /*存GaugeData*/
            if (session.BenchmarkLevel == null)
            {
                LogUtil.LogError($"【沉降】设备暂未收到基准数据,不作存储->DtuName: {session.DtuName} Data: {BitConverter.ToString(requestInfo.ReadBuffer)}");
                return;
            }

            StoreData(session, gaugeData, gaugeSettings);

            StoreAlarmData(session, gaugeData, gaugeSettings);
        }

        /// <summary>
        /// 存采集数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="gaugeData"></param>
        /// <param name="gaugeSettings"></param>
        private void StoreData(GaugeSession session, GaugeInfo gaugeData, BaseGauge gaugeSettings)
        {
            var gd = new AdminGaugeData()
            {
                id = Snowflake.GetUId(),
                pipeline_id = session.PipelineId,
                pipeline_name = session.PipelineName,
                dtu_id = session.DtuId,
                dtu_name = session.DtuName,
                gauge_id = gaugeSettings?.id,
                gauge_code = gaugeSettings?.gauge_code,
                gauge_name = gaugeSettings?.gauge_name,
                gauge_location = gaugeSettings?.gauge_location,
                gauge_pressure = gaugeData.Pressure,
                gauge_temp = gaugeData.Temprature,
                gauge_data = gaugeData.Level,
                gmt_create = DateTime.Now,
                date_create = DateTime.Now.ToString("yyyy-MM-dd"),
                deleted = false
            };

            if (session.BenchmarkLevel != null)
            {
                gd.subside_data = (gaugeData.Level - session.BenchmarkLevel) - gaugeSettings.base_data;
                gd.is_to_warn = Math.Abs((float)gd.subside_data) > gaugeSettings.to_warn_data;
                if (gaugeSettings.is_base == 2)
                {
                    var remark = gd.subside_data > 0 ? "沉降报警" : "抬升报警";
                    gd.remarks = $"{gd.pipeline_name} {gd.dtu_name} {remark}";
                }
                else if (gaugeSettings.is_base == 1)
                {
                    gd.remarks = "该条数据为基准点数据";
                }
            }

            try
            {
                DbContext.DbClient.Insertable(gd).ExecuteCommand();
            }
            catch
            {
                LogUtil.LogError($"【沉降】写入GaugeData数据库错误->dtuId: {session.DtuId} slaveId: {gaugeData.SlaveId}");
            }

            //统一报警表
            if (gd.is_to_warn == false) return;

			var alreadyWarnedToday = DbContext.DbClient.Queryable<BaseWarnData>()
                .Where(o=>o.deleted==false
                    && o.device_id == gd.gauge_id
                    && o.alarm_type == 1
                    && o.gmt_create != null 
                    && DateTime.Now.Date == ((DateTime)o.gmt_create).Date)
                .Any();

            //若采集频率小于1天，则按实际采集数据储存
            if (alreadyWarnedToday && session.Frequence >= 86400) return;
			
			var warnData = new BaseWarnData()
			{
				id = Snowflake.GetUId(),
                device_id = gd.gauge_id,
				device_name = gd.gauge_name,
				device_code = gd.gauge_code?.ToString(),
				c_id = gd.id,
				pipeline_id = gd.pipeline_id,
				pipeline_name = gd.pipeline_name,
				dtu_id = gd.dtu_id,
				dtu_name = gd.dtu_name,
				dtu_type = 1,//沉降
				alarm_type = 1, //正常告警
				alarm_content = gd.remarks,
				is_deal = false,
				gmt_create = DateTime.Now,
				deleted = false,
			};

			try
			{
				DbContext.DbClient.Insertable(warnData).ExecuteCommand();
			}
			catch
			{
				LogUtil.LogError($"【沉降】写入BaseWarnData数据库错误->dtuId: {session.DtuId} slaveId: {gaugeData.SlaveId}");
			}

		}

        /// <summary>
        /// 存报警数据
        /// </summary>
        private void StoreAlarmData(GaugeSession session, GaugeInfo gaugeData, BaseGauge gaugeSettings)
        {
            var gd = new AdminGaugeWarnData()
            {
                id = Snowflake.GetUId(),
                pipeline_id = session.PipelineId,
                pipeline_name = session.PipelineName,
                dtu_id = session.DtuId,
                dtu_name = session.DtuName,
                gauge_id = gaugeSettings?.id,
                gauge_code = gaugeSettings?.gauge_code,
                gauge_name = gaugeSettings?.gauge_name,
                gauge_location = gaugeSettings?.gauge_location,
                gauge_pressure = gaugeData.Pressure,
                gauge_temp = gaugeData.Temprature,
                //gauge_data = gaugeData.Level,
                get_time = DateTime.Now,
                gmt_create = DateTime.Now,
                date_create = DateTime.Now.ToString("yyyy-MM-dd"),
                deleted = false
            };

            if (session.BenchmarkLevel != null)
            {
                gd.subside_data = (gaugeData.Level - session.BenchmarkLevel) - gaugeSettings.base_data;

                bool yesterday2Warn = false, all2Warn = false;
                float yesterDaySubsideData = 0f;
                
                //日变化报警
                if (session.BaseValueYesterday != null)
                {
                    yesterDaySubsideData = (float)(gd.subside_data - session.BaseValueYesterday);
                    if (gaugeSettings.is_to_warn == true)
                        yesterday2Warn = Math.Abs(yesterDaySubsideData) >= (gaugeSettings.to_warn_data ?? 0.0);      
                }

                //累计变化报警
                if (gaugeSettings.all_is_to_warn == true)
                    all2Warn = Math.Abs((float)gd.subside_data) >= (gaugeSettings.all_to_warn_data??0.0);

                var alarmComment = new StringBuilder();
                var comment = gd.subside_data > 0?"沉降":"抬升";

                if (gaugeSettings.is_to_warn == true && all2Warn)
                    alarmComment.Append($"日{comment}报警;");

                if (gaugeSettings.is_to_warn == true && yesterday2Warn)
                    alarmComment.Append($"累计{comment}报警;");

                if (gaugeSettings.is_base == 2)
                {
                    gd.remarks = alarmComment.ToString();
                    gd.is_to_warn = all2Warn | yesterday2Warn;
                }
                else if (gaugeSettings.is_base == 1)
                {
                    gd.remarks = "该条数据为基准点数据";
                }
            }

            try
            {
                DbContext.DbClient.Insertable(gd).ExecuteCommand();
            }
            catch
            {
                LogUtil.LogError($"【沉降】写入GaugeWarnData数据库错误->dtuId: {session.DtuId} slaveId: {gaugeData.SlaveId}");
            }

            if (DeviceUtil.GAUGE_WECHAT_ON == true && gd.is_to_warn == true)
                WechatUtil.SendMessage($"{gd.dtu_name}-{gd.gauge_name}", gd.remarks, WechatUtil.GAUGE_CODE);
        }

        /// <summary>
        /// 打印服务器状态至控制台
        /// </summary>
        public void PrintServerState()
        {
            var sessions = this.GetAllSessions(); 
            LogUtil.LogState($"【沉降】服务状态->连接总数: {sessions.Count()} ");

            var table = new ConsoleTable("No", "Dtu Code", "IP", "Start Time", "Frequency",  "Refresh Time", "Dtu Name");

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
