using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Hangfire.Annotations;

namespace THSRCrawler
{
    public class HTMLParser
    {
        private readonly IHtmlParser _parser;
        public HTMLParser(IHtmlParser htmlParser)
        {
            _parser = htmlParser;
        }

        //訂位紀錄頁面 驗證此筆訂位紀錄是否可變更行程
        public void ValidOrderIsEditable()
        {
           //if html find 變更行程button 
        }

        //驗證這個時段是否有車票
        public void ValidTimeIsAvailable()
        {

        }

        //驗證是否變更成功
        //變更行程頁面 驗證此筆訂位紀錄是否有回程 以及去/回程是否可變更


        //取得此頁面所有的車次
        public void GetTripsPerPage(string html)
        {
			var dom = _parser.ParseDocument(html);
            //主要的大區塊，包含了去、回程，跟訂位的資料
            var mainContent = dom.GetElementById("content");
            //HistoryDetailsModifyTripS2Form_TrainQueryDataViewPanel應該會包含去回程的table，待確認
            var allSection = mainContent.QuerySelectorAll("#HistoryDetailsModifyTripS2Form_TrainQueryDataViewPanel .section_title");
            //section是去、回程table上面的title
            foreach (var section in allSection)
            {
                if (section.FirstElementChild.TextContent.Contains("去程"))
                {
                    //取出此section 的next dom ,存放車次資料的table
                    var alltr = section.ParentElement.QuerySelector(".table_simple tbody").QuerySelectorAll(":scope>tr");
                    var trips = new List<Models.Trips>();
                    foreach (var tr in alltr)
                    {
                        var tds = tr.QuerySelectorAll("td");
                        var result = new Models.Trips();

                        int index = 0;
                        foreach (var td in tds)
                        {
                            var span = td.QuerySelector("span");
                            if (span != null)
                            {
                                string text = span.TextContent;
                                if (index == 0)
                                {
                                    result.train = int.Parse(text);
                                }
                                else if (index == 1)
                                {
                                    result.startTime = text;
                                }
                                else if (index == 2)
                                {
                                    result.arrivalTime = text;
                                }else if (index == 3)
                                {
                                    result.totalTime = text;
                                }else if (index == 4)
                                {
                                    result.date = text;
                                }


                                index++;
                            }

                            var button = td.QuerySelector("input");
                            if (button != null)
                            {
                                result.buttonName = button.GetAttribute("value");
                            }
                        }
                        trips.Add(result);

                    }
                }
            }
		}
    }
}
