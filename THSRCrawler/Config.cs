using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace THSRCrawler
{
    public class Config
    {
        private readonly IConfiguration _config;
        public Config(IConfiguration config)
        {
            _config = config;
        }

        public List<(string IdCard, string OrderId)> GetOrders()
        {
            var orders = _config.GetSection("TicketOrders")
                .GetChildren()
                .ToList()
                .Select(x =>
                    (
                        x.GetValue<string>("IDCard"),
                        x.GetValue<string>("OrderId")
                    )
                )
                .ToList<(string IdCard, string OrderId)>();
            return orders;
        }
    }
}
