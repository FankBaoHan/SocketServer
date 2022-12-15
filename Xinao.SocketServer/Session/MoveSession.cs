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
    public class MoveSession : AppSession<MoveSession, MoveInfo>
    {
        public int TimesTry2Connect = 1;//验证dtucode数据次数

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

        private List<BaseMoveDevice> devices;

        /// <summary>
        /// Dtu下配置的位移设备
        /// </summary>
        public List<BaseMoveDevice> Devices
        {
            get
            {
                var timeDiff = DateTime.Now - LastTimeRefreshData;

                //减少IO次数
                if (!DeviceUtil.IS_MOVE_DATABASE_CACHE_ON
                    || timeDiff.TotalSeconds >= DeviceUtil.MOVE_REFRESH_EXPIRATION_TIME)
                {
                    try { RefreshInfo(DtuCode); } catch { LogUtil.LogError($"【位移】更新配置错误->functionName: GauSession.RefreshInfo"); }
                }

                return devices;
            }

            set { devices = value;}
        }

        /// <summary>
        /// 昨日位移值（非液位） 用于日位移报警
        /// </summary>
        public List<AdminMoveData> BaseValuesYesterday { get; set; }

        /// <summary>
        /// 刷新昨日位移值时间
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
            lock (this)
            {
                LogUtil.LogMoveData($"【位移】发送->DtuCode: {DtuCode} Data: {BitConverter.ToString(data)}");
                base.Send(data, offset, length);
                Thread.Sleep(200);
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
                .Where(o => o.dtu_code == sn && o.deleted == false && o.types == 2)
                .First();

            if (dtu != null)
            {
                var devices = db.Queryable<BaseMoveDevice>()
                .Where(o => o.dtu_code == sn && o.deleted == false && o.status != 2)
                .ToList();

                DtuCode = dtu.dtu_code;
                DtuId = dtu.id;
                DtuName = dtu.dtu_name;
                PipelineId = dtu.pipeline_id;
                PipelineName = dtu.pipeline_name;
                Devices = devices;

                LastTimeRefreshData = DateTime.Now;

                if (BaseValueYesterdayRefreshDate != DateTime.Now.ToString("yyyy-MM-dd"))
                    RefreshBaseValueYesterday();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 刷新昨日位移数据
        /// </summary>
        /// <param name="dtuId"></param>
        private void RefreshBaseValueYesterday()
        {
            if (this.Devices.Count == 0)
                return;

            var db = DbContext.DbClient;

            var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var ids = Devices.Select(d => d.id).ToList();

            //每组取1条
            var baseDatas = db.Queryable<AdminMoveData>()
                .Where(d => d.dtu_id == DtuId && d.date_create == yesterday)
                .OrderBy(d => d.gmt_create, SqlSugar.OrderByType.Desc)
                .Take(1)
                .PartitionBy(d => ids.Contains(d.id))
                .ToList();

            if (baseDatas.Count == 0)
                return;

            this.BaseValuesYesterday = baseDatas;
            this.BaseValueYesterdayRefreshDate = DateTime.Now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 验证Dtu是否存在
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <returns></returns>
        public bool CheckDtu(string sn)
        {
            if (string.IsNullOrEmpty(sn))
                return false;

            return RefreshInfo(sn);
        }
    }
}
