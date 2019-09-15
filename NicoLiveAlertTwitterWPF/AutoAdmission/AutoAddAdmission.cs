﻿using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using NicoLiveAlertTwitterCS.JSONClass;
using NicoLiveAlertTwitterWPF.JSONClass;
using NicoLiveAlertTwitterWPF.ListViewClass;
using NicoLiveAlertTwitterWPF.niconico;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;

namespace NicoLiveAlertTwitterWPF.AutoAdmission
{
    class AutoAddAdmission
    {
        //ListViewデータ
        public ObservableCollection<AutoAddAutoAdmissionListViewData> list = new ObservableCollection<AutoAddAutoAdmissionListViewData>();


        //定期的にニコレポを見る
        public DispatcherTimer nicorepoTimer;

        //定期的にフォロー中番組を見る
        public DispatcherTimer followTimer;

        public void loadAutoAddAutoAdmissionList()
        {
            list.Clear();
            //コミュニティリスト読み込み
            if (Properties.Settings.Default.autoadd_community != "")
            {
                //存在チェック通過
                var account_list = Properties.Settings.Default.autoadd_community;
                var jsonArray = JsonConvert.DeserializeObject<List<AutoAddAutoAdmissionListViewData>>(account_list);
                var pos = 0;
                foreach (var community in jsonArray)
                {
                    var item = new AutoAddAutoAdmissionListViewData { ID = community.ID, Pos = pos };
                    list.Add(item);
                    pos += 1;
                }
            }
        }

        public void addAutoAddAutoAdmissionList(string communityID)
        {
            ///予約枠自動登録自動入場リスト追加
            if (Properties.Settings.Default.autoadd_community != "")
            {
                //追加
                var account_list = Properties.Settings.Default.autoadd_community;
                var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAddAutoAdmissionListViewData>>(account_list);
                accountJSONArray.Add(new AutoAddAutoAdmissionListViewData { ID = communityID });
                //JSON配列に変換
                Properties.Settings.Default.autoadd_community = JsonConvert.SerializeObject(accountJSONArray);
            }
            else
            {
                //初めて
                var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAddAutoAdmissionListViewData>>("[]");
                accountJSONArray.Add(new AutoAddAutoAdmissionListViewData { ID = communityID });
                //JSON配列に変換
                Properties.Settings.Default.autoadd_community = JsonConvert.SerializeObject(accountJSONArray);
            }
            Properties.Settings.Default.Save();
            loadAutoAddAutoAdmissionList();
        }

        public void deleteAutoAddAutoAdmissionList(int pos)
        {
            //削除
            var account_list = Properties.Settings.Default.autoadd_community;
            var accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>(account_list);

            //削除ダイアログ
            var result = System.Windows.MessageBox.Show($"削除しますか？。\n{accountJSONArray[pos].Name}", "削除", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            //押したとき
            if (result == MessageBoxResult.OK)
            {
                //削除決行
                accountJSONArray.RemoveAt(pos);
                //accountJSONArray.RemoveAt(delete_pos);
                //保存
                Properties.Settings.Default.autoadd_community = JsonConvert.SerializeObject(accountJSONArray);
                //ListView更新
                Properties.Settings.Default.Save();
                loadAutoAddAutoAdmissionList();
            }
        }

        public void timerFollowAutoAddAdmission()
        {
            //間隔
            var space = "10";
            if (Properties.Settings.Default.autoadd_time != "")
            {
                space = Properties.Settings.Default.autoadd_time;
            }
            //フォロー中を定期的にスクレイピング
            followTimer = new DispatcherTimer();
            followTimer.Interval = TimeSpan.FromMinutes(int.Parse(space)); //間隔
            followTimer.Tick += FollowTimer;
            followTimer.Start();
        }


        public void timerNicorepoAutoAddAdmission()
        {
            //間隔
            var space = "10";
            if (Properties.Settings.Default.autoadd_time != "")
            {
                space = Properties.Settings.Default.autoadd_time;
            }
            //ニコレポのAPIを定期的に叩く
            nicorepoTimer = new DispatcherTimer();
            nicorepoTimer.Interval = TimeSpan.FromMinutes(int.Parse(space)); //間隔
            nicorepoTimer.Tick += NicorepoTimerAsync;
            nicorepoTimer.Start();
        }

        private async void NicorepoTimerAsync(object sender, object e)
        {
            //ニコレポ巡回
            //今のUnixTime
            long nowUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Properties.Settings.Default.user_session != "")
            {
                //APIリンク
                var urlString = "https://www.nicovideo.jp/api/nicorepo/timeline/my/all?client_app=pc_myrepo";

                //クッキーの設定（user_session）
                var user_session = Properties.Settings.Default.user_session;
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
                using (var stream = await client.GetAsync(new Uri(urlString)))
                {
                    if (stream.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var jsonString = await stream.Content.ReadAsStringAsync();
                        var jsonObject = JsonConvert.DeserializeObject<NicoRepoRootObject>(jsonString);

                        //forEach
                        var pos = 0;
                        foreach (var json in jsonObject.data)
                        {
                            //ニコレポには生放送以外の内容も流れてくるので生放送だけふるう
                            if (json.program != null)
                            {
                                //予約枠の投稿だけひろう
                                if (json.topic == "live.user.program.reserve")
                                {
                                    //番組開始時刻
                                    var date = DateTime.Parse(json.program.beginAt);
                                    //DateTime→UnixTime
                                    var unix = new DateTimeOffset(date.Ticks, new TimeSpan(+09, 00, 00));
                                    //すでに終わってる予約枠は拾わない
                                    if (nowUnixTime <= unix.ToUnixTimeSeconds())
                                    {
                                        //予約枠自動入場
                                        //追加                           
                                        addAdmissionList(json.program.title, json.program.id, unix.ToUnixTimeSeconds(), json.community.id);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //user_session再取得
                        var nicologin = new NicoLogin();
                        nicologin.ReNiconicoLogin();
                    }
                }
            }
        }

        private async void FollowTimer(object sender, object e)
        {
            //フォロー中巡回
            if (Properties.Settings.Default.user_session != "")
            {
                // タイトルを取得したいサイトのURL
                var urlstring = "https://sp.live.nicovideo.jp/favorites";
                //クッキーの設定（user_session）
                var user_session = Properties.Settings.Default.user_session;
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
                using (var stream = await client.GetAsync(new Uri(urlstring)))
                {
                    //var html = await client.GetStringAsync(urlstring);

                    if (stream.StatusCode == HttpStatusCode.OK)
                    {

                        var parser = new HtmlParser();
                        var jsonString = await stream.Content.ReadAsStringAsync();
                        var doc = await parser.ParseDocumentAsync(jsonString);


                        //フォロー中の番組のJSON
                        var json = doc.Head.GetElementsByTagName("script")[3].TextContent;
                        json = HttpUtility.UrlDecode(json);

                        json = json.Replace("window.__initial_state__ = \"", "");
                        json = json.Replace("locationBeforeTransitions\":null}}\";", "locationBeforeTransitions\":null}}");
                        json = json.Replace("window.__public_path__ = \"https://nicolive.cdn.nimg.jp/relive/sp/\";", "");


                        var nicoJSON = JsonConvert.DeserializeObject<RootObject>(json);

                        //forEachで取り出す
                        if (nicoJSON != null)
                        {
                            var pos = 0;
                            foreach (var program in nicoJSON.pageContents.favorites.favoritePrograms.programs)
                            {
                                //予約枠だけ取得
                                if (program.liveCycle == "BeforeOpen")
                                {
                                    //なんかしらんけどbeginAtがフォロー中番組だけ値が大きすぎるのでUnixTimeにする割り算
                                    var beginTime = program.beginAt / 1000L;
                                    //予約枠自動入場
                                    //フォロー中の場合はコミュニティIDが取れない問題。
                                    //めんどくさいけどコミュのサムネURLの一部にコミュのID入ってるので正規表現で取り出す。
                                    var communityId = regexCommunityID(program.socialGroupThumbnailUrl);
                                    addAdmissionList(program.title, program.id, beginTime, communityId);
                                }
                            }
                        }
                    }
                }
            }
        }

        //正規表現でコミュのIDを取り出します。
        public string regexCommunityID(string text)
        {
            Match matche = Regex.Match(text, "(co|ch)([0-9]+)");
            if (Regex.IsMatch(text, "(co|ch)([0-9]+)"))
            {
                //一致した。
                return matche.Value;
            }
            else
            {
                return "";
            }
        }

        public void addAdmissionList(string name, string id, long unix, string communityId)
        {
            //予約枠自動登録するコミュニティリスト
            //設定読み込み
            var communityList = new List<string>();
            var communityString = Properties.Settings.Default.autoadd_community;
            var communityJsonArray = JsonConvert.DeserializeObject<List<AutoAddAutoAdmissionListViewData>>(communityString);
            foreach (var item in communityJsonArray)
            {
                communityList.Add(item.ID);
            }

            //今の予約枠自動登録リスト
            //設定読み込み
            var autoAddList = new List<string>();
            var addAdmissionString = Properties.Settings.Default.auto_admission_list;
            var addAdmissionJsonArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(addAdmissionString);
            foreach (var item in addAdmissionJsonArray)
            {
                autoAddList.Add(item.ID);
            }


            //追加済みコミュニティだった！
            if (communityList.Contains(communityId))
            {
                //すでに追加済みの可能性
                if (!autoAddList.Contains(id))
                {
                    if (Properties.Settings.Default.auto_admission_list != "")
                    {
                        //追加
                        var account_list = Properties.Settings.Default.auto_admission_list;
                        var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);
                        accountJSONArray.Add(new AutoAdmissionJSON { Name = name, ID = id, UnixTime = unix });
                        //JSON配列に変換
                        Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                    }
                    else
                    {
                        //初めて
                        var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>("[]");
                        accountJSONArray.Add(new AutoAdmissionJSON { Name = name, ID = id, UnixTime = unix });
                        //JSON配列に変換
                        Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                    }
                    Properties.Settings.Default.Save();
                }
            }


        }
    }
}
