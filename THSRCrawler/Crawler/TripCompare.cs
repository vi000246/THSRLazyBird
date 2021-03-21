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

        public string FindMatchTrip(Models.ModifyTripType tripType, IEnumerable<Models.Trips> trips, Models.orderPageInfo orderInfo)
        {
            var tripTitle = "去程";
            if (tripType == Models.ModifyTripType.Back)
                tripTitle = "回程";
            //目前訂單的行程資訊
            var orderTripInfo = orderInfo.trips.First(x => x.tripType == tripTitle);
            //組出行程的抵達時間


            //如果目前訂位日期跟設定檔日期一樣，就判斷抵達時間
            //如果不一樣，就挑一個行車時間最短的

            return trips.FirstOrDefault().buttonName;
        }
    }
}
