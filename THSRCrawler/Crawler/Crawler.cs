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
        private readonly Validation _validation;
        public Crawler(RequestClient requestClient, IOptions<Config> config, HTMLParser htmlParser, ILogger<Crawler> logger,Validation validation)
        {
            _requestClient = requestClient;
            _config = config.Value;
            _htmlParser = htmlParser;
            _logger = logger;
            _validation = validation;
        }
        public void init()
        {
            var configs = _config.ticketOrders;
            foreach (var config in configs)
            {
                var orderInfo = post_search_order_form(config.IdCard, config.OrderId);
                
                if (!orderInfo.isTripEditable)
                {
                    _logger.LogInformation($"此訂位代號:{config.OrderId} 無法變更行程");
                    return;
                }

                ModifyTrip(Models.ModifyTripType.To, config, orderInfo);
                if (orderInfo.isRoundTrip)
                {
                    ModifyTrip(Models.ModifyTripType.Back, config, orderInfo);
                }

            }
        }

        public Models.orderPageInfo post_search_order_form(string IdCard,string OrderId)
        {
            _requestClient.GoTo_search_order_page();
            var html = _requestClient.post_search_order_form(IdCard, OrderId);
            var orderInfo = _htmlParser.GetOrderInformation(html);

            return orderInfo;
        }

        public void ModifyTrip(Models.ModifyTripType tripType,TicketOrders config,Models.orderPageInfo orderInfo)
        {
            try
            {
                var modifyTripPageHtml = _requestClient.GoTo_modifyTrip_form();
                //判斷去/回程是否可更改

                _validation.validConfigDateTime(config,orderInfo);

                var formatDate = _config.GetValidOrderDate(config, tripType,orderInfo);
                var html = _requestClient.post_search_trip_form(tripType, formatDate);
                var trips = _htmlParser.GetTripsPerPageAndHandleError(html, tripType);
                if (trips != null && trips.Count() >= 0)
                {
                    var matchTrip = FindMatchTrip(tripType, trips, orderInfo);
                    if (!string.IsNullOrEmpty(matchTrip))
                    {
                        _requestClient.post_modifyTrip_form(matchTrip);
                    }
                }
            }
            catch (Exception ex)
            {
                //這裡不需要處理例外，就讓程式繼續跑
            }

        }

        public string FindMatchTrip(Models.ModifyTripType tripType ,IEnumerable<Models.Trips> trips,Models.orderPageInfo orderInfo)
        {
            var tripTitle = "去程";
            if (tripType == Models.ModifyTripType.Back)
                tripTitle = "回程";
            //目前訂單的行程資訊
            var tripInfo = orderInfo.trips.First(x => x.tripType == tripTitle);
            //組出行程的抵達時間

            //如果目前訂位日期跟設定檔日期一樣，就判斷抵達時間
            //如果不一樣，就挑一個行車時間最短的

            return trips.FirstOrDefault().buttonName;
        }

    }
}
