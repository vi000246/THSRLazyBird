using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using DNTScheduler.Core;
using LexLibrary.Line.NotifyBot;
using LexLibrary.Line.NotifyBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using THSRCrawler.ScheduleJob;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace THSRCrawler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //用來將cookieContainer注入到其他calss,方便操作cookie
            var container = new CookieContainer();
            services.AddSingleton(container);

            //http client的設定
            services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                //設置SSL驗證
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                },
                //關掉自動redirect，因為第一次request會回傳302  要再get一次才會是正確的html content
                AllowAutoRedirect = true,
                CookieContainer = container,
                //表頭有gzip 要解壓
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = true,
            });

            //schedule設定
            services.AddDNTScheduler(options =>
            {
                // 好像是用來讓heroku之類的server keep alive的
                // options.AddPingTask(siteRootUrl: "https://localhost:5001");

                options.AddScheduledTask<ModifyTripJob>(utcNow => utcNow.Second == 1);
                options.AddScheduledTask<UnPaidAlertJob>(utcNow => utcNow.Hour == 22 && utcNow.Minute == 0 && utcNow.Second ==0);
            });
            var config = new Config();

            Configuration.GetSection("Config").Bind(config);
            //config註冊
            var configSection =
                Configuration.GetSection("Config");
            services.Configure<Config>(configSection);

            //addMvc好像過時了，找時間再來研究
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //line notify套件
            
            services.AddLineNotifyBot(new LineNotifyBotSetting
            {
                ClientID = config.notify.linebot.ClientID,
                ClientSecret = config.notify.linebot.ClientSecret,
                AuthorizeApi = "https://notify-bot.line.me/oauth/authorize",
                TokenApi = "https://notify-bot.line.me/oauth/token",
                NotifyApi = "https://notify-api.line.me/api/notify",
                StatusApi = "https://notify-api.line.me/api/status",
                RevokeApi = "https://notify-api.line.me/api/revoke"
            });
                        services.AddTransient<RequestClient>();
            services.AddTransient<Crawler>();
            services.AddTransient<Config>();
            services.AddTransient<HTMLParser>();
            services.AddTransient<Validation>();
            services.AddTransient<TripCompare>();
            services.AddTransient<IHtmlParser,HtmlParser>();
            services.AddTransient<INotify,LineNotify>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDNTScheduler();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                     "default",
                     "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
