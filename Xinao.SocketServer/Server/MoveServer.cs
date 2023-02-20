using ConsoleTables;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xinao.SocketServer.Database;
using Xinao.SocketServer.Database.Models;
using Xinao.SocketServer.Filter;
using Xinao.SocketServer.Info;
using Xinao.SocketServer.Session;
using Xinao.SocketServer.Utils;

namespace Xinao.SocketServer.Server
{
    public class MoveServer : AppServer<MoveSession, MoveInfo>
    {
        public MoveServer()
            : base(new DefaultReceiveFilterFactory<MoveFilter, MoveInfo>())
        {

        }

        protected override void OnStarted()
        {
            LogUtil.LogImportant($"【位移】服务已启动");
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            LogUtil.LogError($"【位移】服务已停止");
            base.OnStopped();
        }

        protected override void OnNewSessionConnected(MoveSession session)
        {
            LogUtil.LogImportant($"【位移】设备尝试连接->IP: {session.RemoteEndPoint.Address}:{session.RemoteEndPoint.Port} SessionId: {session.SessionID}");

            //new Task(() => SendHeartBeat(session)).Start();

            base.OnNewSessionConnected(session);
        }

        protected override void OnSessionClosed(MoveSession session, CloseReason reason)
        {
            LogUtil.LogError($"【位移】设备已断连->IP: {session.RemoteEndPoint.Address} DtuCode: {session.DtuCode} SessionId: {session.SessionID}");
            base.OnSessionClosed(session, reason);
        }

        protected override void ExecuteCommand(MoveSession session, MoveInfo requestInfo)
        {
            string strData;

            try 
            { 
                strData = Encoding.ASCII.GetString(requestInfo.ReadBuffer); 
            } 
            catch 
            {
                LogUtil.LogError($"【位移】设备数据读取失败");
                try { session.Close(); } catch { }
                return;
            }

            //未验证
            if (string.IsNullOrEmpty(session.DtuCode))
            {
                var dtuCode = DeviceUtil.GetMoveDtuCode(strData);
                var dtuExisted = session.CheckDtu(dtuCode);

                if (!dtuExisted)
                {
                    LogUtil.LogError($"【位移】设备序列号验证失败->SessionId: {session.SessionID} Times: {session.TimesTry2Connect} Content: {strData}");
                    
                    if (session.TimesTry2Connect++ >= DeviceUtil.TIME_TRY_TO_CONNECT)
                        try { session.Close(); LogUtil.LogError($"【位移】序列号验证失败,设备断连->SessionId: {session.SessionID} "); } catch { }

                    return;
                }

                if (session.Devices.Count == 0)
                {
                    LogUtil.LogError($"【位移】该Dtu下未配置位移设备->ReceivedCode: {dtuCode} ");
                    try { session.Close(); } catch { }
                    return;
                }

                LogUtil.LogImportant($"【位移】设备连接成功->DtuCode: {session.DtuCode} ");
                new Task(() => SendHeartBeat(session)).Start();
            }
            //已验证 解析数据
            else
            {
                ReceiveCommand(session, requestInfo);
            }
        }

        /// <summary>
        /// 发送心跳 如失败则断连
        /// </summary>
        protected void SendHeartBeat(MoveSession session)
        {
            while (session.Connected)
            {
                try
                {
                    session.Send("Q");
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"【位移】发送心跳失败->DtuCode: {session.DtuCode} Reason: {e.Message}");

                    try { session.Close(); } catch { }

                    break;
                }

                Thread.Sleep(DeviceUtil.MOVE_HEARTBEAT_INTERVAL);
            }
        }

        /// <summary>
        /// 接收并解析报文
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        protected void ReceiveCommand(MoveSession session, MoveInfo requestInfo)
        {
            string data;

            try { data = Encoding.ASCII.GetString(requestInfo.ReadBuffer); } 
            catch 
            {
                LogUtil.LogError($"【位移】设备数据读取失败");
                return;
            }

            //序列号格式
            if (Regex.IsMatch(data, "\\$HB.*\\*HB"))
            {
                var dtuCode = DeviceUtil.GetMoveDtuCode(data);

                if (dtuCode == session.DtuCode)
                    return;

                if (!session.CheckDtu(dtuCode)) 
                {
                    LogUtil.LogError($"【位移】已连接设备序列号验证失败->ReceivedCode: {dtuCode} ");
                    try { session.Close(); } catch { }
                    return;
                }

                if (session.Devices.Count == 0)
                {
                    LogUtil.LogError($"【位移】已连接Dtu下未配置位移设备->ReceivedCode: {dtuCode} ");
                    try { session.Close(); } catch { }
                    return;
                }

                LogUtil.LogImportant($"【位移】设备序列号更新成功->DtuCode: {session.DtuCode} ");

                return;
            }

            if (!Regex.IsMatch(data, "\\$HUASI,GET,TMDATA,([^,]*,){11}.*\\*[A-Fa-f0-9]{2}"))
            {
                LogUtil.LogError($"【位移】设备数据格式错误->ReceivedData: {Encoding.ASCII.GetString(requestInfo.ReadBuffer)} ");
                return;
            }

            if (!DeviceUtil.CheckXor(data))
            {
                LogUtil.LogError($"【位移】设备返回数据校验错误->DtuCode: {session.DtuCode} Data: {Encoding.ASCII.GetString(requestInfo.ReadBuffer)}");
                return;
            }

            var moveDatas = DeviceUtil.ParseMove(data, session.DtuCode);

            /*存Data*/
            StoreData(session, moveDatas);
        }

        /// <summary>
        /// 存数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="moveDatas"></param>
        private void StoreData(MoveSession session, List<MoveInfo> moveDatas)
        {
            var moveSettings = session.Devices;

            if (moveSettings == null)
                return;

            var list = new List<AdminMoveData>();
            var warnList = new List<BaseWarnData>();

            foreach(var moveSetting in moveSettings)
            {
                var moveData = moveDatas
                    .Where(d => d.DeviceCode == moveSetting.device_code)
                    .FirstOrDefault();

                if (moveData == null)
                    continue;

                double distance = 0.0;
                string direction = "";
                if (moveSetting.basex_data != null && moveSetting.basey_data != null)
                {
                    distance = DeviceUtil.MoveDistance(
                        (float)moveData.DeviceXData,
                        (float)moveData.DeviceYData,
                        (float)moveSetting.basex_data,
                        (float)moveSetting.basey_data
                    );

                    direction = DeviceUtil.MoveDirection(
                        (float)moveData.DeviceXData,
                        (float)moveData.DeviceYData,
                        (float)moveSetting.basex_data,
                        (float)moveSetting.basey_data,
                        (float)(moveSetting.default_angle??0.0)
                    );
                }

                double distanceOneDay = 0.0;

                var dataYesterday = session.BaseValuesYesterday?
                    .Where(d => d.device_code == moveSetting.device_code)
                    .FirstOrDefault();

                if (dataYesterday != null)
                {
                    distanceOneDay = DeviceUtil.MoveDistance(
                        (float)moveData.DeviceXData,
                        (float)moveData.DeviceYData,
                        (float)dataYesterday.device_x_data,
                        (float)dataYesterday.device_y_data
                    );
                }

                string remarks = "";
                bool isToWarn = false;

                if (moveSetting.move_alarm_enable == true)
                {
                    if (distanceOneDay >= moveSetting.move_threshold)
                    {
                        remarks = remarks + "单日位移报警;";
                        isToWarn = true;
                    }             
                }

                if (moveSetting.all_move_alarm_enable == true)
                {
                    if (distance >= moveSetting.all_move_threshold)
                    {
                        remarks = remarks + "累计位移报警;";
                        isToWarn = true;
                    }
                }

                //to do 其他报警

                var pdd = new AdminMoveData()
                {
                    id = Snowflake.GetUId(),
                    pipeline_id = session.PipelineId,
                    pipeline_name = session.PipelineName,
                    dtu_id = session.DtuId,
                    dtu_name = session.DtuName,
                    device_id = moveSetting.id,
                    device_name = moveSetting.device_name,
                    device_code = moveSetting.device_code,
                    device_warn_data = (float?)distance,
                    device_location = moveSetting.device_location,
                    device_voltage = moveData.Voltage,
                    device_temprature = moveData.Tempratrue,
                    device_direction = direction,
                    device_x_speed = moveData.DeviceXSpeed,
                    device_y_speed = moveData.DeviceYSpeed,
                    device_z_speed = moveData.DeviceZSpeed,
                    device_x_data = moveData.DeviceXData,
                    device_y_data = moveData.DeviceYData,
                    device_z_data = moveData.DeviceZData,
                    device_angle = moveData.DeviceAngle,
                    is_deal = false,
                    is_to_warn = isToWarn,
                    date_create = DateTime.Now.ToString("yyyy-MM-dd"),
                    gmt_create = DateTime.Now,
                    deleted = false,
                    remarks = remarks,

                };

                list.Add(pdd);

                //if (DeviceUtil.MOVE_WECHAT_ON == true && isToWarn)
                //    WechatUtil.SendMessage($"{pdd.dtu_name}-{pdd.device_name}", pdd.remarks, WechatUtil.MOVE_CODE);

                //统一报警表
                if (pdd.is_to_warn == false) continue;

				var alreadyWarnedToday = DbContext.DbClient.Queryable<BaseWarnData>()
				.Where(o => o.deleted == false
					&& o.device_id == pdd.device_id
					&& o.alarm_type == 1
					&& o.gmt_create != null
					&& DateTime.Now.Date == ((DateTime)o.gmt_create).Date)
				.Any();

                // 位移固定一天至多一条普通报警
				if (alreadyWarnedToday) continue;

				var warnData = new BaseWarnData()
                {
                    id = Snowflake.GetUId(),
                    device_id = pdd.device_id,
                    device_name= pdd.device_name,
                    device_code = pdd.device_code,
                    c_id = pdd.id,
                    pipeline_id = pdd.pipeline_id,
                    pipeline_name = pdd.pipeline_name,
                    dtu_id = pdd.dtu_id,
                    dtu_name = pdd.dtu_name,
                    dtu_type = 2,//位移
                    alarm_type = 1, //正常告警
                    alarm_content = pdd.remarks,
                    is_deal = false,
                    gmt_create = DateTime.Now,
                    deleted = false,
                };

				warnList.Add(warnData);

                // 发送微信公众号
				if (DeviceUtil.MOVE_WECHAT_ON == true && pdd.is_to_warn == true)
				    WechatUtil.SendMessage($"{pdd.dtu_name}-{pdd.device_name}", pdd.remarks, WechatUtil.MOVE_CODE);
			}

			try
            {
                DbContext.DbClient.Insertable(list).ExecuteCommand();
            }
            catch
            {
                LogUtil.LogError($"【位移】写入AdminMoveData数据库错误->dtuCOde: {session.DtuCode} dtuName: {session.DtuName}");
            }

			try
			{
				if (warnList.Count > 0) DbContext.DbClient.Insertable(warnList).ExecuteCommand();
			}
			catch
			{
				LogUtil.LogError($"【位移】写入BaseWarnData数据库错误->dtuCOde: {session.DtuCode} dtuName: {session.DtuName}");
			}
		}

        /// <summary>
        /// 打印服务器状态至控制台
        /// </summary>
        public void PrintServerState()
        {
            var sessions = this.GetAllSessions();
            LogUtil.LogState($"【位移】服务状态->连接总数: {sessions.Count()} ");

            var table = new ConsoleTable("No", "Dtu Code",  "IP", "Start Time", "Refresh Time", "Dtu Name");

            int i = 1;
            foreach (var session in sessions)
            {
                table.AddRow(i++,
                    session.DtuCode,
                    $"{session.RemoteEndPoint.Address}:{session.RemoteEndPoint.Port} ",
                    session.StartTime,
                    session.LastTimeRefreshData,
                    session.DtuName);
            }

            table.Write(Format.Alternative);
        }
    }
}
