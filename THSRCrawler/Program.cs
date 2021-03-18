using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THSRCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                using var serviceScope = host.Services.CreateScope();
                var services = serviceScope.ServiceProvider;

                var myDependency = services.GetRequiredService<Crawler>();
                // //入口寫在這裡 以後註解掉，只用schedule跑
                myDependency.init();
                host.Run();

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
