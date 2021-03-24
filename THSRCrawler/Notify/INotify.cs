using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THSRCrawler
{
    interface INotify
    {
        public void SendMsg(string message,string title = null);
    }
}
