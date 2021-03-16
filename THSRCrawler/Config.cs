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

    }

    public class TicketOrders
    {
        public string IdCard { get; set; }
        public string OrderId { get; set; }
    }
}
