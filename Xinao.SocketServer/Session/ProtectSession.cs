using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xinao.SocketServer.Database;
using Xinao.SocketServer.Database.Models;
using Xinao.SocketServer.Info;
using Xinao.SocketServer.Utils;

namespace Xinao.SocketServer.Session
{
    public class ProtectSession : AppSession<ProtectSession, ProtectInfo>
    {
        /// <summary>
        /// DTU序列号 设备连接后首包数据
        /// </summary>
        public string DtuCode { get; set; }

        /// <summary>
        /// Dtu名称
        /// </summary>
        public string DtuName { get; set; }

        /// <summary>
        /// Dtu Id
        /// </summary>
        public string DtuId { get; set; }

        /// <summary>
        /// 管道Id
        /// </summary>
        public string PipelineId { get; set; }

        /// <summary>
        /// 管道名称
        /// </summary>
        public string PipelineName { get; set; }

        /// <summary>
        /// 采集频率 秒
        /// </summary>
        public int Frequence { get; set; } = 60;

        private BaseProtectDevice device;
        /// <summary>
        /// Dtu所带的设备
        /// </summary>
        public BaseProtectDevice Device 
        { 
            get
            {
                var timeDiff = DateTime.Now - LastTimeRefreshData;

                //减少IO次数
                if (!DeviceUtil.IS_PROTECT_DATABASE_CACHE_ON
                    || timeDiff.TotalSeconds >= DeviceUtil.PROTECT_REFRESH_EXPIRATION_TIME)
                {
                    try { RefreshInfo(DtuCode); } catch { LogUtil.LogError($"【阴保】更新配置错误->functionName: GauSession.RefreshInfo"); }
                }

                return device;
            }

            set
            {
                device = value;
            }
        }

        /// <summary>
        /// 上次更新数据时间
        /// </summary>
        public DateTime LastTimeRefreshData { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 向dtu发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] data, int offset, int length)
        {
            lock (this)
            {
                LogUtil.LogProtectData($"【阴保】发送->DtuCode: {DtuCode} Data: {BitConverter.ToString(data)}");
                base.Send(data, offset, length);
                Thread.Sleep(DeviceUtil.PROTECT_SEND_SLEEP_TIME);
            }
        }

        /// <summary>
        /// 从数据库更新配置信息
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <returns></returns>
        private bool RefreshInfo(string sn)
        {
            var db = DbContext.DbClient;

            var dtu = db.Queryable<BaseDtu>()
                .Where(d => d.dtu_code == sn && d.deleted == false && d.types == 3)
                .First();

            if (dtu != null)
            {
                var device = db.Queryable<BaseProtectDevice>()
                    .Where(d => d.dtu_code == sn && d.deleted == false && d.status != 2)
                    .First();

                if (device != null)
                {
                    var configs = db.Queryable<BaseProtectDeviceConfig>()
                    .Where(c => c.device_id == device.id && c.deleted == false)
                    .ToList();

                    device.configs = configs;
                }

                DtuCode = dtu.dtu_code;
                DtuId = dtu.id;
                DtuName = dtu.dtu_name;
                PipelineId = dtu.pipeline_id;
                PipelineName = dtu.pipeline_name;
                Device = device;
                Frequence = dtu.collect_rate ?? DeviceUtil.GAUGE_DTU_GATHER_DEFALUT_INTERVAL;

                LastTimeRefreshData = DateTime.Now;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 验证Dtu是否存在
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <returns></returns>
        public bool CheckDtu(byte[] readBuffer)
        {
            string sn;
            try { sn = Encoding.ASCII.GetString(readBuffer); } catch { return false; }

            return RefreshInfo(sn);
        }
    }
}
