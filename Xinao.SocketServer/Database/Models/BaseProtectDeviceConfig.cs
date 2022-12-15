using SqlSugar;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("base_protect_device_config")]
    public class BaseProtectDeviceConfig
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string id { get; set; }

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
        /// 报警开关：0：关  1：开
        /// </summary>
        public bool? alarm_enable { get; set; }

        /// <summary>
        /// 正常范围下限
        /// </summary>
        public float? low_limit { get; set; }

        /// <summary>
        /// 正常范围上限
        /// </summary>
        public float? high_limit { get; set; }

        /// <summary>
        /// 最小原始值
        /// </summary>
        public int? low_origin { get; set; }

        /// <summary>
        /// 最大原始值
        /// </summary>
        public int? high_origin { get; set; }

        /// <summary>
        /// 最小实际值
        /// </summary>
        public float? low_actual { get; set; }

        /// <summary>
        /// 最大实际值
        /// </summary>
        public float? high_actual { get; set; }

        /// <summary>
        /// 比例系数
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// 偏移
        /// </summary>
        public string b { get; set; }

        /// <summary>
        /// 阴保设备ID
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 0正常1删除
        /// </summary>
        public bool? deleted { get; set; }
    }
}