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
        const string BaseUrl = "https://irs.thsrc.com.tw/IMINT/";
        readonly IRestClient _client;

        public RequestClient()
        {
            _client = new RestClient(BaseUrl);
            _client.CookieContainer = new System.Net.CookieContainer();
            _client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:77.0) Gecko/20100101 Firefox/77.0";
            _client.FollowRedirects = false;
        }

        public void Login()
        {
            //這是登入頁面，還沒登入
            var loginRequest = new RestRequest(Method.GET);
            loginRequest.AddParameter("wicket:bookmarkablePage",":tw.com.mitac.webapp.thsr.viewer.History", ParameterType.QueryStringWithoutEncode);
            var LoginPage = Execute<string>(loginRequest);
            var abc = 123;

        }

        private T Execute<T>(RestRequest request)
        {
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "zh-TW,zh;q=0.8,en-US;q=0.5,en;q=0.3");
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
