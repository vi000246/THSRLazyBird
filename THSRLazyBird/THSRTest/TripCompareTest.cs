using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using THSRCrawler;

namespace THSRTest
{
    public class TripCompareTest
    {
        private TripCompare _tripCompare = new TripCompare();

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void test() {
            var trips = new List<CrawlerModels.Trips>();
            trips.Add(new CrawlerModels.Trips() { buttonName = "4",startTime="15:30", arrivalTime = "16:25", totalTime = 145, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "5",startTime= "15:30", arrivalTime = "16:05", totalTime = 105, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "6",startTime= "15:30", arrivalTime = "16:45", totalTime = 130, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "7",startTime= "15:30", arrivalTime = "17:25", totalTime = 145, date = "11/30" });

            var tripInfo = new CrawlerModels.tripInfo();
            tripInfo.date = "11/30";
            tripInfo.arrivalTime = "16:40";
            var configTargetDate = new DateTime(DateTime.Now.Year,11,30,16,00,00);
            var buttonName = _tripCompare.FindMatchTrip(trips, tripInfo, configTargetDate);
            Assert.IsTrue(buttonName == "5");

            var tripInfo2 = new CrawlerModels.tripInfo();
            tripInfo2.date = "11/30";
            tripInfo2.arrivalTime = "16:00";
            var buttonName2 = _tripCompare.FindMatchTrip(trips, tripInfo2, configTargetDate);
            Assert.IsNull(buttonName2);
        }
        [Test]
        public void TargetTimeNoMatchTest()
        {
            var trips = new List<CrawlerModels.Trips>();
            trips.Add(new CrawlerModels.Trips() { buttonName = "4", startTime = "15:30", arrivalTime = "16:25", totalTime = 145, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "5", startTime = "15:30", arrivalTime = "16:05", totalTime = 105, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "6", startTime = "15:30", arrivalTime = "16:45", totalTime = 130, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "7", startTime = "15:30", arrivalTime = "17:25", totalTime = 145, date = "11/30" });

            var tripInfo = new CrawlerModels.tripInfo();
            tripInfo.date = "11/30";
            tripInfo.arrivalTime = "16:40";
            var configTargetDate = new DateTime(DateTime.Now.Year, 11, 30, 13, 00, 00);

            var tripInfo2 = new CrawlerModels.tripInfo();
            tripInfo2.date = "11/30";
            tripInfo2.arrivalTime = "16:00";
            var buttonName2 = _tripCompare.FindMatchTrip(trips, tripInfo2, configTargetDate);
            Assert.IsNull(buttonName2);
        }

        [Test]
        public void SortTest()
        {
            var trips = new List<CrawlerModels.Trips>();
            trips.Add(new CrawlerModels.Trips() { buttonName = "4", arrivalTime = "16:25", totalTime = 145, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "5", arrivalTime = "16:05", totalTime = 105, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "6", arrivalTime = "16:45", totalTime = 130, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "7", arrivalTime = "17:25", totalTime = 145, date = "11/30" });
            trips.Sort();
            Assert.IsTrue(trips.First().buttonName=="5");
        }

        [Test]
        public void SortTest_2()
        {
            var trips = new List<CrawlerModels.Trips>();
            trips.Add(new CrawlerModels.Trips() { buttonName = "1", arrivalTime = "21:00", totalTime = 130, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "2", arrivalTime = "21:25", totalTime = 145, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "3", arrivalTime = "21:25", totalTime = 145, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "4", arrivalTime = "21:20", totalTime = 130, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "5", arrivalTime = "21:05", totalTime = 105, date = "11/30" });
            trips.Add(new CrawlerModels.Trips() { buttonName = "6", arrivalTime = "21:45", totalTime = 130, date = "11/30" });
            trips.Sort();
            Assert.IsTrue(trips.First().buttonName == "5");
        }
    }
}
