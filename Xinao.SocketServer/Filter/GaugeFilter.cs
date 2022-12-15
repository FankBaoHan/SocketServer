using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Xinao.SocketServer.Info;

namespace Xinao.SocketServer.Filter
{
    public class GaugeFilter : IReceiveFilter<GaugeInfo>
    {

        public int LeftBufferSize { get; }

        public IReceiveFilter<GaugeInfo> NextReceiveFilter { get; }

        public FilterState State { get; }

        public GaugeInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            var gaugeInfo = new GaugeInfo();
            gaugeInfo.ReadBuffer = new byte[length];
            Buffer.BlockCopy(readBuffer, offset, gaugeInfo.ReadBuffer, 0, length);

            rest = 0;

            return gaugeInfo;
        }

        public void Reset()
        {
        }
    }
}
