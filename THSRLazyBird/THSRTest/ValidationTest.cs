using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using THSRCrawler;
using Microsoft.Extensions.Logging;

namespace THSRTest
{
    public class ValidationTest
    {
        public Validation _validation;


        [SetUp]
        public void Setup()
        {
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<Validation>();
            _validation = new Validation(logger);
        }

        [Test]
        public void checkEmptyConfig_to()
        {
            var config = new TicketOrders();
            config.targetDate.TripToTime = "";
            config.targetDate.TripToDate = "";

            var check = _validation.haveToDateFunc(config);
            Assert.IsFalse(check);
        }
        [Test]
        public void checkEmptyConfig_to_notEmpty()
        {
            var config = new TicketOrders();
            config.targetDate.TripToTime = "11:30";
            config.targetDate.TripToDate = "11/30";

            var check = _validation.haveToDateFunc(config);
            Assert.IsTrue(check);
        }

        [Test]
        public void checkEmptyConfig_back()
        {
            var config = new TicketOrders();
            config.targetDate.TripBackDate = "";
            config.targetDate.TripBackTime = "";

            var check = _validation.haveBackDateFunc(config);
            Assert.IsFalse(check);
        }
        [Test]
        public void checkEmptyConfig_back_notEmpty()
        {
            var config = new TicketOrders();
            config.targetDate.TripBackDate = "11/30";
            config.targetDate.TripBackTime = "11:30";

            var check = _validation.haveBackDateFunc(config);
            Assert.IsTrue(check);
        }

        [Test]
        public void validOrderIdAndIdCardTest()
        {
            try
            {
                var config = new TicketOrders();
                _validation.validOrderIdAndIdCard(config);
            }
            catch (InvalidConfigException ex)
            {
                Assert.True(ex.Message.Contains("訂位代號及身份證字號不得為空"));
            }
        }

        [Test]
        public void ConfigToDateExpireTest()
        {
            try
            {
                var TripToDate = DateTime.Now.AddDays(-1);
                _validation.validPassDate(TripToDate, "123");
                Assert.Fail();
            }
            catch (InvalidConfigException ex)
            {
                Assert.True(ex.Message.Contains("日期為過去時間"));
            }
        }
        [Test]
        public void ConfigDateFormatInvalid()
        {
            try
            {
                var date = "11/30";
                _validation.validDateFormat(date,"123");
                Assert.Fail();
            }
            catch (InvalidConfigException ex)
            {
                Assert.True(ex.Message.Contains("設定檔Date欄位格式錯誤"));
            }
        }
        [Test]
        public void ValidTripConfigAndOrderTime() {
            try
            {
                var tripInfo = new CrawlerModels.tripInfo();
                tripInfo.tripType = "去程";
                tripInfo.date = "2/1";
                tripInfo.arrivalTime = "12:00";
                var configDate = new DateTime(DateTime.Now.Year,1,3,12,50,00);
                _validation.compareTripDateAndConfigDate(tripInfo,configDate);
            }
            catch (InvalidConfigException ex)
            {
                Assert.True(ex.Message.Contains("設定檔的回程不得小於訂位紀錄的去程抵達時間"));
            }
        }
        [Test]
        public void ValidTripConfigAndOrderTime_2()
        {
            try
            {
                var tripInfo = new CrawlerModels.tripInfo();
                tripInfo.tripType = "回程";
                tripInfo.date = "2/1";
                tripInfo.startTime = "12:00";
                var configDate = new DateTime(DateTime.Now.Year, 3, 3, 12, 50, 00);
                _validation.compareTripDateAndConfigDate(tripInfo, configDate);
            }
            catch (InvalidConfigException ex)
            {
                Assert.True(ex.Message.Contains("設定檔的去程不得大於訂位紀錄的回程出發時間"));
            }
        }
    }
}