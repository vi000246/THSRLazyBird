using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RestSharp;

namespace THSRCrawler
{
    public class RequestClient
    {
        const string BaseUrl = "https://irs.thsrc.com.tw";

        private readonly HttpClient _client;
        private readonly IHttpClientFactory _clientFactory;

        public RequestClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;

            _client = _clientFactory.CreateClient("HttpClientWithSSLUntrusted");
            _client.BaseAddress = new Uri(BaseUrl);

            //_client.CookieContainer = new System.Net.CookieContainer();
            //_client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:77.0) Gecko/20100101 Firefox/77.0";
            //_client.FollowRedirects = false;
            //_client.MaxRedirects = 1;
            //ignore ssl error , for debug tool
            //_client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public async void Login()
        {
            //這是登入頁面，還沒登入
            //loginRequest.AddParameter("wicket:bookmarkablePage",":tw.com.mitac.webapp.thsr.viewer.History", ParameterType.QueryStringWithoutEncode);
            var LoginPage = await GetHTML("https://irs.thsrc.com.tw/IMINT/?wicket:bookmarkablePage=:tw.com.mitac.webapp.thsr.viewer.History");
        }



        private async Task<string> GetHTML(string url)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Uri.EscapeUriString(url)),
                Headers = {
                    { "Accept-Encoding", "gzip, deflate, br" },
                    { HttpRequestHeader.ContentType.ToString(), "text/html;charset=UTF-8" },
                    { HttpRequestHeader.Accept.ToString(), "*/*" },
                    { "Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,und;q=0.6,ko;q=0.5" },

                }
            };
            var response = _client.Send(httpRequestMessage);
            int numericStatusCode = (int)response.StatusCode;
            //重定向302回應要再送一次request
            if (numericStatusCode == 302)
            {
                response = _client.Send(Clone(httpRequestMessage));
            }

            var contentType = response.Content.Headers.ContentType;
            if (string.IsNullOrEmpty(contentType.CharSet))
            {
                contentType.CharSet = "utf-8";
            }
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            
            return content;

        }

        public static HttpRequestMessage Clone(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version
            };
            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

    }
}
