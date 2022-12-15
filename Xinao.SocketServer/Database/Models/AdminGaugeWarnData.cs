using SqlSugar;
using System;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("admin_gauge_warn_data")]
    public class AdminGaugeWarnData
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string id { get; set; }

        /// <summary>
        /// 管道id
        /// </summary>
        public string pipeline_id { get; set; }

        /// <summary>
        /// 管道名称
        /// </summary>
        public string pipeline_name { get; set; }

        /// <summary>
        /// dtu
        /// </summary>
        public string dtu_id { get; set; }

        /// <summary>
        /// dtu
        /// </summary>
        public string dtu_name { get; set; }

        /// <summary>
        /// 沉降仪id
        /// </summary>
        public string gauge_id { get; set; }

        /// <summary>
        /// 沉降仪名称
        /// </summary>
        public string gauge_name { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public System.Int32? gauge_code { get; set; }

        /// <summary>
        /// 设备地址：经纬度
        /// </summary>
        public string gauge_location { get; set; }

        /// <summary>
        /// 沉降值
        /// </summary>
        public float? subside_data { get; set; }

        /// <summary>
        /// 水压
        /// </summary>
        public float? gauge_pressure { get; set; }

        /// <summary>
        /// 水温
        /// </summary>
        public float? gauge_temp { get; set; }

        /// <summary>
        /// 采样时间
        /// </summary>
        public DateTime? get_time { get; set; }

        /// <summary>
        /// 最晚报警时间
        /// </summary>
        public DateTime? end_time { get; set; }

        /// <summary>
        /// 该报警是否为通知数据。0不是，1是
        /// </summary>
        public bool? is_to_warn { get; set; }

        /// <summary>
        /// 字符串日期：年-月-日
        /// </summary>
        public string date_create { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? gmt_create { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool? deleted { get; set; }
    }
}