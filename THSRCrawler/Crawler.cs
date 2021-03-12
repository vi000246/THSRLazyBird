using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace THSRCrawler
{
    public class Crawler
    {
        RequestClient client = new RequestClient();
        //測試用訂位代號 03352429 
        public void init()
        {
           client.Login(); 
        }

        //變更行程
        //取得可用車次列表
        //重新訂票
    }
}
