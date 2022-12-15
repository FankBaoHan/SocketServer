using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xinao.SocketServer.Info;

namespace Xinao.SocketServer.Filter
{
    internal class ProtectFilter : IReceiveFilter<ProtectInfo>
    {
        public int LeftBufferSize { get; }

        public IReceiveFilter<ProtectInfo> NextReceiveFilter { get; }

        public FilterState State { get; }

        public ProtectInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            var protectInfo = new ProtectInfo();
            protectInfo.ReadBuffer = new byte[length];
            Buffer.BlockCopy(readBuffer, offset, protectInfo.ReadBuffer, 0, length);

            rest = 0;

            return protectInfo;
        }

        public void Reset()
        {
        }
    }
}
