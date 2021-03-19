using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THSRCrawler
{
    public class Models
    {
        public class Trips
        {
            //button name是uniqle的，每個頁面的button都不會重覆
            public string buttonName { get; set; }
            //車次代號
            public int train { get; set; }
            public string startTime { get; set; }
            public string arrivalTime { get; set; }
            public string totalTime { get; set; }
            public string date { get; set; }
        }
    }
}
