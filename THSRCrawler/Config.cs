using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace THSRCrawler
{
    public class Config
    {
        public List<TicketOrders> ticketOrders { get; set; }
        public Notify notify { get; set; }
        public bool IsEnableSchedule { get; set; }
        public int MaxETA { get; set; }

        private readonly ILogger<ModifyTripJob> _logger;

        public Config()
        {
        }

        public Config(ILogger<ModifyTripJob> logger)
        {
            _logger = logger;
        }

        //將設定檔的訂位時間轉成高鐵網站可用的格式
        public (string tripDate,string tripTime) GetValidOrderDate(TicketOrders config, CrawlerModels.ModifyTripType tripType, CrawlerModels.orderPageInfo orderInfo)
        {
            string tripDate = "";
            string tripTime = "";
            var targetDate = config.targetDate;
            switch (tripType)
            {
                case CrawlerModels.ModifyTripType.To:
                    tripDate = targetDate.TripToDate;
                    tripTime =targetDate.TripToTime;
                    break;
                case CrawlerModels.ModifyTripType.Back:
                    tripDate = targetDate.TripBackDate;
                    tripTime = targetDate.TripBackTime;
                    break;
            }

            var formatTime = getTimeDropDownValue(tripTime);
            if (string.IsNullOrEmpty(formatTime))
            {
                var msg = $"訂位代號:{config.OrderId} 設定檔Time欄位格式錯誤，time:{tripTime} 必須為mm:dd 以30分鐘為間隔";
                _logger.LogCritical(msg);
                throw new InvalidConfigException(msg);
            }

            return(tripDate, formatTime);
        }

        private string getTimeDropDownValue(string time)
        {
            CrawlerModels.timeDropdown.TryGetValue(time, out string validtime);
            return validtime;
        }

    }

    public class TicketOrders
    {
        public string IdCard { get; set; }
        public string OrderId { get; set; }
        public TargetDate targetDate { get; set; } = new TargetDate();
    }

    public class TargetDate
    {
        public string TripToDate { get; set; }
        public string TripBackDate { get; set; }
        public string TripToTime { get; set; }
        public string TripBackTime { get; set; }
    }

    public class Notify
    {
        public LineBot linebot { get; set; }
        public SMTP smtp { get; set; }
    }

    public class LineBot
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string[] AccessToken { get; set; }
    }

    public class SMTP
    {
        public string Account { get; set; }
        public string Password { get; set; }

    }

}
