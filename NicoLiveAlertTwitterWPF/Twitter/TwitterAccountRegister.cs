using Newtonsoft.Json;
using NicoLiveAlertTwitterWPF.JSONClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NicoLiveAlertTwitterWPF.Twitter
{
    class TwitterAccountRegister
    {
        //Twitter API
        CoreTweet.Tokens twitter;
        //ListView
        public ObservableCollection<AccountListViewData> list = new ObservableCollection<AccountListViewData>();

        //アカウント、IDの配列作っとく？
        List<string> nameList = new List<string>();
        List<string> idList = new List<string>();

        public void initTwitter(Boolean showDialog)
        {
            //初期化。引数はログイン無いときダイアログだすか
            //ログイン情報取得
            if (Properties.Settings.Default.consumer_key != "")
            {
                var consumer_key = Properties.Settings.Default.consumer_key;
                var consumer_secret = Properties.Settings.Default.consumer_secret;
                var access_token = Properties.Settings.Default.access_token;
                var access_token_secret = Properties.Settings.Default.access_token_secret;
                twitter = CoreTweet.Tokens.Create(consumer_key, consumer_secret, access_token, access_token_secret);
            }
            else
            {
                if (showDialog)
                {
                    //ログインしてね
                    showLoginMessage();
                }
            }
            //ListView読み込み
            setListViewData();
        }


        public void showLoginMessage()
        {
            //ダイアログ
            System.Windows.Forms.MessageBox.Show("ログインして下さい。");
        }



        public void addAccount(string name)
        {
            initTwitter(true);
            //アカウント登録
            //アカウント検索
            var user = twitter.Users.Show(screen_name: name);

            //追加
            if (Properties.Settings.Default.account_list != "")
            {
                //追加
                var account_list = Properties.Settings.Default.account_list;
                var accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>(account_list);
                accountJSONArray.Add(new NicoFavListJSON { Name = user.Name, ID = user.Id.ToString() });
                //JSON配列に変換
                Properties.Settings.Default.account_list = JsonConvert.SerializeObject(accountJSONArray);
            }
            else
            {
                //初めて
                var accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>("[]");
                accountJSONArray.Add(new NicoFavListJSON { Name = user.Name, ID = user.Id.ToString() });
                //JSON配列に変換
                Properties.Settings.Default.account_list = JsonConvert.SerializeObject(accountJSONArray);
            }

            Properties.Settings.Default.Save();
            //ListView更新
            setListViewData();
        }

        public void setListViewData()
        {
            //ListViewのデータを入れる。

            //空にする
            list.Clear();
            //読み込む
            if (Properties.Settings.Default.account_list != "")
            {
                //存在チェック通過
                var account_list = Properties.Settings.Default.account_list;
                var accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>(account_list);
                foreach (NicoFavListJSON account in accountJSONArray)
                {
                    //配列に入れる
                    list.Add(new AccountListViewData { Name = account.Name, ID = account.ID, Pos = accountJSONArray.IndexOf(account) });
                    nameList.Add(account.Name);
                    idList.Add(account.ID);
                }
            }
        }

        public void deleteAccount(int pos)
        {
            //アカウント削除。引数は配列の位置です。
            //ダイアログ出す

            var result = System.Windows.Forms.MessageBox.Show("削除しますか？", "削除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            //押したとき
            if (result == DialogResult.Yes)
            {
                //削除決行
                var account_list = Properties.Settings.Default.account_list;
                var accountJSONArray = JsonConvert.DeserializeObject<List<NicoFavListJSON>>(account_list);
                accountJSONArray.RemoveAt(pos);
                //accountJSONArray.RemoveAt(delete_pos);
                //保存
                Properties.Settings.Default.account_list = JsonConvert.SerializeObject(accountJSONArray);
                Properties.Settings.Default.Save();
                //ListView更新
                setListViewData();
            }

        }
    }
}
