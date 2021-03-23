using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LexLibrary.Line.NotifyBot;
using LexLibrary.Line.NotifyBot.Models;
using Newtonsoft.Json;

namespace THSRCrawler
{
    public class HomeController : Controller
    {
        private readonly LineNotifyBotApi _lineNotifyBotApi = null;

        public HomeController(LineNotifyBotApi lineNotifyBotApi)
        {
            _lineNotifyBotApi = lineNotifyBotApi;
        }

        public async Task<IActionResult> Index(string code)
        {
            string callBackUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Url.Action("CallBack"));

            //如果有code參數，就取得AccessToken
            if (!string.IsNullOrWhiteSpace(code))
            {
                var apiResult = await _lineNotifyBotApi.Token(new TokenRequestDTO
                {
                    Code = code,
                    RedirectUri = callBackUrl
                });

                ViewBag.AccessToken = apiResult.Access_Token;
            }

            //產生授權連結
            ViewBag.ClientAuthorizeUrl = _lineNotifyBotApi.GenerateAuthorizeUrl(callBackUrl, "1234");

            return View();
        }

        public IActionResult CallBack(AuthorizeCallbackDTO callbackDTO)
        {
            // 可能是第三方攻擊
            if (callbackDTO.State != "1234")
            {
                return new UnauthorizedResult();
            }

            //點擊授權後會回傳code參數
            return RedirectToAction("Index", new { callbackDTO.Code });
        }

        public async Task<IActionResult> Notify(string accessToken, string message)
        {
            var apiResult = await _lineNotifyBotApi.Notify(new NotifyRequestDTO
            {
                AccessToken = accessToken,
                Message = message
            });

            ViewBag.ResultJson = JsonConvert.SerializeObject(apiResult, Formatting.Indented);

            return PartialView("Notify", accessToken);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
