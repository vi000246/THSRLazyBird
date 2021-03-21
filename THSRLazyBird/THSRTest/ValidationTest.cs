using NUnit.Framework;
using THSRCrawler;

namespace THSRTest
{
    public class ValidationTest
    {
        public readonly Validation _validation = new Validation();


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var config = new TicketOrders();
            var orderInfo = new Models.orderPageInfo();
            _validation.validConfigDateTime(config,orderInfo);
            Assert.Pass();
        }
    }
}