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
            _logger.LogDebug("開始執行訂票流程...");
            var configs = _config.ticketOrders;
            foreach (var config in configs)
            {
                _validation.validOrderIdAndIdCard(config);
 
                var orderInfo = post_search_order_form(config.IdCard, config.OrderId);
                
                if (!orderInfo.isTripEditable)
                {
                    _logger.LogDebug($"訂位代號:{config.OrderId} 無法變更行程");
                    return;
                }
                _validation.validConfigDateTime(config,orderInfo);

                if (_validation.haveToDateFunc(config))
                {
                    var result = ModifyTrip(CrawlerModels.ModifyTripType.To, config, orderInfo);
                    _logger.LogDebug($"訂位代號:{config.OrderId} 去程執行結果:{result}");
                }
                if (orderInfo.isRoundTrip && _validation.haveBackDateFunc(config))
                {
                    var result = ModifyTrip(CrawlerModels.ModifyTripType.Back, config, orderInfo);
                    _logger.LogDebug($"訂位代號:{config.OrderId} 回程執行結果:{result}");
                }

            }
            _logger.LogDebug("訂票流程執行成功!");
        }

        public CrawlerModels.orderPageInfo post_search_order_form(string IdCard,string OrderId)
        {
            CrawlerModels.orderPageInfo orderInfo = null;
            try
            {
                var orderPageHtml = _requestClient.GoTo_search_order_page();
                var searchOrderUrl = _htmlParser.GetNextStepUrl(orderPageHtml);
                var html = _requestClient.post_search_order_form(IdCard, OrderId, searchOrderUrl);
                orderInfo = _htmlParser.GetOrderInformation(html);
            }
            catch (CritialPageErrorException ex)
            {
                throw new CritialPageErrorException($"身份證字號:{IdCard}\r\n訂位代號:{OrderId}\r\n {ex.Message}");
            }

            return orderInfo;
        }

        public CrawlerModels.modifyResult ModifyTrip(CrawlerModels.ModifyTripType tripType,TicketOrders config,CrawlerModels.orderPageInfo orderInfo)
        {
            CrawlerModels.modifyResult result = CrawlerModels.modifyResult.fail;

            try
            {
                var modifyTripPageHtml = _requestClient.GoTo_modifyTrip_form(orderInfo.nextStepUrl);
                //判斷去/回程是否可更改
                var OrderStatus = _htmlParser.CheckModifyFormIsAvailable(modifyTripPageHtml);
                _validation.validOrderStatus(tripType,OrderStatus);
                var searchTripUrl = _htmlParser.GetNextStepUrl(modifyTripPageHtml);

                var formatDate = _config.GetValidOrderDate(config, tripType,orderInfo);
                var html = _requestClient.post_search_trip_form(tripType,searchTripUrl, formatDate);
                var modifyOrderUrl = _htmlParser.GetNextStepUrl(html);
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
                        string modifyResultHtml = _requestClient.post_modifyTrip_form(matchTrip, modifyOrderUrl, tripType);
                        result = _htmlParser.ModifyOrderResult(modifyResultHtml);
                        if (result == CrawlerModels.modifyResult.needConfirm)
                        {
                            var confirmOrderUrl = _htmlParser.GetNextStepUrl(modifyResultHtml);
                            string confirmResult = _requestClient.post_confirm_form(confirmOrderUrl);
                            result = _htmlParser.ModifyOrderResult(confirmResult);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
            }
            return result;

        }



    }
}
