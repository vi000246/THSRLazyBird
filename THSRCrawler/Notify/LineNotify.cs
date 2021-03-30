using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LexLibrary.Line.NotifyBot;
using LexLibrary.Line.NotifyBot.Models;
using Microsoft.Extensions.Options;
using THSRCrawler.CustomException;

namespace THSRCrawler
{
    public class LineNotify: INotify
    {
        private readonly LineNotifyBotApi _lineNotifyBotApi;
        private readonly Config _config;

        public LineNotify(LineNotifyBotApi _lineNotifyBotApi, IOptions<Config> _config)
        {
            this._lineNotifyBotApi = _lineNotifyBotApi;
            this._config = _config.Value;
        }

        public void SendMsg(string message,string title=null)
        {
            try
            {
                title = string.IsNullOrEmpty(title) ? "" : title;
                foreach (var accessToken in _config.notify.linebot.AccessToken)
                {

                    _lineNotifyBotApi.Notify(new NotifyRequestDTO
                    {
                        AccessToken = accessToken,
                        Message = $"{title}:\r\n{message}"
                    });
                }
            }
            catch (Exception ex)
            {
                throw new NotifyException(ex.Message);
            }

        }
    }
}
