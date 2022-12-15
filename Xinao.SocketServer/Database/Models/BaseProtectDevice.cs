using SqlSugar;
using System.Collections.Generic;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("base_protect_device")]
    public class BaseProtectDevice
    {
        /// <summary>
        /// 唯一标识：阴保设备表
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string id { get; set; }

        /// <summary>
        /// Dtu编号
        /// </summary>
        public string dtu_code { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string device_name { get; set; }

        /// <summary>
        /// 设备编号：SLAVE ID。同一DTU下编号唯一
        /// </summary>
        public string device_code { get; set; }

        /// <summary>
        /// 掉线延时。单位：秒
        /// </summary>
        public int? offline_time { get; set; }

        /// <summary>
        /// 设备地址：经纬度
        /// </summary>
        public string device_location { get; set; }

        /// <summary>
        /// 设备状态：1：启用；2：停用；3：掉线
        /// </summary>
        public int? status { get; set; }

        /// <summary>
        /// 设备的各项配置参数
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<BaseProtectDeviceConfig> configs { get; set; }

        /// <summary>
        /// 0正常1删除
        /// </summary>
        public bool? deleted { get; set; }
    }
}