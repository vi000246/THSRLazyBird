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
            var isToDateValid = validDate(config.targetDate.TripToDate);
           var isBackDateValid = validDate(config.targetDate.TripBackDate);
           var haveBackDate = string.IsNullOrEmpty(config.targetDate.TripBackDate);
           //回程可以不填寫,如果為空就不驗證回程
           if ((!haveBackDate && !isBackDateValid.isValid) || !isToDateValid.isValid)
           {
                 var msg = $"訂位代號:{config.OrderId} 設定檔Date欄位格式錯誤， 必須為yyyy/mm/dd";
                _logger.LogCritical(msg);
                throw new ArgumentException(msg);

           }
           var isBackDatefuture = validPassDate(isBackDateValid.dDate.Value);
           var isToDatefuture = validPassDate(isToDateValid.dDate.Value);
           if ((!haveBackDate && !isBackDatefuture) || !isToDatefuture)
           {
               var msg = $"訂位代號:{config.OrderId} 日期為過去時間 ，請重新設置";
               _logger.LogCritical(msg);
               throw new ArgumentException(msg);
           }


           /**logic:去程到達時間不可晚於回程出發時間或相距過近
              去程日期不可晚於回程日期
              直接判斷日期就好，不想判斷到時間了**/

           var toTripInfo = orderInfo.trips.FirstOrDefault(x => x.tripType == "去程");
           var orderToDate = Convert.ToDateTime(DateTime.Now.Year + $"/{toTripInfo.date} {toTripInfo.arrivalTime}");


           if (haveBackDate)
           {
               var backTripInfo = orderInfo.trips.FirstOrDefault(x => x.tripType == "回程");
               var orderBackDate =
                   Convert.ToDateTime(DateTime.Now.Year + $"/{backTripInfo.date} {backTripInfo.arrivalTime}");
           }



        }

        private (bool isValid,DateTime? dDate) validDate(string tripDate)
        {
            if (!DateTime.TryParseExact(tripDate, "yyyy/mm/dd", null, System.Globalization.DateTimeStyles.None, out DateTime _date) ||
                !DateTime.TryParse(tripDate, out DateTime dDate))
            {
                return (false,null);

            }

            return (true,dDate);
        }

        private bool validPassDate(DateTime dDate)
        {
            if (DateTime.Now.Date > dDate.Date)
            {
                return false;
            }
            return true;
        }
    }
}
