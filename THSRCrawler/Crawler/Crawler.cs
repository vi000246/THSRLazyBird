using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using RestSharp;
using ILogger = NLog.ILogger;

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
        private readonly ILogger<Crawler> _logger;
        public Crawler(RequestClient requestClient, IOptions<Config> config, HTMLParser htmlParser, ILogger<Crawler> logger)
        {
            _requestClient = requestClient;
            _config = config.Value;
            _htmlParser = htmlParser;
            _logger = logger;
        }
        public void init()
        {
            var orders = _config.ticketOrders;
            foreach (var order in orders)
            {
                var orderInfo = post_search_order_form(order.IdCard,order.OrderId);
                
                if (!orderInfo.isTripEditable)
                {
                    _logger.LogInformation($"此訂位代號:{order.OrderId} 無法變更行程");
                    return;
                }

                ModifyTrip(Models.ModifyTripType.To);
                if (orderInfo.isRoundTrip)
                {
                    ModifyTrip(Models.ModifyTripType.Back);
                }

            }
        }

        public void GoTo_search_order_page()
        {
            _requestClient.GoTo_search_order_page();
        }

        public Models.orderPageInfo post_search_order_form(string IdCard,string OrderId)
        {
            GoTo_search_order_page();
            var html =  _requestClient.post_search_order_form(IdCard,OrderId);
            var orderInfo = _htmlParser.GetOrderInformation(html);
            return orderInfo;
        }

        public string ModifyTrip(Models.ModifyTripType tripType)
        {
            var modifyTripPageHtml = _requestClient.GoTo_modifyTrip_form();
            //判斷去/回程是否可更改


            var html = _requestClient.post_search_trip_form(tripType);
            var trips = _htmlParser.GetTripsPerPage(html, _requestClient.GetTripType());
            //找出符合的行程
            //變更行程
            return "";
        }

        public string ModifyTrip_NextPage()
        {
            return _requestClient.GoTo_ModifyTrip_NextPage();
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
