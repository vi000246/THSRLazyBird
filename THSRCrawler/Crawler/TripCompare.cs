using System;
using System.Collections.Generic;
using System.Linq;

namespace THSRCrawler
{
    public class TripCompare
    {
        public TripCompare()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trips">搜尋車次頁面取得的車次列表</param>
        /// <param name="orderTripInfo">目前訂單的行程資訊</param>
        /// <param name="ConfigTargetDate">設定檔設定的目標出發時間</param>
        /// <returns></returns>
        public string FindMatchTrip(List<CrawlerModels.Trips> trips, CrawlerModels.tripInfo orderTripInfo,DateTime ConfigTargetDate)
        {
            //組出行程的抵達時間
            var orderArrivalDate = Convert.ToDateTime(DateTime.Now.Year + $"/{orderTripInfo.date} {orderTripInfo.arrivalTime}");
            var tripsOrderByTime = trips.OrderBy(x => x.arrivalTime).ToList();
            //依照自訂方法排序
            tripsOrderByTime.Sort();
            //將車次列表轉成Datetime格式
              var tripsList = tripsOrderByTime.Select(x => new
                {
                    buttonName = x.buttonName,
                    arrivalTime = Convert.ToDateTime(DateTime.Now.Year + $"/{x.date} {x.arrivalTime}"),
                    startTime = Convert.ToDateTime(DateTime.Now.Year + $"/{x.date} {x.startTime}"),
                    totalTime = x.totalTime
                })
                //如果出發時間跟設定檔設定的目標出發時間相比，差超過一小時，就不要使用
                  .Where(x=>Math.Abs((x.startTime-ConfigTargetDate).TotalHours) < 1)
                .ToList();

            //找出比目前訂位紀錄的抵達時間早的車次 這個判斷可能會有問題 先拿掉好了 依搜尋到的結果為主
            // return tripsList.FirstOrDefault(x => x.arrivalTime < orderArrivalDate)?.buttonName;
            return tripsList.FirstOrDefault()?.buttonName;
        }
    }
}
