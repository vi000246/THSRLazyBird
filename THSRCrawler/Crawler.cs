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
        public Crawler(RequestClient requestClient)
        {
            _requestClient = requestClient;
            
        }
        public void init()
        {
            Login();
            LoginTicketHistoryPage();
        }

        public void Login()
        {
            _requestClient.LoginPage();
        }

        public void LoginTicketHistoryPage()
        {
            _requestClient.LoginTicketHistoryPage();
        }

        //變更行程
        //取得可用車次列表
        //重新訂票
    }
}
