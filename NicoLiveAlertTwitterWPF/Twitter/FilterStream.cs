using CoreTweet;
using CoreTweet.Streaming;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using NicoLiveAlertTwitterWPF.ListViewClass;
using NicoLiveAlertTwitterWPF.niconico;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NicoLiveAlertTwitterWPF.Twitter
{
    class FilterStream
    {
        public CancellationToken cancelToken;
        public CancellationTokenSource tokenSource;
        /*        //Timeline
                UserActivitySession _currentActivity;
                AdaptiveCard card;
        */

        //履歴機能
        ProgramHistory.ProgramHistory programHistory = new ProgramHistory.ProgramHistory();

        //他の配信サイトでも利用できるように
        OtherLive.OtherLiveList otherLive = new OtherLive.OtherLiveList();

        public void connectFilterStream(MainWindow page)
        {
            //ログイン情報取得
            if (Properties.Settings.Default.consumer_key != "")
            {
                //スレッド止める時に使うらしい。
                tokenSource = new CancellationTokenSource();
                cancelToken = tokenSource.Token;

                var consumer_key = Properties.Settings.Default.consumer_key;
                var consumer_secret = Properties.Settings.Default.consumer_secret;
                var access_token = Properties.Settings.Default.access_token;
                var access_token_secret = Properties.Settings.Default.access_token_secret;
                var twitter = CoreTweet.Tokens.Create(consumer_key, consumer_secret, access_token, access_token_secret);

                //ID取得
                var account_list = Properties.Settings.Default.account_list;
                var accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>(account_list);
                List<long> ids = new List<long>();
                foreach (var id in accountJSONArray)
                {
                    ids.Add(long.Parse(id.ID));
                }

                //FilterStreamの検証用。
                //ids = getTakusan23Followers();

                //FilterStream
                Task task = new Task(async () =>
                {
                    try
                    {
                        Console.WriteLine("接続を開始");
                        var stream = twitter.Streaming.Filter(follow: ids).OfType<StatusMessage>().Select(x => x.Status);
                        foreach (var tw in stream)
                        {
                            //キャンセルされてるか
                            if (!cancelToken.IsCancellationRequested)
                            {
                                //キャンセルされていないときは続ける
                                //本人以外（RTなんかも拾ってしまう）のツイートには反応しない
                                if (ids.Contains(tw.User.Id ?? 0))
                                {
                                    /*
                                                                    Debug.WriteLine("-------------------");
                                                                    Debug.WriteLine(tw.User.Name);
                                                                    Debug.WriteLine(tw.Text);
                                                                    Debug.WriteLine("-------------------");
                                    */

                                    await page.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        //UIスレッドで動く

                                        //番組をブラウザで開いたかチェック
                                        var opend = false;

                                        //レスポンスのEntitiesにニコ生のURLがあるかも？
                                        var entities = tw.Entities.Urls;
                                        foreach (var url in entities)
                                        {
                                            if (!string.IsNullOrEmpty(findProgramId(url.ExpandedUrl)))
                                            {
                                                //ニコ生の場合は番組IDを正規表現で取り出す
                                                lunchBrowser(findProgramId(url.ExpandedUrl));
                                                showNotification(tw);
                                                setMicrosoftTimeline(tw);
                                                //履歴追加
                                                programHistory.addHistory(findProgramId(url.ExpandedUrl));
                                                opend = true;
                                                //通知
                                                page.NotifyIcon.ShowBalloonTip("番組が開始しました。入場します！", tw.Text, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
                                            }
                                            else
                                            {
                                                if (Properties.Settings.Default.setting_otherlive_mode != "")
                                                {
                                                    if (Boolean.Parse(Properties.Settings.Default.setting_otherlive_mode))
                                                    {
                                                        //ニコ生以外はこっちに来る。
                                                        //URLで探す。
                                                        foreach (var i in otherLive.urlList)
                                                        {
                                                            //forEachで回す
                                                            if (url.ExpandedUrl.Contains(i))
                                                            {
                                                                //あった！
                                                                opend = true;
                                                                //開く
                                                                launchBrowserOtherLive(url.ExpandedUrl);
                                                                //履歴追加
                                                                programHistory.addOtherLiveHistory(tw, url.ExpandedUrl);
                                                                //通知
                                                                page.NotifyIcon.ShowBalloonTip("番組が開始しました。入場します！", tw.Text, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
                                                            }
                                                        }
                                                        //もし無かったとき
                                                        if (opend == false)
                                                        {
                                                            //クライアント名で探す
                                                            foreach (var i in otherLive.clientList)
                                                            {
                                                                //forEachで回す
                                                                if (tw.Source.Contains(i))
                                                                {
                                                                    //あった！
                                                                    opend = true;
                                                                    launchBrowserOtherLive(url.ExpandedUrl);
                                                                    //履歴追加
                                                                    programHistory.addOtherLiveHistory(tw, url.ExpandedUrl);
                                                                    //通知
                                                                    page.NotifyIcon.ShowBalloonTip("番組が開始しました。入場します！", tw.Text, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (opend == false)
                                        {
                                            //上のEntities.urls[]で取れるはずだけど番組IDだけとかでも動くように
                                            //ここはニコ生だけ。
                                            if (!string.IsNullOrEmpty(findProgramId(tw.Text)))
                                            {
                                                lunchBrowser(findProgramId(tw.Text));
                                                showNotification(tw);
                                                //履歴追加
                                                programHistory.addHistory(findProgramId(tw.Text));
                                                setMicrosoftTimeline(tw);
                                                //通知
                                                page.NotifyIcon.ShowBalloonTip("番組が開始しました。入場します！", tw.Text, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
                                            }
                                        }
                                    }));
                                }
                            }
                            else
                            {
                                //キャンセルされたので終了
                                return;
                            }
                        }

                    }
                    catch (TwitterException e)
                    {
                        await page.Dispatcher.BeginInvoke((Action)(() =>
                         {
                             //ダイアログ
                             System.Windows.Forms.MessageBox.Show("リアルタイム更新を有効にできませんでした。\nTwitter APIの制限にかかった可能性があります。少し待ってみて下さい。", "エラー");
                         }));
                    }

                }, cancelToken);
                task.Start();
            }
            else
            {
                //ログインしてね
                showLoginMessage();
            }
        }



        private void setMicrosoftTimeline(Status tw)
        {
            /*            CreateAdaptiveCardForTimeline(tw);
                        var programId = findProgramId(tw.Text);
                        //タイムラインに追加する
                        var userChannel = UserActivityChannel.GetDefault();
                        var userActivity = await userChannel.GetOrCreateUserActivityAsync($"NicoLiveAlert_TwitterCS_{programId}");

                        //設定
                        userActivity.VisualElements.DisplayText = $"番組が開始しました。\n{tw.Text}";
                        userActivity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(card.ToJson());
                        userActivity.ActivationUri = new Uri("https://live2.nicovideo.jp/watch/" + programId);

                        //保存
                        await userActivity.SaveAsync();

                        _currentActivity?.Dispose();
                        _currentActivity = userActivity.CreateSession();
            */
        }

        private void CreateAdaptiveCardForTimeline(Status tw)
        {

            /*            // Create an adaptive card specifically to reference this app in Windows 10 Timeline.
                        card = new AdaptiveCard("1.0")
                        {
                            // Select a good background image.
                            BackgroundImage = new Uri(tw.User.ProfileImageUrlHttps)
                        };

                        // Add a heading to the card, which allows the heading to wrap to the next line if necessary.
                        var apodHeading = new AdaptiveTextBlock
                        {
                            Text = tw.Text,
                            Size = AdaptiveTextSize.Large,
                            Weight = AdaptiveTextWeight.Bolder,
                            Wrap = true,
                            MaxLines = 2
                        };
                        card.Body.Add(apodHeading);

                        // Add a description to the card, and note that it can wrap for several lines.
                        var apodDesc = new AdaptiveTextBlock
                        {
                            Text = tw.User.Name,
                            Size = AdaptiveTextSize.Default,
                            Weight = AdaptiveTextWeight.Lighter,
                            Wrap = true,
                            MaxLines = 3,
                            Separator = true
                        };
                        card.Body.Add(apodDesc);
            */
        }

        private void showLoginMessage()
        {
            //ダイアログ
            System.Windows.Forms.MessageBox.Show("ログインして下さい。");
        }


        //ブラウザ起動
        private void lunchBrowser(string programId)
        {
            if (!string.IsNullOrEmpty(programId))
            {
                System.Diagnostics.Process.Start("https://live2.nicovideo.jp/watch/" + programId);
            }
        }

        //他の配信サイト用ブラウザ起動
        private void launchBrowserOtherLive(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        //通知
        private void showNotification(Status tw)
        {

        }

        //ツイート内容から番組IDを取得する（正規表現）
        private String findProgramId(string text)
        {
            Match matche = Regex.Match(text, "(lv)([0-9]+)");
            if (Regex.IsMatch(text, "(lv)([0-9]+)"))
            {
                //一致した。
                return matche.Value;

            }
            else
            {
                return "";
            }
        }

        //私のフォロワー
        private List<long> getTakusan23Followers()
        {
            var consumer_key = Properties.Settings.Default.consumer_key;
            var consumer_secret = Properties.Settings.Default.consumer_secret;
            var access_token = Properties.Settings.Default.access_token;
            var access_token_secret = Properties.Settings.Default.access_token_secret;
            var twitter = CoreTweet.Tokens.Create(consumer_key, consumer_secret, access_token, access_token_secret);
            List<long> list = new List<long>();
            var follows = twitter.Followers.Ids(screen_name: "takusan__23");
            foreach (var id in follows)
            {
                list.Add(id);
            }
            return list;
        }
    }
}