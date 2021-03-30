using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THSRCrawler
{
    public class ModifyTripJob: IScheduledTask
    {
        private readonly ILogger<ModifyTripJob> _logger;
        private readonly Crawler _crawler;
        private readonly Config _config;

        public ModifyTripJob(ILogger<ModifyTripJob> logger, Crawler crawler, IOptions<Config> config)
        {
            _logger = logger;
            _crawler = crawler;
            _config = config.Value;
            IsShuttingDown = !_config.IsEnableSchedule;
        }

        public bool IsShuttingDown { get; set; }

        public async Task RunAsync()
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            _logger.LogInformation("Running ModifyTripJob.");
            _crawler.init();
        }
    }
}
