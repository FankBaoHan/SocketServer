using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Info
{
    public class GaugeInfo : BaseInfo
    {
        /// <summary>
        /// modbus从站地址
        /// </summary>
        public int SlaveId { get; set; }

        /// <summary>
        /// 水温
        /// </summary>
        public float Temprature { get; set; }

        /// <summary>
        /// 水压
        /// </summary>
        public float Pressure { get; set; }

        /// <summary>
        /// 液位
        /// </summary>
        public float Level { get; set; }

    }
}
