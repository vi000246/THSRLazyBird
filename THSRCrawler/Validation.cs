using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace THSRCrawler
{
    public class Validation
    {
        private readonly ILogger<Validation> _logger;

        public Validation()
        {
        }

        public Validation(ILogger<Validation> logger)
        {
            _logger = logger;
        }


        public void validConfigDateTime(TicketOrders config, Models.orderPageInfo orderInfo)
        {
            if (config.targetDate == null || string.IsNullOrEmpty(config.targetDate.TripToDate) || string.IsNullOrEmpty(config.targetDate.TripToTime)) {
                throw new InvalidConfigException($"訂位代號:{config.OrderId} 請填寫去程目標時段");
            }

            var haveBackDate = string.IsNullOrEmpty(config.targetDate.TripBackDate);

            var configToDate = validDateFormat(config.targetDate.TripToDate,config.OrderId);
            validPassDate(configToDate, config.OrderId);
            /**
             * logic:去程到達時間不可晚於回程出發時間或相距過近
             * 去程日期不可晚於回程日期
            **/
            var backTripInfo = orderInfo.trips.FirstOrDefault(x => x.tripType == "回程");
            compareTripDateAndConfigDate(backTripInfo, configToDate);


            if (haveBackDate)
            {
                var configBackDate = validDateFormat(config.targetDate.TripBackDate, config.OrderId);
                validPassDate(configBackDate, config.OrderId);
                var toTripInfo = orderInfo.trips.FirstOrDefault(x => x.tripType == "去程");
                compareTripDateAndConfigDate(toTripInfo, configBackDate);
            }

        }

        public DateTime validDateFormat(string tripDate,string OrderId)
        {
            if (!DateTime.TryParseExact(tripDate, "yyyy/mm/dd", null, System.Globalization.DateTimeStyles.None, out DateTime _date) ||
                !DateTime.TryParse(tripDate, out DateTime dDate))
            {
                var msg = $"訂位代號:{OrderId} 設定檔Date欄位格式錯誤， 必須為yyyy/mm/dd";
                _logger.LogCritical(msg);
                throw new InvalidConfigException(msg);

            }

            return dDate;
        }

        public void validPassDate(DateTime dDate,string orderId)
        {
            if (DateTime.Now.Date > dDate.Date)
            {
                var msg = $"訂位代號:{orderId} 日期為過去時間 ，請重新設置";
                _logger.LogCritical(msg);
                throw new InvalidConfigException(msg);
            }
        }
        public void compareTripDateAndConfigDate(Models.tripInfo tripInfo,DateTime configDate) {
            DateTime orderDate;
            try
            {
                if (tripInfo == null) {
                    throw new ArgumentException("無法取得訂位紀錄的車次");
                }
                orderDate = Convert.ToDateTime(DateTime.Now.Year + $"/{tripInfo.date} {tripInfo.arrivalTime}");
            }
            catch (Exception ex) {
                throw new ArgumentException($"無法取得訂位紀錄的車次日期 {tripInfo.date} {tripInfo.arrivalTime}");
            }

            if (tripInfo.tripType == "去程")
            {
                //加30分鐘的buffer
                if (configDate <= orderDate.AddMinutes(30))
                    throw new InvalidConfigException("設定檔的回程不得小於訂位紀錄的去程");
            }
            else if (tripInfo.tripType == "回程")
            {
                if(configDate >= orderDate.AddMinutes(-30))
                    throw new InvalidConfigException("設定檔的去程不得大於訂位紀錄的回程");
            }
            else {
               
            }
        }
    }
}
