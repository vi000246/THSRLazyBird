using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Fluent;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using THSRCrawler.CustomException;

namespace THSRCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            INotify notify = null;
            try
            {

                var host = CreateHostBuilder(args).Build();
                using var serviceScope = host.Services.CreateScope();
                var services = serviceScope.ServiceProvider;

                var myDependency = services.GetRequiredService<Crawler>();
                // //入口寫在這裡 以後註解掉，只用schedule跑
                // myDependency.init();
                notify = services.GetRequiredService<INotify>();
                host.Run();

            }
            catch (CritialPageErrorException e)
            {
                logger.Fatal(e.Message);
                notify.SendMsg(e.Message,"高鐵頁面錯誤訊息");
                throw;
            }
            catch (InvalidConfigException e)
            {
                logger.Fatal(e.Message);
                notify.SendMsg(e.Message,"設定檔錯誤");
                throw;
            }
            catch (NotifyException e)
            {
                logger.Fatal(e.Message);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                notify.SendMsg(e.Message);
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    })
                    .UseNLog();
                });

    }
}
