using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace THSRCrawler
{
    public class Crawler
    {


        private readonly RequestClient _requestClient;
        private readonly Config _config;
        public Crawler(RequestClient requestClient, Config config)
        {
            _requestClient = requestClient;
            _config = config;

        }
        public void init()
        {
            var orders = _config.GetOrders();
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

        //變更行程
        //取得可用車次列表
        //重新訂票
    }
}
