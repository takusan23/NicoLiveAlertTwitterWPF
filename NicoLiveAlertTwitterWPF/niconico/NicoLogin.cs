using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoLogin
    {
        public void niconicoLogin(string mail, string pass)
        {
            //niconicoログイン
            // 
            var cookie_container = new CookieContainer();
            using (var h = new HttpClientHandler() { CookieContainer = cookie_container })
            using (var c = new HttpClient(h) { BaseAddress = new Uri("https://secure.nicovideo.jp/secure/login?site=niconico") })
            {
                var content = new FormUrlEncodedContent(
                    new Dictionary<string, string> {
            { "next_url", "" },
            { "mail", mail },
            { "password", pass }
                    }
                );
                c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "NicoLiveAlert_Twitter;@takusan_23");
                var login = c.PostAsync("", content).Result;

            }

            foreach (Cookie cookie in cookie_container.GetCookies(new Uri("https://secure.nicovideo.jp/secure/login?site=niconico")))
            {
                if (cookie.Name == "user_session")
                {
                    Properties.Settings.Default.user_session = cookie.Value;
                    //一応メアドも保存
                    Properties.Settings.Default.mail = mail;
                    Properties.Settings.Default.pass = pass;
                    Properties.Settings.Default.Save();

                    //ログイン成功ダイアログ
                    MessageBox.Show($"ログインに成功しました。");

                }
            }
        }

        public void ReNiconicoLogin()
        {
            //2回目以降はどうぞ
            var mail = Properties.Settings.Default.mail;
            var pass = Properties.Settings.Default.pass;
            niconicoLogin(mail, pass);
        }
    }
}
