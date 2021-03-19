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
        public (string tripDate,string tripTime) GetValidOrderDate(TicketOrders order, Models.ModifyTripType tripType)
        {
            string tripDate = "";
            string tripTime = "";
            var targetDate = order.targetDate;
            switch (tripType)
            {
                case Models.ModifyTripType.To:
                    tripDate = targetDate.TripToDate;
                    tripTime = getTimeDropDownValue(targetDate.TripToTime);
                    break;
                case Models.ModifyTripType.Back:
                    tripDate = targetDate.TripBackDate;
                    tripTime = getTimeDropDownValue(targetDate.TripBackTime);
                    break;
            }

            string[] formats = { "yyyy/mm/dd", "yyyy/m/d", "yyyy/m/dd", "yyyy/mm/d" };
            if (!DateTime.TryParseExact(tripDate, formats, null, System.Globalization.DateTimeStyles.None, out DateTime _date) ||
                !DateTime.TryParse(tripDate, out DateTime dDate))
            {
                var msg = $"訂位代號:{order.OrderId} 設定檔Date欄位格式錯誤，Date:{tripDate} 必須為yyyy/mm/dd";
                _logger.LogCritical(msg);
                throw new ArgumentException(msg);
            }

            if (DateTime.Now.Date > dDate.Date)
            {
                var msg = $"訂位代號:{order.OrderId} 日期已過期 {(DateTime.Now - dDate).Days}天，請重新設置，Date:{tripDate}";
                _logger.LogCritical(msg);
                throw new ArgumentException(msg);
            }

            if (string.IsNullOrEmpty(tripTime))
            {
                var msg = $"訂位代號:{order.OrderId} 設定檔Time欄位格式錯誤，time:{tripTime} 必須為mm:dd 以30分鐘為間隔";
                _logger.LogCritical(msg);
                throw new ArgumentException(msg);
            }

            return(tripDate,tripTime);
        }

        private string getTimeDropDownValue(string time)
        {
            Models.timeDropdown.TryGetValue(time, out string validtime);
            return validtime;
        }

    }

    public class TicketOrders
    {
        public string IdCard { get; set; }
        public string OrderId { get; set; }
        public TargetDate targetDate { get; set; }
    }

    public class TargetDate
    {
        public string TripToDate { get; set; }
        public string TripBackDate { get; set; }
        public string TripToTime { get; set; }
        public string TripBackTime { get; set; }
    }
}
