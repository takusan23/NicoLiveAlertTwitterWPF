using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoLiveAlertTwitterWPF.OtherLive
{
    class OtherLiveList
    {
        //URL一覧ListViewにいれるやつ
        public ObservableCollection<ListViewClass.OtherLiveSiteListViewData> urlListViewData = new ObservableCollection<ListViewClass.OtherLiveSiteListViewData>();
        //Client一覧ListViewにいれるやつ
        public ObservableCollection<ListViewClass.OtherLiveSiteListViewData> clientListViewData = new ObservableCollection<ListViewClass.OtherLiveSiteListViewData>();

        //URL・Clientだけの配列
        //FilterStreamで使う。
        public ObservableCollection<string> urlList = new ObservableCollection<string>();
        public ObservableCollection<string> clientList = new ObservableCollection<string>();

        public void loadOtherLiveURL()
        {
            urlListViewData.Clear();
            urlList.Clear();
            //他の配信サイトでも自動入場する機能（URL版）
            if (Properties.Settings.Default.setting_otherlive_url != "")
            {
                var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>(Properties.Settings.Default.setting_otherlive_url);
                var pos = 0;
                foreach (var item in otherListJSONArray)
                {
                    urlList.Add(item.URL);
                    urlListViewData.Add(new ListViewClass.OtherLiveSiteListViewData { URL = item.URL, Pos = pos });
                    pos += 1;
                }
            }
            else
            {
                //初期化
                initOtherLiveURL();
            }
        }

        public void loadOtherLiveClient()
        {
            clientListViewData.Clear();
            clientList.Clear();
            //他の配信サイトでも自動入場する機能（クライアント名版）
            if (Properties.Settings.Default.setting_otherlive_client != "")
            {
                var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>(Properties.Settings.Default.setting_otherlive_client);
                var pos = 0;
                foreach (var item in otherListJSONArray)
                {
                    clientList.Add(item.Client);
                    clientListViewData.Add(new ListViewClass.OtherLiveSiteListViewData { Client = item.Client, Pos = pos });
                    pos += 1;
                }
            }
            else
            {
                //初期化
                initOtherLiveClient();
            }
        }

        public void initOtherLiveURL()
        {
            //他の配信サイトでも自動入場する機能（URL）の中身初期化するとき
            //はじめはようつべとツイキャスとふわっちで使えるように
            //なお作者は老害なので他サイトで見に行くことはほぼない模様（コメントがあれ）

            //ようつべはアップロードと同時にツイートする機能がなくなった模様。
            //ようつべこれ動画を共有しても自動で開いちゃうんだけどURLからはどうしようもないよね？

            var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>("[]");

            //追加
            var youtube = new ListViewClass.OtherLiveSiteListViewData { URL = "youtu.be" };
            var twitcas = new ListViewClass.OtherLiveSiteListViewData { URL = "cas.st" };
            var huwatti = new ListViewClass.OtherLiveSiteListViewData { URL = "r.whowatch.tv" };
            otherListJSONArray.Add(youtube);
            otherListJSONArray.Add(twitcas);
            otherListJSONArray.Add(huwatti);

            //JSON配列に変換
            Properties.Settings.Default.setting_otherlive_url = JsonConvert.SerializeObject(otherListJSONArray);
            Properties.Settings.Default.Save();
        }

        public void initOtherLiveClient()
        {
            //他の配信サイトでも自動入場する機能（クライアント名）の中身初期化するとき
            //はじめはようつべとツイキャスとふわっちで使えるように
            //なお作者は老害なので他サイトで見に行くことはほぼない模様（コメントがあれ）

            //ようつべはアップロードと同時にツイートする機能がなくなったみたいなのでクライアント一覧にはないです。

            var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>("[]");

            //追加
            var twitcas = new ListViewClass.OtherLiveSiteListViewData { Client = "TwitCasting" };
            var huwatti = new ListViewClass.OtherLiveSiteListViewData { Client = "ふわっち" };
            otherListJSONArray.Add(twitcas);
            otherListJSONArray.Add(huwatti);

            //JSON配列に変換
            Properties.Settings.Default.setting_otherlive_client = JsonConvert.SerializeObject(otherListJSONArray);
            Properties.Settings.Default.Save();        
        }

        public void addOtherLiveURL(string url)
        {
            //他の配信サイトでも利用する機能のURL追加
            if (Properties.Settings.Default.setting_otherlive_url != "")
            {
                var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>(Properties.Settings.Default.setting_otherlive_url);
                var urlItem = new ListViewClass.OtherLiveSiteListViewData { URL = url };
                otherListJSONArray.Add(urlItem);
                Properties.Settings.Default.setting_otherlive_url = JsonConvert.SerializeObject(otherListJSONArray);
                Properties.Settings.Default.Save();
                //再読み込み
                loadOtherLiveURL();
            }
            else
            {
                //初期化
                loadOtherLiveURL();
            }
        }

        public void addOtherLiveClient(string client)
        {
            //他の配信サイトでも利用する機能のクライアント追加
            if (Properties.Settings.Default.setting_otherlive_client != "")
            {
                var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>(Properties.Settings.Default.setting_otherlive_client);
                var urlItem = new ListViewClass.OtherLiveSiteListViewData { Client = client };
                otherListJSONArray.Add(urlItem);
                Properties.Settings.Default.setting_otherlive_client = JsonConvert.SerializeObject(otherListJSONArray);
                Properties.Settings.Default.Save();
                //再読み込み
                loadOtherLiveClient();
            }
            else
            {
                //初期化
                loadOtherLiveURL();
            }
        }

        public void deleteOtherLiveURL(int pos)
        {
            //他の配信サイトでも利用する機能のURL削除
            var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>(Properties.Settings.Default.setting_otherlive_url);
            otherListJSONArray.RemoveAt(pos);
            Properties.Settings.Default.setting_otherlive_url = JsonConvert.SerializeObject(otherListJSONArray);
            Properties.Settings.Default.Save();
            //再読み込み
            loadOtherLiveURL();
        }

        public void deleteOtherLiveClient(int pos)
        {
            //他の配信サイトでも利用する機能のクライアント削除
            var otherListJSONArray = JsonConvert.DeserializeObject<List<ListViewClass.OtherLiveSiteListViewData>>(Properties.Settings.Default.setting_otherlive_client);
            otherListJSONArray.RemoveAt(pos);
            Properties.Settings.Default.setting_otherlive_client = JsonConvert.SerializeObject(otherListJSONArray);
            Properties.Settings.Default.Save();
            //再読み込み
            loadOtherLiveClient();
        }
    }
}
