using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public Models.orderPageInfo GetOrderInformation(string html)
        {
            var _parser = new HtmlParser();
            var result = new Models.orderPageInfo();
            var dom = _parser.ParseDocument(html);
            var mainContent = dom.GetElementById("content");
            var allTitle = mainContent.QuerySelectorAll(".table_simple tbody tr .section_subtitle");
            result.isRoundTrip = allTitle.Select(x => x.TextContent).Any(x => x == "回程");
            result.isTripEditable = dom.QuerySelector("input[value='變更行程']") != null ;
            var paymentRow = dom.QuerySelector(".table_details  tr  td:contains('交易狀態')");
            var paymentDetail = paymentRow.NextElementSibling.QuerySelectorAll("span > span").Select(x=>x.TextContent);
            result.isAlreadPaid = paymentDetail.Any(x=>x== "已付款");
            if (!result.isAlreadPaid)
            {
                result.paymentDeadLine = paymentDetail.FirstOrDefault(x => Regex.IsMatch(x, @"\d+\/\d+"));
            }

            return result;
        }

        //驗證這個時段是否有車票
        public void ValidTimeIsAvailable()
        {

        }

        //驗證是否變更成功
        //變更行程頁面 驗證此筆訂位紀錄是否有回程 以及去/回程是否可變更


        //取得此頁面所有的車次
        public List<Models.Trips> GetTripsPerPage(string html,Models.ModifyTripType tripType)
        {
            var trips = new List<Models.Trips>();
			var dom = _parser.ParseDocument(html);
            //主要的大區塊，包含了去、回程，跟訂位的資料
            var mainContent = dom.GetElementById("content");
            //HistoryDetailsModifyTripS2Form_TrainQueryDataViewPanel應該會包含去回程的table，待確認
            var allSection = mainContent.QuerySelectorAll("#HistoryDetailsModifyTripS2Form_TrainQueryDataViewPanel .section_title");
            //section是去、回程table上面的title
            foreach (var section in allSection)
            {
                if (Regex.IsMatch(section.FirstElementChild.TextContent, "去程|回程"))
                {
                    //取出此section 的next dom ,存放車次資料的table
                    var alltr = section.ParentElement.QuerySelector(".table_simple tbody").QuerySelectorAll(":scope>tr");
                    
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

            return trips;
        }
    }
}
