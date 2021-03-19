using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THSRCrawler.ScheduleJob
{
    /// <summary>
    /// 用來通知尚未付款的車票
    /// </summary>
    public class UnPaidAlertJob: IScheduledTask
    {
        private readonly ILogger<ModifyTripJob> _logger;
        private readonly Crawler _crawler;
        private readonly Config _config;

        public UnPaidAlertJob(ILogger<ModifyTripJob> logger, Crawler crawler, IOptions<Config> config)
        {
            _logger = logger;
            _crawler = crawler;
            _config = config.Value;
            IsShuttingDown = _config.IsEnableSchedule;
        }

        public bool IsShuttingDown { get; set; }

        public async Task RunAsync()
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            _logger.LogInformation("Running UnPaidAlertJob.");
        }
    }
}
