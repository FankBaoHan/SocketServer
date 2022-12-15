using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xinao.SocketServer.Info;
using static System.Collections.Specialized.BitVector32;
using Xinao.SocketServer.Utils;
using System.Data;
using Xinao.SocketServer.Database;
using Xinao.SocketServer.Database.Models;
using System.Threading;

namespace Xinao.SocketServer.Session
{
    public class GaugeSession : AppSession<GaugeSession, GaugeInfo>
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

        private List<BaseGauge> gauges;
        /// <summary>
        /// dtu下配置的沉降设备
        /// </summary>
        public List<BaseGauge> Gauges
        {
            get 
            {
                var timeDiff = DateTime.Now - LastTimeRefreshData;

                //减少IO次数
                if (!DeviceUtil.IS_GAUGE_DATABASE_CACHE_ON
                    || timeDiff.TotalSeconds >= DeviceUtil.GAUGE_REFRESH_EXPIRATION_TIME)
                {
                    try { RefreshInfo(DtuCode); } catch { LogUtil.LogError($"【沉降】更新配置错误->functionName: GauSession.RefreshInfo"); }
                }

                return gauges;
            }

            set { gauges = value; }
            
        }

        /// <summary>
        /// dtu下基准设备的液位
        /// </summary>
        public float? BenchmarkLevel { get; set; }

        /// <summary>
        /// 昨日沉降值（非液位） 用于日沉降报警
        /// </summary>
        public float? BaseValueYesterday { get; set; }

        /// <summary>
        /// 刷新昨日沉降值时间
        /// </summary>
        public string BaseValueYesterdayRefreshDate { get; set; }

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
            lock(this)
            {
                LogUtil.LogGaugeData($"【沉降】发送->DtuCode: {DtuCode} Data: {BitConverter.ToString(data)}");
                base.Send(data, offset, length);
                Thread.Sleep(DeviceUtil.GAUGE_SEND_SLEEP_TIME);
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
                .Where(o => o.dtu_code == sn && o.deleted == false && o.types == 1)
                .First();

            if (dtu != null)
            {
                var gauges = db.Queryable<BaseGauge>()
                .Where(o => o.dtu_code == sn && o.deleted == false && o.status != 2)
                .ToList();

                DtuCode = dtu.dtu_code;
                DtuId = dtu.id;
                DtuName = dtu.dtu_name;
                PipelineId = dtu.pipeline_id;
                PipelineName = dtu.pipeline_name;
                Gauges = gauges;
                Frequence = dtu.collect_rate ?? DeviceUtil.GAUGE_DTU_GATHER_DEFALUT_INTERVAL;

                LastTimeRefreshData = DateTime.Now;

                if (BaseValueYesterdayRefreshDate != DateTime.Now.ToString("yyyy-MM-dd"))
                    RefreshBaseValueYesterday(dtu.id);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 刷新昨日沉降数据
        /// </summary>
        /// <param name="dtuId"></param>
        private void RefreshBaseValueYesterday(string dtuId)
        {
            if (string.IsNullOrEmpty(dtuId))
                return;

            var db = DbContext.DbClient;

            var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            var baseValue = db.Queryable<AdminGaugeData>()
                .Where(d => d.dtu_id == dtuId && d.date_create == yesterday)
                .OrderBy(d => d.gmt_create, SqlSugar.OrderByType.Desc)
                .Select(d => d.subside_data)
                .First();

            if (baseValue == null)
                return;

            this.BaseValueYesterday = baseValue;
            this.BaseValueYesterdayRefreshDate = DateTime.Now.ToString("yyyy-MM-dd");
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
