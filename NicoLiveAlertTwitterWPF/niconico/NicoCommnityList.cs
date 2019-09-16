using AngleSharp;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoCommnityList
    {

        List<string> communityIDList = new List<string>();

        //参加中のコミュニティ全て取得
        public async void getFollowCommunity(string url, AutoAdmission.AutoAddAdmission autoAddAdmission)
        {
            List<NicoFavListJSON> accountJSONArray;
            if (Properties.Settings.Default.autoadd_community != "")
            {
                var account_list = Properties.Settings.Default.autoadd_community;
                accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>(account_list);
                foreach (var community in accountJSONArray)
                {
                    communityIDList.Add(community.ID);
                }
            }
            else
            {
                accountJSONArray = new List<NicoFavListJSON>();
            }

            if (Properties.Settings.Default.user_session != "")
            {
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

                var client = new HttpClient(header);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "NicoLiveAlert_Twitter;@takusan_23");
                using (var stream = await client.GetAsync(new Uri(url)))
                {
                    //var html = await client.GetStringAsync(urlstring);

                    if (stream.StatusCode == HttpStatusCode.OK)
                    {

                        var parser = new HtmlParser();
                        var jsonString = await stream.Content.ReadAsStringAsync();
                        var doc = await parser.ParseDocumentAsync(jsonString);

                        var table = doc.Body.GetElementsByClassName("md-cmn_communities_frm")[0].GetElementsByTagName("li");
                        //一つ一つ
                        foreach (var li in table)
                        {
                            var communityID = li.GetElementsByClassName("profile")[0].GetElementsByTagName("a")[0].GetAttribute("href");
                            communityID = communityID.Replace("/community/", "");

                            //追加する
                            //あるかも！？
                            if (!communityIDList.Contains(communityID))
                            {
                                accountJSONArray.Add(new NicoFavListJSON { ID = communityID });
                            }
                            //保存
                            Properties.Settings.Default.autoadd_community = JsonConvert.SerializeObject(accountJSONArray);
                            Properties.Settings.Default.Save();
                        }

                        autoAddAdmission.loadAutoAddAutoAdmissionList();

                        //次のページは？
                        var next = doc.Body.GetElementsByClassName("next")[0];
                        if (next.GetAttribute("href") != null)
                        {
                            getFollowCommunity("https://com.nicovideo.jp/" + next.GetAttribute("href"), autoAddAdmission);
                        }
                    }
                }
            }
        }
    }
}
