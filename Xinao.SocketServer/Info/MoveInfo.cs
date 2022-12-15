using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Info
{
    public class MoveInfo : BaseInfo
    {
        /// <summary>
        /// 设备标志
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// 电压
        /// </summary>
        public float? Voltage { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public float? Tempratrue { get; set; }

        /// <summary>
        /// 加速度x
        /// </summary>
        public float? DeviceXSpeed { get; set; }

        /// <summary>
        /// 加速度y
        /// </summary>
        public float? DeviceYSpeed { get; set; }

        /// <summary>
        /// 加速度z
        /// </summary>
        public float? DeviceZSpeed { get; set; }

        /// <summary>
        /// x轴坐标
        /// </summary>
        public float? DeviceXData { get; set; }

        /// <summary>
        /// y轴坐标
        /// </summary>
        public float? DeviceYData { get; set; }

        /// <summary>
        /// z轴坐标
        /// </summary>
        public float? DeviceZData { get; set; }

        /// <summary>
        /// 旋转角
        /// </summary>
        public float? DeviceAngle { get; set; }
    }
}
