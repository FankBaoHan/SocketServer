using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Utils
{
    public class WechatUtil
    {
        private static string AccessToken = string.Empty;

        private static DateTime RefreshTokenTime = DateTime.MinValue;

        private static double EXPIRE_MILLSECONDS = 1000 * 60 * 60 * 2;

        private static string SendTemplateMessage(string openId, string templateId, object data, string url, string topColor = "#173177")
        {
            var postUrl = string
                .Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}"
                , GetAccessToken());

            var msgData = new
            {
                touser = openId,
                template_id = templateId,
                topcolor = topColor,
                url = url,
                data = data
            };

            var postData = JsonConvert.SerializeObject(msgData);

            var result = HttpUtil.PostBody(postUrl, postData);

            var jb = JObject.Parse(result);
            return jb["errcode"].ToString() == "0" ? "ok" : jb["errmsg"].ToString();
        }

        private static string GetAccessToken()
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                TryRefreshToken();
            }
            else
            {
                if ((DateTime.Now - RefreshTokenTime).TotalMilliseconds >= EXPIRE_MILLSECONDS)
                    TryRefreshToken();
            }

            return AccessToken;
        }

        private static void TryRefreshToken()
        {

        }
    }
}
