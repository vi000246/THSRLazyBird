using System.Collections.Generic;
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
            var tripType = Models.ModifyTripType.To;
            var trips = new List<Models.Trips>();
            var orderInfo = new Models.orderPageInfo();
            _tripCompare.FindMatchTrip(tripType,trips,orderInfo);
        }
    }
}
