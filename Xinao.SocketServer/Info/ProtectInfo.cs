using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Info
{
    public class ProtectInfo : BaseInfo
    {
        /// <summary>
        /// modbus从站地址
        /// </summary>
        public int SlaveId { get; set; }

        /// <summary>
        /// 阴保电位原始值
        /// </summary>
        public short ElectricPotential{ get; set; }

        /// <summary>
        /// 自然电位原始值
        /// </summary>
        public short NaturePotential { get; set; }

        /// <summary>
        /// 直流电流原始值
        /// </summary>
        public short DcCurrent { get; set; }

        /// <summary>
        /// 交流干扰电压
        /// </summary>
        public short AcInterferenceVoltage { get; set; }

        /// <summary>
        /// 交流杂散电压原始值
        /// </summary>
        public short AcStrayVoltage { get; set; }
    }
}
