using SqlSugar;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("base_move_device")]
    public class BaseMoveDevice
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string id { get; set; }

        /// <summary>
        /// 所属管道
        /// </summary>
        public string pipeline_id { get; set; }

        /// <summary>
        /// 管道名称
        /// </summary>
        public string pipeline_name { get; set; }

        /// <summary>
        /// 所属DTU
        /// </summary>
        public string dtu_id { get; set; }

        /// <summary>
        /// DTU名称
        /// </summary>
        public string dtu_name { get; set; }

        /// <summary>
        /// DTU编码
        /// </summary>
        public string dtu_code { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string device_name { get; set; }

        /// <summary>
        /// 设备序列号
        /// </summary>
        public string device_code { get; set; }

        /// <summary>
        /// 掉线延时。单位：秒
        /// </summary>
        public int? offline_time { get; set; }

        /// <summary>
        /// 接收数据时间间隔。单位：秒
        /// </summary>
        public int? get_data_time { get; set; }

        /// <summary>
        /// 深度：设备埋设深度
        /// </summary>
        public float? depth { get; set; }

        /// <summary>
        /// x轴基准坐标值
        /// </summary>
        public float? basex_data { get; set; }

        /// <summary>
        /// y轴基准坐标值
        /// </summary>
        public float? basey_data { get; set; }

        /// <summary>
        /// z轴基准坐标值
        /// </summary>
        public float? basez_data { get; set; }

        /// <summary>
        /// 设备地址：经纬度
        /// </summary>
        public string device_location { get; set; }

        /// <summary>
        /// 位移报警值（日变化）
        /// </summary>
        public float? move_threshold { get; set; }

        /// <summary>
        /// 位移报警开关（日变化）
        /// </summary>
        public bool? move_alarm_enable { get; set; }

        /// <summary>
        /// 累计变化位移报警值
        /// </summary>
        public float? all_move_threshold { get; set; }

        /// <summary>
        /// 累计位移报警开关
        /// </summary>
        public bool? all_move_alarm_enable { get; set; }

        /// <summary>
        /// 埋设基准黑线朝向，黑线对X轴（与正北的顺时针夹角 0~360°）
        /// </summary>
        public float? default_angle { get; set; }

        /// <summary>
        /// X轴加速度报警基准值
        /// </summary>
        public float? xacc_threshold { get; set; }

        /// <summary>
        /// X轴加速度报警开关
        /// </summary>
        public bool? xacc_alarm_enable { get; set; }

        /// <summary>
        /// Y轴加速度报警基准值
        /// </summary>
        public float? yacc_threshold { get; set; }

        /// <summary>
        /// Y轴加速度报警开关
        /// </summary>
        public bool? yacc_alarm_enable { get; set; }

        /// <summary>
        /// 旋转角报警基准值
        /// </summary>
        public float? angle_threshold { get; set; }

        /// <summary>
        /// 旋转角报警开关
        /// </summary>
        public bool? angle_alarm_enable { get; set; }

        /// <summary>
        /// 设备状态：1：启用；2：停用；3：掉线
        /// </summary>
        public int? status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }

        /// <summary>
        /// 0正常1删除
        /// </summary>
        public bool? deleted { get; set; }
    }
}