using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace THSRCrawler
{
    public class HTMLParser
    {
        private readonly IHtmlParser _parser;
        private readonly Config _config;
        private readonly ILogger<HTMLParser> _logger;
        public HTMLParser(IHtmlParser htmlParser, ILogger<HTMLParser> logger, IOptions<Config> config)
        {
            _parser = htmlParser;
            _config = config.Value;
            _logger = logger;
        }

        public CrawlerModels.orderPageInfo GetOrderInformation(string html)
        {
            var _parser = new HtmlParser();
            var result = new CrawlerModels.orderPageInfo();
            var dom = _parser.ParseDocument(html);
            var mainContent = dom.GetElementById("content");
            ValidSessionExpire(mainContent);
            CheckPageError(dom);

            var allrow = mainContent.QuerySelectorAll(".table_simple tbody tr");
            foreach (var row in allrow)
            {
                var title = row.QuerySelector(".section_subtitle");
                if (title != null && Regex.IsMatch(title.TextContent, "去程|回程"))
                {
                    var tds = row.ChildNodes.ToList().Select(x => x.TextContent).Where(x => !string.IsNullOrEmpty(x?.Trim())).ToList();
                    var tripInfo = new CrawlerModels.tripInfo();
                    tripInfo.tripType = tds[0];
                    tripInfo.date = tds[1];
                    tripInfo.trainNo = tds[2];
                    tripInfo.startStation = tds[3];
                    tripInfo.arrivalStation = tds[4];
                    tripInfo.startTime = tds[5];
                    tripInfo.arrivalTime = tds[6];
                    tripInfo.price = tds[7];
                    tripInfo.seat = tds[8];
                    result.trips.Add(tripInfo);
                };
            }
            result.isRoundTrip = result.trips.Any(x => x.tripType == "回程");
            result.isTripEditable = dom.QuerySelector("input[value='變更行程']") != null ;
            var form = dom.QuerySelector("form");
            result.nextStepUrl = form.GetAttribute("action");
            var paymentRow = dom.QuerySelector(".table_details  tr  td:contains('交易狀態')");
            var paymentDetail = paymentRow.NextElementSibling.QuerySelectorAll("span > span").Select(x=>x.TextContent);
            result.isAlreadPaid = paymentDetail.Any(x=>x== "已付款");
            if (!result.isAlreadPaid)
            {
                result.paymentDeadLine = paymentDetail.FirstOrDefault(x => Regex.IsMatch(x, @"\d+\/\d+"));
            }

            return result;
        }

        //如果已付款的訂單，要變更坐位時會有confirm message
        public CrawlerModels.modifyResult ModifyOrderResult(string html)
        {
            var dom = _parser.ParseDocument(html);
            CheckPageError(dom);
            var mainContent = dom.GetElementById("content");
            ValidSessionExpire(mainContent);
            var content = dom.QuerySelector(".payment_title");
            switch (content.InnerHtml)
            {
                case string a when a.Contains("您確定要變更本筆訂位嗎"):
                    return CrawlerModels.modifyResult.needConfirm;
                case string a when a.Contains("行程變更成功"):
                    return CrawlerModels.modifyResult.success;
            }

            return CrawlerModels.modifyResult.fail;

        }

        public string GetNextStepUrl(string html)
        {
            var dom = _parser.ParseDocument(html);
            var form = dom.QuerySelector("form");
            return form.GetAttribute("action");
        }

        public CrawlerModels.modifyTripPageInfo GetModifyPageInformation(string html)
        {
            var result = new CrawlerModels.modifyTripPageInfo();

            return result;
        }


        public void CheckPageError(string html)
        {
            var dom = _parser.ParseDocument(html);
            CheckPageError(dom);
        }

        private void CheckPageError(IHtmlDocument dom)
        {
            var msg = "";
            var mainContent = dom.GetElementsByClassName("feedbackPanelERROR");
            if (mainContent.Any())
            {
                msg = mainContent.First().FirstElementChild.TextContent;
                if (msg.Contains("查無訂位紀錄"))
                {
                    throw new CritialPageErrorException(msg);
                }

                throw new ArgumentException(msg);
            }
            
        }

        public void ValidSessionExpire(IElement content)
        {
            if (content == null) return;
            var txt = content.QuerySelectorAll(".standard_text");
            if (txt.Length == 0) return;
            if (txt.First().InnerHtml.Contains("無法繼續提供您訂票的服務"))
            {
                throw new ArgumentException("Session已過期，請重新嘗試");
            }
        }

        //判斷行程是否可修改
        public CrawlerModels.orderStatus CheckModifyFormIsAvailable(string html)
        {
            var dom = _parser.ParseDocument(html);
            CheckPageError(dom);
            var Trip_to = dom.GetElementById("toTimeInputField").GetAttribute("disabled") == "disabled";
            var Trip_back = dom.GetElementById("backTimeInputField").GetAttribute("disabled") == "disabled";
            if (!Trip_to && !Trip_back)
                return CrawlerModels.orderStatus.BothAvailable;
            else if (!Trip_to && Trip_back)
                return CrawlerModels.orderStatus.onlyTo;
            else if (Trip_to && !Trip_back)
                return CrawlerModels.orderStatus.onlyBack;
            else
                return CrawlerModels.orderStatus.BothCannot;
        }


        //取得此頁面所有的車次
        public IEnumerable<CrawlerModels.Trips> GetTripsPerPageAndHandleError(string html,CrawlerModels.ModifyTripType tripType)
        {
            var trips = new List<CrawlerModels.Trips>();
			var dom = _parser.ParseDocument(html);
            CheckPageError(dom);

            //主要的大區塊，包含了去、回程，跟訂位的資料
            var mainContent = dom.GetElementById("content");
            ValidSessionExpire(mainContent);
            var panelName = "HistoryDetailsModifyTripS2Form_TrainQueryDataViewPanel";
            panelName = tripType == CrawlerModels.ModifyTripType.Back ? panelName + "2" : panelName;
            var allSection = mainContent.QuerySelectorAll($"#{panelName} .section_title");
            //section是去、回程table上面的title
            foreach (var section in allSection)
            {
                if (Regex.IsMatch(section.FirstElementChild.TextContent, "去程|回程"))
                {
                    //取出此section 的next dom ,存放車次資料的table
                    var alltr = section.ParentElement
                        .QuerySelector(".table_simple tbody")
                        .QuerySelectorAll(":scope>tr:not(:first-child)");
                    
                    foreach (var tr in alltr)
                    {
                        var tripRowElement = tr.Children.Select(x =>
                        {
                            if (x.FirstChild.NodeName == "INPUT")
                            {
                                return x.FirstElementChild.GetAttribute("value");
                            }

                            return x.TextContent;
                        }).Where(x => !string.IsNullOrEmpty(x?.Trim())).ToList();

                        var totalTime = DateTime.ParseExact(tripRowElement[4], "h:mm",
                           null);

                        trips.Add(new CrawlerModels.Trips()
                        {
                            buttonName = tripRowElement[0],
                            train = int.Parse(tripRowElement[1]),
                            startTime = tripRowElement[2],
                            arrivalTime = tripRowElement[3],
                            totalTime = (int)totalTime.TimeOfDay.TotalMinutes,
                            date = tripRowElement[5]

                        });
                    }


                    
                }
            }

            return trips.Where(x=>x.buttonName != null && x.totalTime <= _config.MaxETA).OrderBy(x=>x.arrivalTime);
        }
    }
}
