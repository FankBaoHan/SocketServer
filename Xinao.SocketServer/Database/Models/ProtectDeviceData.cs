using SqlSugar;
using System;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("protect_device_data")]
    public class ProtectDeviceData
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string id { get; set; }

        /// <summary>
        /// 阴保设备ID
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 类型：
        ///1：阴保电位 
        ///2：自然电位
        ///3：直流电流
        ///4：交流干扰电压
        ///5：交流杂散电压
        /// </summary>
        public int? types { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime? gather_time { get; set; }

        /// <summary>
        /// 采集日期 yyyy/MM/dd
        /// </summary>
        public string gather_date { get; set; }

        /// <summary>
        /// 正常范围下限
        /// </summary>
        public float? low_limit { get; set; }

        /// <summary>
        /// 正常范围上限
        /// </summary>
        public float? high_limit { get; set; }

        /// <summary>
        /// 转换后显示值
        /// </summary>
        public float? actual_value { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string device_name { get; set; }

        /// <summary>
        /// 管道名称
        /// </summary>
        public string pipeline_name { get; set; }

        /// <summary>
        /// DTU名称
        /// </summary>
        public string dtu_name { get; set; }

        /// <summary>
        /// 是否报警 0:否 1:是
        /// </summary>
        public bool? is_alarm { get; set; }

        /// <summary>
        /// 所属管道 ID
        /// </summary>
        public string pipeline_id { get; set; }

        /// <summary>
        /// 所属DTU ID
        /// </summary>
        public string dtu_id { get; set; }

        /// <summary>
        /// 0正常1删除
        /// </summary>
        public bool? deleted { get; set; }

        /// <summary>
        /// 报警是否处理；0未处理；1已处理
        /// </summary>
        public bool? is_deal { get; set; }

        /// <summary>
        /// 报警内容
        /// </summary>
        public string alarm_comment { get; set; }
    }
}