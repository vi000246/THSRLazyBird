using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
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
        private readonly TripCompare _tripCompare;
        public Crawler(RequestClient requestClient, IOptions<Config> config, HTMLParser htmlParser, ILogger<Crawler> logger,Validation validation,TripCompare tripCompare)
        {
            _requestClient = requestClient;
            _config = config.Value;
            _htmlParser = htmlParser;
            _logger = logger;
            _validation = validation;
            _tripCompare = tripCompare;
        }
        public void init()
        {
            var configs = _config.ticketOrders;
            foreach (var config in configs)
            {
                _validation.validOrderIdAndIdCard(config);

                var orderInfo = post_search_order_form(config.IdCard, config.OrderId);
                
                if (!orderInfo.isTripEditable)
                {
                    _logger.LogInformation($"此訂位代號:{config.OrderId} 無法變更行程");
                    return;
                }
                _validation.validConfigDateTime(config,orderInfo);

                if (_validation.haveToDateFunc(config))
                {
                    ModifyTrip(CrawlerModels.ModifyTripType.To, config, orderInfo);
                }
                if (orderInfo.isRoundTrip && _validation.haveBackDateFunc(config))
                {
                    ModifyTrip(CrawlerModels.ModifyTripType.Back, config, orderInfo);
                }

            }
        }

        public CrawlerModels.orderPageInfo post_search_order_form(string IdCard,string OrderId)
        {
            _requestClient.GoTo_search_order_page();
            var html = _requestClient.post_search_order_form(IdCard, OrderId);
            var orderInfo = _htmlParser.GetOrderInformation(html);

            return orderInfo;
        }

        public void ModifyTrip(CrawlerModels.ModifyTripType tripType,TicketOrders config,CrawlerModels.orderPageInfo orderInfo)
        {
            try
            {
                var modifyTripPageHtml = _requestClient.GoTo_modifyTrip_form();
                //判斷去/回程是否可更改
                var OrderStatus = _htmlParser.CheckModifyFormIsAvailable(modifyTripPageHtml);
                if (OrderStatus == CrawlerModels.orderStatus.BothCannot)
                {
                    throw new ArgumentException("無法修改行程");
                }

                if (tripType == CrawlerModels.ModifyTripType.To)
                {
                    if(OrderStatus == CrawlerModels.orderStatus.onlyBack)
                        throw new ArgumentException("無法修改去程");
                }
                else if(tripType == CrawlerModels.ModifyTripType.Back)
                {
                    if(OrderStatus == CrawlerModels.orderStatus.onlyTo)
                        throw new ArgumentException("無法修改回程");

                }


                var formatDate = _config.GetValidOrderDate(config, tripType,orderInfo);
                var html = _requestClient.post_search_trip_form(tripType, formatDate);
                var trips = _htmlParser.GetTripsPerPageAndHandleError(html, tripType);
                if (trips != null && trips.Count() >= 0)
                {
                    var tripInfo = new CrawlerModels.tripInfo();
                    if (tripType == CrawlerModels.ModifyTripType.To)
                        tripInfo = orderInfo.trips.FirstOrDefault(x => x.tripType == "去程");
                    else
                    {
                        tripInfo = orderInfo.trips.FirstOrDefault(x => x.tripType == "回程");
                    }

                    var matchTrip = _tripCompare.FindMatchTrip(trips.ToList(), tripInfo,formatDate.tripDateTime);
                    if (!string.IsNullOrEmpty(matchTrip))
                    {
                        string result = _requestClient.post_modifyTrip_form(matchTrip);
                        if (_htmlParser.ModifyOrderResult(result) == CrawlerModels.modifyResult.needConfirm)
                        {
                            _requestClient.post_confirm_form();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //這裡不需要處理例外，就讓程式繼續跑
            }

        }



    }
}
