using SqlSugar;
using System;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("admin_gauge_data")]
    public class AdminGaugeData
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
        /// dtuID
        /// </summary>
        public string dtu_id { get; set; }

        /// <summary>
        /// dtu名称
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
        /// 沉降仪编号
        /// </summary>
        public int? gauge_code { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public float? gauge_temp { get; set; }

        /// <summary>
        /// 水压
        /// </summary>
        public float? gauge_pressure { get; set; }

        /// <summary>
        /// 设备地址：经纬度
        /// </summary>
        public string gauge_location { get; set; }

        /// <summary>
        /// 液位：设备直接返回的数值
        /// </summary>
        public float? gauge_data { get; set; }

        /// <summary>
        /// 沉降值：比较后的值
        /// </summary>
        public float? subside_data { get; set; }

        /// <summary>
        /// 是否报警
        /// </summary>
        public bool? is_to_warn { get; set; }

        /// <summary>
        /// 每次读取沉降仪时，同一DTU下赋相同值
        /// </summary>
        public string group_id { get; set; }

        /// <summary>
        /// 字符串日期：年-月-日
        /// </summary>
        public string date_create { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? gmt_create { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? deleted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string remarks { get; set; }
    }
}