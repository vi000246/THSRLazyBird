using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;

namespace THSRCrawler
{
    public class HTMLParser
    {
        //驗證此筆訂位紀錄是否可變更行程，或是查無座位
        public void ValidOrderIsEditable()
        {
            
        }

        //驗證這個時段是否有車票
        public void ValidTimeIsAvailable()
        {

        }

        //取得此頁面所有的車次
        public void GetTripsPerPage(string html)
        {

        }
    }
}
