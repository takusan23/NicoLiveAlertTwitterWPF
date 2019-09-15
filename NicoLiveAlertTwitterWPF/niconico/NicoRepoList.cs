using Newtonsoft.Json;
using NicoLiveAlertTwitterCS.JSONClass;
using NicoLiveAlertTwitterWPF.JSONClass;
using NicoLiveAlertTwitterWPF.ListViewClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoRepoList
    {
        //ListViewのでーた
        public ObservableCollection<ProgramListViewData> list = new ObservableCollection<ProgramListViewData>();

        public async void loadNicoRepo(Boolean dialogShow)
        {
            list.Clear();
            //今のUnixTime
            long nowUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Properties.Settings.Default.user_session != "")
            {
                //ニコレポAPI？
                var urlString = "https://www.nicovideo.jp/api/nicorepo/timeline/my/all?client_app=pc_myrepo";
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
                client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", "user_session=" + Properties.Settings.Default.user_session);
                using (var stream = await client.GetAsync(new Uri(urlString)))
                {
                    if (stream.StatusCode == HttpStatusCode.OK)
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
                                    //開場時間
                                    var dateTime = "開場時間 : " + date.ToString();
                                    //すでに終わってる予約枠は拾わない
                                    if (nowUnixTime <= unix.ToUnixTimeSeconds())
                                    {
                                        var name = $"{json.program.title} | {json.community.name} | {json.program.id}";
                                        var item = new ProgramListViewData { Name = name, beginAt = unix.ToUnixTimeSeconds(), ID = json.program.id, Pos = pos, dateTime = dateTime };
                                        list.Add(item);
                                        pos += 1;
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
            else
            {
                if (dialogShow)
                {
                    showLoginMessage();
                }
            }

        }


        private void showLoginMessage()
        {
            //ダイアログ出す
            System.Windows.MessageBox.Show("ニコニコにログインして下さい。", "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAPIErrorDialog()
        {
            //ダイアログ出す
            System.Windows.MessageBox.Show("ニコニコへ再ログインします。", "user_session切れ？", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void addAdmissionProgram(int pos)
        {
            //登録ボタン押した

            var item = list[pos];

            //登録ボタン押した。ダイアログだす
            var dateTime = FromUnixTime(item.beginAt);

            //今の予約枠自動登録リスト
            //設定読み込み
            var admissionList = new List<string>();
            var addAdmissionString = Properties.Settings.Default.auto_admission_list;
            var addAdmissionJsonArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(addAdmissionString);
            if (addAdmissionJsonArray != null)
            {
                foreach (var admission in addAdmissionJsonArray)
                {
                    admissionList.Add(admission.ID);
                }
            }

            //確認ダイアログ
            var result = System.Windows.MessageBox.Show($"この番組は開場時間になったら自動で入場します。\n{item.Name} 開場時間 : {dateTime.ToString()}", "登録", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (!admissionList.Contains(item.ID))
            {
                //かぶってない
                //押したとき
                if (result == MessageBoxResult.OK)
                {
                    //追加
                    if (Properties.Settings.Default.auto_admission_list != "")
                    {
                        //追加
                        var account_list = Properties.Settings.Default.auto_admission_list;
                        var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);
                        accountJSONArray.Add(new AutoAdmissionJSON { Name = item.Name, ID = item.ID, UnixTime = item.beginAt });
                        //JSON配列に変換
                        Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                    }
                    else
                    {
                        //初めて
                        var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>("[]");
                        accountJSONArray.Add(new AutoAdmissionJSON { Name = item.Name, ID = item.ID, UnixTime = item.beginAt });
                        //JSON配列に変換
                        Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                    }
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                //被りダイアログ
                System.Windows.MessageBox.Show($"追加済みです。\n{item.Name} 開場時間：{dateTime.ToString()}", "追加済みです。", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        public DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
    }
}
