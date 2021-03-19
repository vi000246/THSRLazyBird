using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Hangfire.Annotations;
using Microsoft.Extensions.Logging;
using NLog;

namespace THSRCrawler
{
    public class HTMLParser
    {
        private readonly IHtmlParser _parser;
        private readonly ILogger<HTMLParser> _logger;
        public HTMLParser(IHtmlParser htmlParser, ILogger<HTMLParser> logger)
        {
            _parser = htmlParser;
            _logger = logger;
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

        public Models.modifyTripPageInfo GetModifyPageInformation(string html)
        {
            var result = new Models.modifyTripPageInfo();

            return result;
        }


        //用來驗證變更行程頁面的錯誤，輸入錯的日期、不能早於出發時間、查無該時段車票等
        public (bool isValid, string msg) ValidModifyTripError(IHtmlDocument dom)
        {
            var msg = "";
            var isValid = true;
            var mainContent = dom.GetElementsByClassName("feedbackPanelERROR");
            if (mainContent.Any())
            {
                msg = mainContent.First().FirstElementChild.TextContent;
                isValid = false;
            }

            return (isValid, msg);
        }


        //取得此頁面所有的車次
        public IEnumerable<Models.Trips> GetTripsPerPageAndHandleError(string html,Models.ModifyTripType tripType)
        {
            var trips = new List<Models.Trips>();
			var dom = _parser.ParseDocument(html);
            var validResult = ValidModifyTripError(dom);
            if (!validResult.isValid)
            {
               _logger.LogInformation(validResult.msg);
               throw new ArgumentException(validResult.msg);
            }

            //主要的大區塊，包含了去、回程，跟訂位的資料
            var mainContent = dom.GetElementById("content");
            var panelName = "HistoryDetailsModifyTripS2Form_TrainQueryDataViewPanel";
            panelName = tripType == Models.ModifyTripType.Back ? panelName + "2" : panelName;
            var allSection = mainContent.QuerySelectorAll($"#{panelName} .section_title");
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

            return trips.Where(x=>x.buttonName != null);
        }
    }
}
