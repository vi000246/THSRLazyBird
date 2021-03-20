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
            public int totalTime { get; set; }
            public string date { get; set; }
        }

        // 變更行程時選擇的類型
        public enum ModifyTripType
        {
            To,
            Back
        }

        public class orderPageInfo
        {
            public bool isRoundTrip { get; set; }
            public bool isTripEditable { get; set; }
            public bool isAlreadPaid { get; set; }
            //付款期限 格式mm/dd
            public string paymentDeadLine { get; set; }
            public List<tripInfo> trips { get; set; } = new List<tripInfo>();
        }

        public class tripInfo
        {
            public string tripType { get; set; }
            public string date { get; set; }
            public string trainNo { get; set; }
            public string startStation { get; set; }
            public string arrivalStation { get; set; }
            public string startTime { get; set; }
            public string arrivalTime { get; set; }
            public string price { get; set; }
            public string seat { get; set; }

        }

        public class modifyTripPageInfo
        {
            public bool is_To_Trip_Editable { get; set; }
            public bool is_Back_Trip_Editable { get; set; }
        }

        public static Dictionary<string, string> timeDropdown = new Dictionary<string, string>()
        {
            {"00:00","1201A"},
            {"00:30","1230A"},
            {"05:00","500A"},
            {"05:30","530A"},
            {"06:00","600A"},
            {"06:30","630A"},
            {"07:00","700A"},
            {"07:30","730A"},
            {"08:00","800A"},
            {"08:30","830A"},
            {"09:00","900A"},
            {"09:30","930A"},
            {"10:00","1000A"},
            {"10:30","1030A"},
            {"11:00","1100A"},
            {"11:30","1130A"},
            {"12:00","1200N"},
            {"12:30","1230P"},
            {"13:00","100P"},
            {"13:30","130P"},
            {"14:00","200P"},
            {"14:30","230P"},
            {"15:00","300P"},
            {"15:30","330P"},
            {"16:00","400P"},
            {"16:30","430P"},
            {"17:00","500P"},
            {"17:30","530P"},
            {"18:00","600P"},
            {"18:30","630P"},
            {"19:00","700P"},
            {"19:30","730P"},
            {"20:00","800P"},
            {"20:30","830P"},
            {"21:00","900P"},
            {"21:30","930P"},
            {"22:00","1000P"},
            {"22:30","1030P"},
            {"23:00","1100P"},
            {"23:30","1130P"},
        };
    }
}
