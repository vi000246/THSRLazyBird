using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace THSRCrawler
{
    public class Config
    {
        public List<TicketOrders> ticketOrders { get; set; }
        public bool IsEnableSchedule { get; set; }
        public int MaxETA { get; set; }

        public Config()
        {
            validateConfig();
        }

        public void validateConfig()
        {
            //start date不能晚於back date
            //如果start date=back date,驗證start time不能晚於back time
            //驗證時間格式
        }

    }

    public class TicketOrders
    {
        public string IdCard { get; set; }
        public string OrderId { get; set; }
        public string TripToDate { get; set; }
        public string TripEndDate { get; set; }
        public string TripToTime { get; set; }
        public string TripBackTime { get; set; }
    }
}
