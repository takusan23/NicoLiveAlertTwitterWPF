using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoLiveHTMLProgram
    {
        //ニコ生の放送ページの中にあるJSONをスクレイピング
        //アニメの一挙放送、朝鮮中央テレビ、ニコニコニュースなどは公式チャンネルのためprograminfoでは取得できない（リダイレクトされる）
        //予約枠自動入場でアニメの一挙を追加できるように。
        public async Task<JSONClass.HTMLPRogramJSON> getProgramJSONData(string liveId)
        {
            var urlString = $"https://sp.live.nicovideo.jp/watch/{liveId}";

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
                    var htmlString = await stream.Content.ReadAsStringAsync();

                    var angleSharp = new HtmlParser();
                    var html = angleSharp.ParseDocument(htmlString);
                    //とる
                    var jsonString = html.GetElementsByTagName("script")[3].TextContent;
                    //デコード
                    jsonString = HttpUtility.UrlDecode(jsonString);

                    //JSON以外の文字が含まれているのでいらない部分を削る
                    jsonString = jsonString.Replace("window.__initial_state__ = \"", "");
                    jsonString = jsonString.Replace("locationBeforeTransitions\":null}}\";", "locationBeforeTransitions\":null}}");
                    jsonString = jsonString.Replace("window.__public_path__ = \"https://nicolive.cdn.nimg.jp/relive/sp/\";", "");

                    //パースする
                    return JsonConvert.DeserializeObject<JSONClass.HTMLPRogramJSON>(jsonString);
                }
            }
            return null;
        }
    }
}
