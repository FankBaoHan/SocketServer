using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xinao.SocketServer.Info;
using Xinao.SocketServer.Server;
using Xinao.SocketServer.Session;

namespace Xinao.SocketServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var gaugePort = System.Configuration.ConfigurationManager.AppSettings["gaugePort"].ToInt32();
            var protectPort = System.Configuration.ConfigurationManager.AppSettings["protectPort"].ToInt32();
            var movePort = System.Configuration.ConfigurationManager.AppSettings["movePort"].ToInt32();

            var gaugeServer = new GaugeServer();
            var protectServer = new ProtectServer();
            var moveServer = new MoveServer();

            //Setup the SocketServer
            if (!gaugeServer.Setup(gaugePort) || !protectServer.Setup(protectPort) || !moveServer.Setup(movePort))
            {
                Console.WriteLine("服务器初始化失败,请检查端口是否被占用!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine();

            //Try to start the SocketServer
            if (!gaugeServer.Start() || !protectServer.Start() || !moveServer.Start())
            {
                Console.WriteLine("服务器启动失败!");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                if (Console.ReadKey().KeyChar == 's')
                {
                    gaugeServer.PrintServerState();
                    protectServer.PrintServerState();
                    moveServer.PrintServerState();
                }

                continue;
            }
        }
    }
}
