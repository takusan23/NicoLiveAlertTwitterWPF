using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoLiveGetPlayerStatus
    {
        //getPlayerStatusを叩く。
        //アニメの一挙放送、朝鮮中央テレビ、ニコニコニュースなどは公式チャンネルのためprograminfoでは取得できない（リダイレクトされる）
        //getplayerstatusだと公式チャンネルの放送でも取得できる。
        //でもニコ生アラート（本家）が亡くなった今、このAPIもいつまで使えるか不明。
        public async Task<AngleSharp.Html.Dom.IHtmlDocument> getPlayerStatus(string liveId)
        {
            //https通信が使えるようになってた
            var urlString = $"https://live.nicovideo.jp/api/getplayerstatus?v={liveId}";

            //user_session
            var user_session = Properties.Settings.Default.user_session;

            //Cookieをせっと（user_session）
            var cookieContainer = new CookieContainer();
            var cookie = new Cookie();
            cookie.Name = "user_session";
            cookie.Value = user_session;
            cookie.Domain = ".nicovideo.jp";
            cookieContainer.Add(cookie);

            var header = new HttpClientHandler();
            header.CookieContainer = cookieContainer;

            // 指定したサイトのHTMLをストリームで取得する
            var client = new HttpClient(header);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "NicoLiveAlert_Twitter;@takusan_23");
            using (var stream = await client.GetAsync(new Uri(urlString)))
            {
                if (stream.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var xmlString = await stream.Content.ReadAsStringAsync();
                    Console.WriteLine(xmlString);
                    //パースする
                    var parser = new HtmlParser();
                    return await parser.ParseDocumentAsync(xmlString);
                }
            }
            return null;
        }
    }
}
