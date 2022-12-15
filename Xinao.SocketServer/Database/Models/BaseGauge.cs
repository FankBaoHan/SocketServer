using SqlSugar;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("base_gauge")]
    public class BaseGauge
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
        public string gauge_name { get; set; }

        /// <summary>
        /// 设备编号:slave ID
        /// </summary>
        public int? gauge_code { get; set; }

        /// <summary>
        /// 参照设备：1是；2否
        /// </summary>
        public int? is_base { get; set; }

        /// <summary>
        /// 掉线延时。单位：秒
        /// </summary>
        public int? offline_time { get; set; }

        /// <summary>
        /// 埋设沉降值：非基准点设备返回的数据减去基准点设备返回数据后，再和埋设沉降值进行比较。比较后的结果和报警阈值范围进行比较，超出范围则生成报警数据。
        /// </summary>
        public float? base_data { get; set; }

        /// <summary>
        /// 沉降报警值（日变化报警阈值）
        /// </summary>
        public float? to_warn_data { get; set; }

        /// <summary>
        /// 日变化报警开关（是否报警）
        /// </summary>
        public bool? is_to_warn { get; set; }

        /// <summary>
        /// 累计变化报警阈值
        /// </summary>
        public float? all_to_warn_data { get; set; }

        /// <summary>
        /// 累计变化报警开关
        /// </summary>
        public bool? all_is_to_warn { get; set; }

        /// <summary>
        /// 设备地址：经纬度
        /// </summary>
        public string gauge_location { get; set; }

        /// <summary>
        /// 设备状态：1：启用；2：停用；3：掉线
        /// </summary>
        public int? status { get; set; }

        /// <summary>
        /// 0正常1删除
        /// </summary>
        public bool? deleted { get; set; }
    }
}