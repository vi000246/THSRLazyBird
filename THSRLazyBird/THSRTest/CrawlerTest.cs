using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using THSRCrawler;

namespace THSRTest
{
    public class CrawlerTest
    {
        private static IWebHost _webHost = null;
        private Crawler _crawler;

        public CrawlerTest() {
            _webHost = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .Build();
            _crawler = GetService<Crawler>();
        }

        static T GetService<T>()
        {
            var scope = _webHost.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<T>();
        }


        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void test() {
            _crawler.init();
        }
    }
}
