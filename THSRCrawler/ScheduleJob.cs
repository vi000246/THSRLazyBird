using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THSRCrawler
{
    public class ScheduleJob: IScheduledTask
    {
        private readonly ILogger<ScheduleJob> _logger;
        private readonly Crawler _crawler;
        private readonly Config _config;

        public ScheduleJob(ILogger<ScheduleJob> logger, Crawler crawler, IOptions<Config> config)
        {
            _logger = logger;
            _crawler = crawler;
            _config = config.Value;
        }

        public bool IsShuttingDown { get; set; }

        public async Task RunAsync()
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            _logger.LogInformation("Running schedule job.");
            if(_config.IsEnableSchedule)
            _crawler.init();
        }
    }
}
