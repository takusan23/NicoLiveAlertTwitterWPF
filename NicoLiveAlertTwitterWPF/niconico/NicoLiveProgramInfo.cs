using Newtonsoft.Json;
using NicoLiveAlertTwitterCS.JSONClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoLiveProgramInfo
    {
        //番組情報API
        public async Task<NicoProgramInfoRootObject> getProgramInfo(string liveId)
        {
            if (Properties.Settings.Default.user_session != "")
            {
                var urlString = "https://live2.nicovideo.jp/watch/" + liveId + "/programinfo";

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
                        var jsonString = await stream.Content.ReadAsStringAsync();
                        var jsonObject = JsonConvert.DeserializeObject<NicoProgramInfoRootObject>(jsonString);
                        return jsonObject;
                    }
                }
            }
            return null;
        }
    }
}
