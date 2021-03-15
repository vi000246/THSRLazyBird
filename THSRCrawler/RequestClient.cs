using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Newtonsoft.Json;
using RestSharp;

namespace THSRCrawler
{
    public class RequestClient
    {
        const string BaseUrl = "https://irs.thsrc.com.tw";

        private readonly HttpClient _client;
        private readonly IHttpClientFactory _clientFactory;
        private readonly Config _config;

        public RequestClient(IHttpClientFactory clientFactory,Config config)
        {
            _clientFactory = clientFactory;

            _client = _clientFactory.CreateClient("HttpClientWithSSLUntrusted");
            _client.BaseAddress = new Uri(BaseUrl);
            _config = config;
        }

        public async void LoginPage()
        {
            var LoginPage = await GetHTML("https://irs.thsrc.com.tw/IMINT/?wicket:bookmarkablePage=:tw.com.mitac.webapp.thsr.viewer.History");
        }

        public async void LoginTicketHistoryPage()
        {
            var orders = _config.GetOrders();
            foreach (var order in orders)
            {
                // var requestContent = new FormUrlEncodedContent(new[]
                // {
                // new KeyValuePair<string, string>("idInputRadio:rocId", order.IdCard),
                // new KeyValuePair<string, string>("orderId", order.OrderId),
                // new KeyValuePair<string, string>("SelectPNRView:idPnrInputRadio", "radio18"),
                // new KeyValuePair<string, string>("idInputRadio", "radio10")
                // });
                var content = new List<KeyValuePair<string, string>>();
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("idInputRadio:rocId"), order.IdCard));
                content.Add(new KeyValuePair<string, string>("orderId", order.OrderId));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:idPnrInputRadio"), "radio18"));
                content.Add(new KeyValuePair<string, string>("idInputRadio", "radio10"));
                var url = "https://irs.thsrc.com.tw/IMINT/?wicket:interface=:6:HistoryForm::IFormSubmitListener";
                var html =await PostForm(url, content);

            }

        }

        private async Task<string> PostForm(string url,dynamic content)
        {
            var httpRequestMessage = requestBuilder(url, HttpMethod.Post, content);
            // httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = _client.Send(httpRequestMessage);
            return responseParse(httpRequestMessage, response);
        }


        private async Task<string> GetHTML(string url)
        {
            var httpRequestMessage = requestBuilder(url,HttpMethod.Get);
            var response = _client.Send(httpRequestMessage);
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
                RequestUri = new Uri(Uri.EscapeUriString(url)),
                Headers = {
                    { "Accept-Encoding", "gzip, deflate, br" },
                    { HttpRequestHeader.ContentType.ToString(), contentType },
                    { HttpRequestHeader.Accept.ToString(), "*/*" },
                    { "Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,und;q=0.6,ko;q=0.5" },
                },
                Content = HttpContent
            };

            
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
                response = _client.Send(request);
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
