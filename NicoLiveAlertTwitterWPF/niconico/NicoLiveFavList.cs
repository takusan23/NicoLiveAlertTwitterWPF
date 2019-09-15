using AngleSharp.Html.Parser;
using Newtonsoft.Json;
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
using System.Web;
using System.Windows;
using System.Windows.Forms;

namespace NicoLiveAlertTwitterWPF.niconico
{
    class NicoLiveFavList
    {
        //ListViewにいれるやつ
        public ObservableCollection<ProgramListViewData> list = new ObservableCollection<ProgramListViewData>();

        public async void loadNicoFavList(Boolean dialogShow)
        {
            //ニコ生フォロー中表示。引数はログイン無い時にダイアログだすかどうか
            list.Clear();
            if (Properties.Settings.Default.user_session != "")
            {
                // タイトルを取得したいサイトのURL
                var urlstring = "https://sp.live.nicovideo.jp/favorites";

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
                                    var dateTime = "開場時間 : " + DateTimeOffset.FromUnixTimeSeconds(beginTime).LocalDateTime.ToString();
                                    var item = new ProgramListViewData { Name = program.title + " | " + program.socialGroupName + " | " + program.id, Pos = pos, beginAt = beginTime, ID = program.id, dateTime = dateTime };
                                    pos += 1;
                                    list.Add(item);
                                }
                            }
                        }
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
            System.Windows.MessageBox.Show("ニコニコにログインして下さい。", "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        public void addAdmissionProgram(int pos)
        {
            //登録ボタン押した。ダイアログだす
            var item = list[pos];
            var dateTime = FromUnixTime(item.beginAt);

            //確認ダイアログ
            var result = System.Windows.MessageBox.Show($"この番組は開場時間になったら自動で入場します。\n{item.Name} 開場時間 : {dateTime.ToString()}", "登録", MessageBoxButton.OKCancel, MessageBoxImage.Information);

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


            //かぶってないか確認
            if (!admissionList.Contains(item.ID))
            {
                //かぶってなければ追加
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
