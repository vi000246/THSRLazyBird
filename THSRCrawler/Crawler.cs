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
        public Crawler(RequestClient requestClient, IOptions<Config> config)
        {
            _requestClient = requestClient;
            _config = config.Value;

        }
        public void init()
        {
            var orders = _config.ticketOrders;
            foreach (var order in orders)
            {
                Login();
                LoginTicketHistoryPage(order.IdCard,order.OrderId);
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
