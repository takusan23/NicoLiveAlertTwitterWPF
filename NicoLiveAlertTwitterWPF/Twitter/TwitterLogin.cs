using NicoLiveAlertTwitterCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static CoreTweet.OAuth;

namespace NicoLiveAlertTwitterWPF.Twitter
{
    class TwitterLogin
    {
        OAuthSession session;
        //コンシューマーキー取得
        public String consumer_key = TwitterKey.consumer_key;
        public String consumer_secret = TwitterKey.consumer_secret;

        //認証画面出す
        public void showTwitterParge()
        {
            //認証画面をブラウザで開く。
            session = Authorize(consumer_key, consumer_secret);
            System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);
        }


        //PINコードからアクセストークン発行
        public void GetAccessToken(string pin)
        {
            //ダイアログの認証押したとき

            //アクセストークン取得
            var token = session.GetTokens(pin);

            //アクセストークンほぞん
            Properties.Settings.Default.consumer_key = consumer_key;
            Properties.Settings.Default.consumer_secret = consumer_secret;
            Properties.Settings.Default.access_token = token.AccessToken;
            Properties.Settings.Default.access_token_secret = token.AccessTokenSecret;
            Properties.Settings.Default.Save();

            //ログイン成功ダイアログ
            MessageBox.Show("ログインに成功しました。\n" + token.ScreenName);

        }
    }
}
