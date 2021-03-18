using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RestSharp;

namespace THSRCrawler
{
    /// <summary>
    /// 用來儲放crawler的整個流程
    /// </summary>
    public class Crawler
    {


        private readonly RequestClient _requestClient;
        private readonly Config _config;
        private readonly HTMLParser _htmlParser;
        public Crawler(RequestClient requestClient, IOptions<Config> config, HTMLParser htmlParser)
        {
            _requestClient = requestClient;
            _config = config.Value;
            _htmlParser = htmlParser;

        }
        public void init()
        {
            var orders = _config.ticketOrders;
            foreach (var order in orders)
            {
                Login();
                LoginTicketHistoryPage(order.IdCard,order.OrderId);
                var html = GetModifyTripHTML();
                _htmlParser.GetTripsPerPage(html);
                var nextPageHtml = ModifyTrip_NextPage();
            }
        }

        public void Login()
        {
            _requestClient.LoginPage();
        }

        public void LoginTicketHistoryPage(string IdCard,string OrderId)
        {
            _requestClient.LoginTicketHistoryPage(IdCard,OrderId);
        }

        public string GetModifyTripHTML()
        {
            return _requestClient.GetModifyTripHTML();
        }

        public string ModifyTrip_NextPage()
        {
            return _requestClient.ModifyTrip_NextPage();
        }

        public void GetAllAvailableTickets()
        {
            //取得所有可用車次
        }

        public void FindMatchTickets()
        {
            //取得符合條件的車票
        }

        public void ChangeOrderTickets()
        {
            //變更行程至指定的時段
        }

    }
}
