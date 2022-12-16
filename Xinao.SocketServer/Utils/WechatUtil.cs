using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Runtime.InteropServices;

namespace Xinao.SocketServer.Utils
{
    public class WechatUtil
    {
        public static string POST_URL = System.Configuration.ConfigurationManager.AppSettings["wechatUrl"];//API地址

        public static readonly short GAUGE_CODE = 1;
        public static readonly short MOVE_CODE = 2;

        public static bool SendMessage(string title, string content, short type)
        {
            var client = new RestClient(POST_URL);

            var request = new RestRequest()
            {
                Method = Method.Post,
                Timeout = 10000
            };

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            request
                .AddParameter("title", title)
                .AddParameter("content", content)
                .AddParameter("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .AddParameter("type", type.ToString());

            RestResponse response = null;
            try
            {
                response = client.Execute(request);
            }
            catch (Exception e)
            {
                Log(type, $"发送微信公众号消息失败->设备信息:{title} error:{e.Message}");
                return false;
            }

            var ro = JObject.Parse(response.Content);
            var code = ro["code"]?.ToString();

            if (code == "200")
            {
                Log(type, $"发送微信公众号消息完成->设备信息:{title}");
                return true;
            }

            Log(type, $"发送微信公众号消息失败->设备信息:{title} content:{response?.Content}");
            return false;
        }

        private static void Log(short type, string content)
        {
            if (GAUGE_CODE == type) { LogUtil.LogMoveData($"【沉降】{content}"); }
            if (MOVE_CODE == type) { LogUtil.LogMoveData($"【位移】{content}"); }
        }
    }
}
