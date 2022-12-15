using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xinao.SocketServer.Info;

namespace Xinao.SocketServer.Filter
{
    public class MoveFilter : IReceiveFilter<MoveInfo>
    {
        public int LeftBufferSize { get; }

        public IReceiveFilter<MoveInfo> NextReceiveFilter { get; }

        public FilterState State { get; }

        public MoveInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            var moveInfo = new MoveInfo();
            moveInfo.ReadBuffer = new byte[length];
            Buffer.BlockCopy(readBuffer, offset, moveInfo.ReadBuffer, 0, length);

            rest = 0;

            return moveInfo;
        }

        public void Reset()
        {
        }
    }
}
