using SqlSugar;
using System;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("admin_move_data")]
    public class AdminMoveData
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
        /// 位移机id
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 位移机名称
        /// </summary>
        public string device_name { get; set; }

        /// <summary>
        /// 位移机编号
        /// </summary>
        public string device_code { get; set; }

        /// <summary>
        /// 测算后位移值
        /// </summary>
        public float? device_warn_data { get; set; }

        /// <summary>
        /// 设备地址：经纬度
        /// </summary>
        public string device_location { get; set; }

        /// <summary>
        /// 电压
        /// </summary>
        public float? device_voltage { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public float? device_temprature { get; set; }

        /// <summary>
        /// 位移方向
        /// </summary>
        public string device_direction { get; set; }

        /// <summary>
        /// 加速度x
        /// </summary>
        public float? device_x_speed { get; set; }

        /// <summary>
        /// 加速度y
        /// </summary>
        public float? device_y_speed { get; set; }

        /// <summary>
        /// 加速度z
        /// </summary>
        public float? device_z_speed { get; set; }

        /// <summary>
        /// 坐标x
        /// </summary>
        public float? device_x_data { get; set; }

        /// <summary>
        /// 坐标y
        /// </summary>
        public float? device_y_data { get; set; }

        /// <summary>
        /// 坐标z
        /// </summary>
        public float? device_z_data { get; set; }

        /// <summary>
        /// 旋转角
        /// </summary>
        public float? device_angle { get; set; }

        /// <summary>
        /// 是否处理；0未处理；1已处理
        /// </summary>
        public bool? is_deal { get; set; }

        /// <summary>
        /// 是否为微信通知数据。0不是，1是
        /// </summary>
        public bool? is_to_warn { get; set; }

        /// <summary>
        /// 字符串日期：年-月-日
        /// </summary>
        public string date_create { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? gmt_deal { get; set; }

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