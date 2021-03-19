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
    /// note:在用抓包工具時，記得先把brwoser cookie清掉，抓到的封包才是最準的，不然會有cache,會debug老半天
    /// note2:強烈懷疑網址中的interface=:1:  的數字是有意義的，如果取不到正確response，可以查查這個參數
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
        
        //第一次進到頁面,取得相關cookie
        public void GoTo_search_order_page()
        {
            var LoginPage = GetHTML("/IMINT/?wicket:bookmarkablePage=:tw.com.mitac.webapp.thsr.viewer.History");
        }

        //輸入訂位代號跟身份證字號，取得訂位紀錄的付款頁面
        public string post_search_order_form(string IdCard,string OrderId)
        {
                var content = new List<KeyValuePair<string, string>>();
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("HistoryForm:hf:0"), ""));
                content.Add(new KeyValuePair<string, string>("idInputRadio", "radio10"));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("idInputRadio:rocId"), IdCard));
                content.Add(new KeyValuePair<string, string>("orderId", OrderId));
                content.Add(new KeyValuePair<string, string>("SubmitButton", "登入查詢"));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:idPnrInputRadio"), "radio18"));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:idPnrInputRadio:rocPnrId"), ""));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:selectStartStation"), ""));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:selectDestinationStation"), ""));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:toTimeInputField"), DateTime.Now.ToString("yyyy/MM/dd")));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:toTrainIDInputField"), ""));
                content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("SelectPNRView:divCaptcha:securityCode"), ""));
                var cookies = _cookieContainer.GetCookies(new Uri(BaseUrl+"IMINT"));
                var url = $"/IMINT/;{cookies.FirstOrDefault(x=>x.Name == "JSESSIONID")}?wicket:interface=:0:HistoryForm::IFormSubmitListener";
                var html = PostForm(url, content);
                return html;
        }

        //輸入要更改的日期跟時間,取得該時段的車票

        public string post_search_trip_form(Models.ModifyTripType tripType,TicketOrders order)
        {
            var formatDate = _config.GetValidOrderDate(order,tripType);

            var content = new List<KeyValuePair<string, string>>();
            content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("HistoryDetailsModifyTripS1Form:hf:0"), ""));
            content.Add(new KeyValuePair<string, string>("bookingMethod", "radio10"));
            switch (tripType)
            {
                case Models.ModifyTripType.To:
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("toContainer:toCheck"), "on")); //勾選變更去程
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("toContainer:toTimeInputField"), formatDate.tripDate));
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("toContainer:toTimeTable"), formatDate.tripTime));
                    content.Add( new KeyValuePair<string, string>(Uri.EscapeUriString("toContainer:toTrainIDInputField"), ""));
                    break;
                case Models.ModifyTripType.Back:
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("backContainer:backCheck"), "on"));//勾選變更回程
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("backContainer:backTimeInputField"), formatDate.tripDate));
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("backContainer:backTimeTable"), formatDate.tripTime));
                    content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("backContainer:backTrainIDInputField"), ""));
                    break;
            }

            content.Add(new KeyValuePair<string, string>("SubmitButton", "開始查詢"));
            var url = $"/IMINT/?wicket:interface=:2:HistoryDetailsModifyTripS1Form::IFormSubmitListener";
            var html = PostForm(url, content);
            return html;
        }

        //取得變更行程頁面，更晚的車票
        public string GoTo_ModifyTrip_NextPage()
        {
            var html = GetHTML("/IMINT/?wicket:interface=:7:HistoryDetailsModifyTripS2Form:TrainQueryDataViewPanel:PreAndLaterTrainContainer:laterTrainLink::IBehaviorListener&wicket:behaviorId=0&random=0.6036634629572775");
            return html;
        }

        //取得變更行程頁面，更早的車票
        public string GoTo_ModifyTrip_PrevPage()
        {
            var html = GetHTML("/IMINT/?wicket:interface=:7:HistoryDetailsModifyTripS2Form:TrainQueryDataViewPanel:PreAndLaterTrainContainer:laterTrainLink::IBehaviorListener&wicket:behaviorId=0&random=0.6036634629572775");
            return html;
        }

        //送出變更行程表單
        public string post_modifyTrip_form()
        {
            var content = new List<KeyValuePair<string, string>>();
            content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("HistoryDetailsModifyTripS2Form:hf:0"), ""));
            //這兩欄是給js判斷用的，沒影響
            content.Add(new KeyValuePair<string, string>("paymentStatus", "2"));
            content.Add(new KeyValuePair<string, string>("ticketTakeStatus", "5"));

            //輸入該車次的radio button name
            content.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("TrainQueryDataViewPanel:TrainGroup"), "radio17"));
            content.Add(new KeyValuePair<string, string>("SubmitButton", "確認車次"));
            var url = $"/IMINT/?wicket:interface=:7:HistoryDetailsModifyTripS2Form::IFormSubmitListener";
            var html = PostForm(url, content);
            return html;
        }

        //進到變更行程頁面
        public string GoTo_modifyTrip_form()
        {
            var clickChangtTripButton = new List<KeyValuePair<string, string>>();
            clickChangtTripButton.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("HistoryDetailsForm:hf:0"), ""));
            clickChangtTripButton.Add(new KeyValuePair<string, string>("ShowCarDiff", "1"));
            clickChangtTripButton.Add(new KeyValuePair<string, string>("isStudentInfo", "0"));
            clickChangtTripButton.Add(new KeyValuePair<string, string>(Uri.EscapeUriString("TicketProcessButtonPanel:ModifyTripButton"), "變更行程"));
            var html = PostForm("/IMINT/?wicket:interface=:1:HistoryDetailsForm::IFormSubmitListener", clickChangtTripButton);

            return html;
        }

        private string PostForm(string url,dynamic content)
        {
            var httpRequestMessage = requestBuilder(url, HttpMethod.Post, content);
            var response = GetResponseAndSetCookie(httpRequestMessage);
            return responseParse(httpRequestMessage, response);
        }


        private string GetHTML(string url)
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
            var cookieHeaders = response.Headers.Where(pair => pair.Key == "Set-Cookie");
            // foreach (var value in cookieHeaders.SelectMany(header => header.Value))
            // {
            // }

            return response;
        }


    }
}
