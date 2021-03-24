using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NETCore.MailKit.Core;
using THSRCrawler.CustomException;

namespace THSRCrawler
{
    public class EmailNotify:INotify
    {
        private readonly IEmailService _emailService;
        private readonly Config _config;

        public EmailNotify(IOptions<Config> _config, IEmailService emailService )
        {
            _emailService = emailService;
            this._config = _config.Value;
        }

        public async void SendMsg(string message,string title=null)
        {
            try
            {
                title = string.IsNullOrEmpty(title) ? "" : title;
                _emailService.Send(buildEmailAddress(), $"[THSRLazyBird] {title}",
                    message);
            }
            catch (Exception ex)
            {
                throw new NotifyException(ex.Message);
            }
        }

        private string buildEmailAddress()
        {
            var account = _config.notify.smtp.Account;
            return $"{account}@gmail.com";
        }

    }
}
