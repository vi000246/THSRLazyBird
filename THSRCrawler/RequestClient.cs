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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
            request.AddCookie("JSESSIONID", "");
            request.AddCookie("IRS-SESSION", "");
            request.AddCookie("THSRC-IRS", "");
            request.AddCookie("name", "value");
            request.AddCookie("ak_bmsc", "");
            request.AddCookie("bm_sv", "");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,und;q=0.6,ko;q=0.5");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");
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
