using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Org.BouncyCastle.Ocsp;
using RestSharp;

namespace Xinao.SocketServer.Utils
{
    public class HttpUtil
    {
        public static string Get(string url, string reqData, Dictionary<string, string> headers = null)
        {
            try
            {
                url = new UriBuilder(url) { Query = reqData }.ToString();

                var client =  new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Get,
                    Timeout = 30000,

                };

                if (Convert.ToBoolean(headers?.Any()))
                    foreach (var header in headers) request.AddHeader(header.Key, header.Value);

                var response = client.Execute(request);

                return response.Content;

            }
            catch (Exception ex)
            {
                Console.WriteLine("HttpUtils Get->" + ex.Message);
                return ex.ToString();
            }
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string reqData, Dictionary<string, string> headers = null)
        {
            try
            {
                url = new UriBuilder(url) { Query = reqData }.ToString();

                var client = new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Get,
                    Timeout = 30000,

                };

                if (Convert.ToBoolean(headers?.Any()))
                    foreach (var header in headers) request.AddHeader(header.Key, header.Value);

                var response = await client.ExecuteAsync(request);

                return response.Content;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// PostBody
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string PostBody(string url, object reqData = null, Dictionary<string, string> headers = null)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Post,
                    Timeout = 30000
                };

                if (Convert.ToBoolean(headers?.Any()))
                    foreach (var header in headers) request.AddHeader(header.Key, header.Value);

                request.AddJsonBody(reqData);

                var response = client.Execute(request);

                Console.WriteLine($"{DateTime.Now} HttpUtils PostBody->{response.Content}");

                return response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("HttpUtils PostBody->" + ex.Message);
                return ex.ToString();
            }

        }

        /// <summary>
        /// Post Json string
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static string PostBody(string url, string jsonStr)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Post,
                    Timeout = 30000
                };

                request.AddStringBody(jsonStr, DataFormat.Json);

                var response = client.Execute(request);

                //Console.WriteLine($"{DateTime.Now} HttpUtils PostBody->{response.Content}");

                return response.Content;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("HttpUtils PostBody->" + ex.Message);
                return ex.ToString();
            }
        }

        /// <summary>
        /// PostBodyAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<string> PostBodyAsync(string url, object reqData = null, Dictionary<string, string> headers = null)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Post,
                    Timeout = 30000,

                };

                if (Convert.ToBoolean(headers?.Any()))
                    foreach (var header in headers) request.AddHeader(header.Key, header.Value);

                request.AddBody(reqData);

                var response = await client.ExecuteAsync(request);

                return response.Content;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        /// <summary>
        /// PostForm
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string PostForm(string url, Dictionary<string, object> reqData = null, Dictionary<string, string> headers = null)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Post,
                    Timeout = 30000,

                };

                if (Convert.ToBoolean(headers?.Any()))
                {
                    foreach (var header in headers)
                        request.AddHeader(header.Key, header.Value);
                }

                if (Convert.ToBoolean(reqData?.Any()))
                {
                    foreach (var item in reqData)
                        request.AddParameter(item.Key, item.Value, ParameterType.GetOrPost);

                }

                var response = client.Execute(request);

                return response.Content;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        /// <summary>
        /// PostFormAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<string> PostFormAsync(string url, Dictionary<string, object> reqData = null, Dictionary<string, string> headers = null)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                {
                    Method = Method.Post,
                    Timeout = 30000,

                };

                if (Convert.ToBoolean(headers?.Any()))
                {
                    foreach (var header in headers)
                        request.AddHeader(header.Key, header.Value);
                }

                if (Convert.ToBoolean(reqData?.Any()))
                {
                    foreach (var item in reqData)
                        request.AddParameter(item.Key, item.Value, ParameterType.GetOrPost);

                }

                var response = await client.ExecuteAsync(request);

                return response.Content;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

    }
}
