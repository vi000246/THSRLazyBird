using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using RestSharp;

namespace THSRCrawler
{
    public class RequestClient
    {
        const string BaseUrl = "https://irs.thsrc.com.tw";
        readonly IRestClient _client;

        public RequestClient()
        {
            _client = new RestClient(BaseUrl);
            _client.CookieContainer = new System.Net.CookieContainer();
            _client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:77.0) Gecko/20100101 Firefox/77.0";
            _client.FollowRedirects = false;
            _client.MaxRedirects = 1;
            //ignore ssl error , for debug tool
            _client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public void Login()
        {
            //這是登入頁面，還沒登入
            var loginRequest = new RestRequest("/IMINT/",Method.GET);
            loginRequest.AddParameter("wicket:bookmarkablePage",":tw.com.mitac.webapp.thsr.viewer.History", ParameterType.QueryStringWithoutEncode);
            var LoginPage = Execute<string>(loginRequest);
            var abc = 123;

        }

        private T Execute<T>(RestRequest request)
        {
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            //request.AddHeader("Sec-Fetch-Site:", "none");
            //request.AddHeader("Sec-Fetch-Mode:", "cors");
            //request.AddHeader("Sec-Fetch-Dest:", "empty");
            request.AddHeader("Accept-Language", "zh-TW,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            //IRestResponse response3 = _client.Get(request);
            IRestResponse response2 = _client.Execute(request);
            int numericStatusCode = (int)response2.StatusCode;
            var response = _client.Execute<T>(request);
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var Exception = new Exception(message, response.ErrorException);
                throw Exception;
            }
            return response.Data;
        }

    }
}
