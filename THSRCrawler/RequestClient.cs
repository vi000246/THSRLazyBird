using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        }

        public void Login()
        {
            //這是登入頁面，還沒登入
            var loginRequest = new RestRequest("IMINT",Method.GET);
            loginRequest.AddParameter("wicket:bookmarkablePage", ":tw.com.mitac.webapp.thsr.viewer.History");
            var response = _client.Get<string>(loginRequest);
            var LoginPage = Execute<string>(loginRequest);
            var abc = 123;

        }

        private T Execute<T>(RestRequest request) 
        {
            var response = _client.Execute<dynamic>(request);
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
