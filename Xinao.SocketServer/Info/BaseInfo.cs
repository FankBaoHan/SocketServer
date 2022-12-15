using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Info
{
    public class BaseInfo : IRequestInfo
    {
        /// <summary>
        /// 原始byte数据
        /// </summary>
        public byte[] ReadBuffer { get; set; }

        /// <summary>
        /// 无意义
        /// </summary>
        public string Key {get;set;}
    }
}
