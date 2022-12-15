using SuperSocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Utils
{
    public class LogUtil
    {
        public static bool IS_LOG_GAUGE_DATA = System.Configuration.ConfigurationManager.AppSettings["gaugeLogData"].ToBoolean();
        public static bool IS_LOG_PROTECT_DATA = System.Configuration.ConfigurationManager.AppSettings["protectLogData"].ToBoolean();
        public static bool IS_LOG_MOVE_DATA = System.Configuration.ConfigurationManager.AppSettings["moveLogData"].ToBoolean();
        public static void Log(string content)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{DateTime.Now} {content}");
        }

        public static void LogError(string content) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now} {content}");
        }

        public static void LogImportant(string content)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{DateTime.Now} {content}");
        }

        public static void LogGaugeData(string content) 
        {
            if (IS_LOG_GAUGE_DATA)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{DateTime.Now} {content}");
            }
        }

        public static void LogProtectData(string content)
        {
            if (IS_LOG_PROTECT_DATA)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{DateTime.Now} {content}");
            }
        }

        public static void LogMoveData(string content)
        {
            if (IS_LOG_MOVE_DATA)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{DateTime.Now} {content}");
            }
        }

        public static void LogState(string content)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{DateTime.Now} {content}");
        }
    }
}
