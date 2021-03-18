using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RestSharp;

namespace THSRCrawler
{
    /// <summary>
    /// 用來儲放跟http request有關的邏輯
    /// </summary>
    public class RequestClient
    {
        const string BaseUrl = "https://irs.thsrc.com.tw/";

        private readonly HttpClient _client;
        private readonly IHttpClientFactory _clientFactory;
        private readonly Config _config;
        private readonly CookieContainer _cookieContainer;


        public RequestClient(IHttpClientFactory clientFactory,Config config,CookieContainer cookieContainer)
        {
            _clientFactory = clientFactory;

            _client = _clientFactory.CreateClient("HttpClientWithSSLUntrusted");
            _client.BaseAddress = new Uri(BaseUrl);
            _config = config;
            _client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            _cookieContainer = cookieContainer;
        }

        public async void LoginPage()
        {
            var LoginPage = await GetHTML("/IMINT/?wicket:bookmarkablePage=:tw.com.mitac.webapp.thsr.viewer.History");
        }

        //輸入訂位代號跟身份證字號，取得訂位紀錄
        public async void LoginTicketHistoryPage(string IdCard,string OrderId)
        {
                var content = new List<KeyValuePair<string, string>>();
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("idInputRadio:rocId"), IdCard));
                content.Add(new KeyValuePair<string, string>("orderId", OrderId));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:idPnrInputRadio"), "radio18"));
                content.Add(new KeyValuePair<string, string>("idInputRadio", "radio10"));
            //看起來url只要有jssessionid就給過了，不需要真的加上jssessionid的cookie
                var url = $"/IMINT/;jsessionid={_cookieContainer.GetCookies(new Uri(BaseUrl)).FirstOrDefault(x=>x.Name == "JSSESSIONID")}?wicket:interface=:0:HistoryForm::IFormSubmitListener";
                var html =await PostForm(url, content);
        }

        private async Task<string> PostForm(string url,dynamic content)
        {
            var httpRequestMessage = requestBuilder(url, HttpMethod.Post, content);
            var response = GetResponseAndSetCookie(httpRequestMessage);
            return responseParse(httpRequestMessage, response);
        }


        private async Task<string> GetHTML(string url)
        {
            var httpRequestMessage = requestBuilder(url,HttpMethod.Get);
            var response = GetResponseAndSetCookie(httpRequestMessage);
            return responseParse(httpRequestMessage, response);
        }

        private HttpRequestMessage requestBuilder(string url, HttpMethod method,dynamic content = null)
        {
            var contentType = "text/html;charset=UTF-8";
            FormUrlEncodedContent HttpContent = null;
            if (content != null)
            {
                contentType = "application/x-www-form-urlencoded";
                HttpContent = new FormUrlEncodedContent(content);
            }
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(Uri.EscapeUriString(url), UriKind.RelativeOrAbsolute),
                Content = HttpContent,
            };

            httpRequestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpRequestMessage.Headers.Add(HttpRequestHeader.ContentType.ToString(), contentType);
            httpRequestMessage.Headers.Add(HttpRequestHeader.Accept.ToString(), "*/*");
            httpRequestMessage.Headers.Add("Origin", "https://irs.thsrc.com.tw");
            httpRequestMessage.Headers.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,und;q=0.6,ko;q=0.5");
            
            return httpRequestMessage;
        }

        private string responseParse(HttpRequestMessage httpRequestMessage, HttpResponseMessage response)
        {
            int numericStatusCode = (int)response.StatusCode;
            //重定向302，需要再發送一次request
            if (numericStatusCode == 302)
            {
                var request = Clone(httpRequestMessage);
                //302的表頭會有要redirect的url,要再重送一次request到這個url
                request.RequestUri = response.Headers.Location;
                response = GetResponseAndSetCookie(request);
            }

            var contentType = response.Content.Headers.ContentType;
            if (string.IsNullOrEmpty(contentType.CharSet))
            {
                contentType.CharSet = "utf-8";
            }
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return content;

        }

        //debug用，看有沒有回傳需要的cookie
        private HttpResponseMessage GetResponseAndSetCookie(HttpRequestMessage request)
        {
            var response = _client.Send(request);
            // var cookieHeaders = response.Headers.Where(pair => pair.Key == "Set-Cookie");
            // foreach (var value in cookieHeaders.SelectMany(header => header.Value))
            // {
            //     _cookieContainer.SetCookies(request.RequestUri, value);
            // }

            return response;
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
