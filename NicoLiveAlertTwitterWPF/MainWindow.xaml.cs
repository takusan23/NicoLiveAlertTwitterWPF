using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using NicoLiveAlertTwitterWPF.AutoAdmission;
using NicoLiveAlertTwitterWPF.Info;
using NicoLiveAlertTwitterWPF.niconico;
using NicoLiveAlertTwitterWPF.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NicoLiveAlertTwitterWPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        //現在のバージョン？
        public static string AppVersion = "1.0.0 2019/09/17";

        //ページの配列
        List<Grid> pageList = new List<Grid>();

        //Twitterログイン
        TwitterLogin twitterLogin = new TwitterLogin();
        //生主アカウント
        TwitterAccountRegister twitterAccountRegister = new TwitterAccountRegister();
        //FilterStream
        FilterStream filterStream = new FilterStream();

        //ニコニコログイン
        NicoLogin nicoLogin = new NicoLogin();
        //ニコ生フォロー中番組
        NicoLiveFavList nicoLiveFavList = new NicoLiveFavList();
        //ニコレポ
        NicoRepoList nicoRepoList = new NicoRepoList();

        //予約枠自動入場リスト
        AutoAdmissionList autoAdmissionList = new AutoAdmissionList();
        //自動入場タイマー
        AutoAdmission.AutoAdmission autoAdmission = new AutoAdmission.AutoAdmission();
        //予約枠自動登録
        AutoAddAdmission autoAddAdmission = new AutoAddAdmission();

        //履歴
        ProgramHistory.ProgramHistory programHistory = new ProgramHistory.ProgramHistory();

        //他の配信サイトでも自動入場
        OtherLive.OtherLiveList otherLive = new OtherLive.OtherLiveList();

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine(Properties.Settings.Default.setting_filterstream_addadmission == "" || Boolean.Parse(Properties.Settings.Default.setting_filterstream_addadmission));

            pageList.Add(HomePanel);
            pageList.Add(LiveStreamerAccountPanel);
            pageList.Add(AutoAdmissionPanel);
            pageList.Add(FavouriteProgramPanel);
            pageList.Add(NicoRepoPanel);
            pageList.Add(LoginPanel);
            pageList.Add(AutoAddAdmissionPanel);
            pageList.Add(HistoryPanel);
            pageList.Add(SettingPanel);
            pageList.Add(AppInfoPanel);

            //Twitterアカウント設定
            twitterAccountRegister.initTwitter(false);
            LiveStreamerAccountListView.ItemsSource = twitterAccountRegister.list;

            //ニコ生フォロー中
            nicoLiveFavList.loadNicoFavList(false);
            NicoFavListView.ItemsSource = nicoLiveFavList.list;

            //ニコレポ
            nicoRepoList.loadNicoRepo(false);
            NicoRepoListView.ItemsSource = nicoRepoList.list;

            //予約枠自動入場
            autoAdmissionList.loadList(false);
            AutoAdmissionListView.ItemsSource = autoAdmissionList.list;

            //予約枠自動登録
            autoAddAdmission.loadAutoAddAutoAdmissionList();
            AutoAddAdmissionListView.ItemsSource = autoAddAdmission.list;

            //予約枠自動入場が始まるか監視
            autoAdmission.startAutoAdmission(this);

            //履歴機能
            //Console.WriteLine(Properties.Settings.Default.program_history);
            programHistory.loadHisotry();
            HistoryListView.ItemsSource = programHistory.list;

            //他の配信サイトでも自動入場
            otherLive.loadOtherLiveURL();
            otherLive.loadOtherLiveClient();
            SettingOtherLiveURLListView.ItemsSource = otherLive.urlListViewData;
            SettingOtherLiveClientListView.ItemsSource = otherLive.clientListViewData;

            //設定読み込み
            loadSetting();

            //起動時はアラート履歴を開く
            pageList[7].Visibility = Visibility.Visible;
            SideMenuListView.SelectedItem = NavItemHistory;
        }


        private void TwitterLoginConsumerKeyButton_Click(object sender, RoutedEventArgs e)
        {
            //Twitterの開発者登録が済んでる人向け設定
            var ConsumerKeyDialog = new TwitterConsumerKeyDialog();
            //Twitterログイン
            if (ConsumerKeyDialog.ShowDialog() == true)
            {
                //認証画面出す
                twitterLogin.consumer_key = ConsumerKeyDialog.ConsumerKey;
                twitterLogin.consumer_secret = ConsumerKeyDialog.ConsumerSecret;
                twitterLogin.showTwitterParge();
                var PinDialog = new PINDialog();
                if (PinDialog.ShowDialog() == true)
                {
                    //アクセストークン取得
                    twitterLogin.GetAccessToken(PinDialog.ResponseText);
                }
            }
        }

        private void NicoRepoAddAdmissionButton_Click(object sender, RoutedEventArgs e)
        {
            //ニコレポから予約枠自動入場追加した
            nicoRepoList.addAdmissionProgram(int.Parse((sender as Button).Tag.ToString()), autoAdmissionList);
        }

        private void NicoFavAddAutoAdmissionButton_Click(object sender, RoutedEventArgs e)
        {
            //ニコ生フォロー中から予約枠自動入場追加した
            nicoLiveFavList.addAdmissionProgram(int.Parse((sender as Button).Tag.ToString()), autoAdmissionList);
        }

        private void AutoAdmissionDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //予約枠自動入場削除
            autoAdmissionList.deleteAutoAdmission(int.Parse((sender as Button).Tag.ToString()));
        }

        private void TwitterAccountDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //生主アカウント削除ボタン
            twitterAccountRegister.deleteAccount(int.Parse((sender as Button).Tag.ToString()));
            LiveStreamerAccountListView.ItemsSource = twitterAccountRegister.list;
        }

        private void TwitterLoginButton_Click(object sender, RoutedEventArgs e)
        {
            //Twitterログイン
            twitterLogin.showTwitterParge();
            var PinDialog = new PINDialog();
            if (PinDialog.ShowDialog() == true)
            {
                //アクセストークン取得
                twitterLogin.GetAccessToken(PinDialog.ResponseText);
            }
        }

        private void NicoLoginButton_Click(object sender, RoutedEventArgs e)
        {
            //ニコニコログインボタン
            var NicoLoginDialog = new NicoLoginMailPassDialog();
            if (NicoLoginDialog.ShowDialog() == true)
            {
                //メアド、パスワード取得してログイン
                nicoLogin.niconicoLogin(NicoLoginDialog.MailAdress, NicoLoginDialog.Password);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //左のサイドメニュー押したとき
            var tag = (sender as ListViewItem).Tag;

            //全て非表示
            foreach (var page in pageList)
            {
                page.Visibility = Visibility.Collapsed;
            }

            //切り替える
            switch (tag)
            {
                case "NavItemHome":
                    //ホーム
                    pageList[0].Visibility = Visibility.Visible;
                    break;
                case "NavItemLiveStreamerTwitter":
                    //生主アカウント
                    pageList[1].Visibility = Visibility.Visible;
                    break;
                case "NavItemAutoAdmission":
                    //予約枠自動入場
                    autoAdmissionList.loadList(true);
                    pageList[2].Visibility = Visibility.Visible;
                    break;
                case "NavItemFollowProgram":
                    //フォロー中番組
                    nicoLiveFavList.loadNicoFavList(true);
                    pageList[3].Visibility = Visibility.Visible;
                    break;
                case "NavItemNicorepo":
                    //ニコレポ
                    nicoRepoList.loadNicoRepo(true);
                    pageList[4].Visibility = Visibility.Visible;
                    break;
                case "NavItemLogin":
                    //ログイン
                    pageList[5].Visibility = Visibility.Visible;
                    break;
                case "NavItemAutoAddAdmission":
                    //予約枠自動登録
                    pageList[6].Visibility = Visibility.Visible;
                    break;
                case "NavItemHistory":
                    //アラート履歴
                    pageList[7].Visibility = Visibility.Visible;
                    programHistory.loadHisotry();
                    break;
                case "NavItemSetting":
                    //設定
                    pageList[8].Visibility = Visibility.Visible;
                    break;
                case "NavItemAppInfo":
                    //このアプリについて
                    pageList[9].Visibility = Visibility.Visible;
                    break;
            }
        }

        private void LiveStreamerAddButton_Click(object sender, RoutedEventArgs e)
        {
            //アカウント追加
            twitterAccountRegister.addAccount(LiveStreamerAddAccountTextBox.Text);
        }

        private void FilterStreamSwitch_Click(object sender, RoutedEventArgs e)
        {
            //FilterStream接続
            var mode = (sender as ToggleButton).IsChecked;
            if (mode == true)
            {
                //接続
                filterStream.connectFilterStream(this);
                FilterStreamTextBlock.Text = "リアルタイム更新を利用中　";
            }
            else
            {
                //切断
                filterStream.tokenSource.Cancel();
                FilterStreamTextBlock.Text = "リアルタイム更新を開始　";
            }
        }
        private void AutoAddAdmissionSwitch_Click(object sender, RoutedEventArgs e)
        {
            //予約枠自動登録スイッチ押した
            var mode = (sender as ToggleButton).IsChecked;
            if (mode == true)
            {
                //開始。
                autoAddAdmission.timerFollowAutoAddAdmission();
                autoAddAdmission.timerNicorepoAutoAddAdmission();
                autoAddAdmission.nicorepoTimer.Start();
                autoAddAdmission.followTimer.Start();
                AutoAddAdmissionTextBlock.Text = "予約枠自動登録利用中　";
            }
            else
            {
                //止める。
                autoAddAdmission.nicorepoTimer.Stop();
                autoAddAdmission.followTimer.Stop();
                AutoAddAdmissionTextBlock.Text = "予約枠自動登録開始　";
            }
        }

        private async void AutoAdmissionAddButton_Click(object sender, RoutedEventArgs e)
        {
            //予約枠自動入場追加ボタン
            var autoAdmissionDialog = new AutoAdmissionDialog();
            var nicoProgramInfo = new NicoLiveProgramInfo();
            if (autoAdmissionDialog.ShowDialog() == true)
            {
                var json = await nicoProgramInfo.getProgramInfo(autoAdmissionDialog.ProgramID);
                if (json.data.status == "reserved")
                {
                    //UnixTime
                    var unixTime = json.data.beginAt;
                    var title = json.data.title;
                    var id = autoAdmissionDialog.ProgramID;

                    //追加
                    autoAdmissionList.addAdmission(title, id, unixTime, true);
                }
                else
                {
                    MessageBox.Show("予約枠のみが追加可能です。", "予約枠追加", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AutoAddAdmissionTime_TextChanged(object sender, TextChangedEventArgs e)
        {
        }


        private void AutoAddAdmissionCommunityDelete_Click(object sender, RoutedEventArgs e)
        {
            //予約枠自動登録のコミュニティ削除ボタン押した
            //コミュニティ削除
            var pos = (sender as Button).Tag;
            autoAddAdmission.deleteAutoAddAutoAdmissionList(int.Parse(pos.ToString()));
        }

        private void AutoAddAdmissionButton_Click(object sender, RoutedEventArgs e)
        {
            //予約枠自動登録追加ボタン押した
            autoAddAdmission.addAutoAddAutoAdmissionList(AutoAddAdmissionAddCommunity.Text);
        }

        private void AutoAddAdmissionTimeButton_Click(object sender, RoutedEventArgs e)
        {
            //予約枠自動登録のAPI/スクレイピング間隔
            //TextBoxの内容が変わると呼ばれる
            Properties.Settings.Default.autoadd_time = AutoAddAdmissionTimeTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void SettingCheckBoxClick(object sender, RoutedEventArgs e)
        {
            //設定保存する
            var name = (sender as CheckBox).Name;
            var check = (sender as CheckBox).IsChecked.ToString();
            switch (name)
            {
                case "SettingLaunchStartFilterStreamCheck":
                    Properties.Settings.Default.setting_launch_start_filterstream = check;
                    break;
                case "SettingLaunchStartAutoAddAdmissionCheck":
                    Properties.Settings.Default.setting_launch_start_autoaddadmission = check;
                    break;
                case "SettingOtherLiveSwitch":
                    Properties.Settings.Default.setting_otherlive_mode = check;
                    break;
                case "SettingFilterStreamAddAdmissionCheck":
                    Properties.Settings.Default.setting_filterstream_addadmission = check;
                    break;
            }
            Properties.Settings.Default.Save();
        }

        private void loadSetting()
        {
            //設定読み込む
            if (Properties.Settings.Default.setting_launch_start_filterstream != "")
            {
                SettingLaunchStartFilterStreamCheck.IsChecked = Boolean.Parse(Properties.Settings.Default.setting_launch_start_filterstream);
                //起動時にFilterStreamへ接続する
                if (Boolean.Parse(Properties.Settings.Default.setting_launch_start_filterstream))
                {
                    //接続
                    filterStream.connectFilterStream(this);
                    FilterStreamTextBlock.Text = "リアルタイム更新を利用中　";
                    FilterStreamSwitch.IsChecked = true;
                }
            }
            if (Properties.Settings.Default.setting_launch_start_autoaddadmission != "")
            {
                SettingLaunchStartAutoAddAdmissionCheck.IsChecked = Boolean.Parse(Properties.Settings.Default.setting_launch_start_autoaddadmission);
                //起動時に予約枠自動登録を利用する
                if (Boolean.Parse(Properties.Settings.Default.setting_launch_start_autoaddadmission))
                {
                    //開始。
                    autoAddAdmission.timerFollowAutoAddAdmission();
                    autoAddAdmission.timerNicorepoAutoAddAdmission();
                    autoAddAdmission.nicorepoTimer.Start();
                    autoAddAdmission.followTimer.Start();
                    AutoAddAdmissionTextBlock.Text = "予約枠自動登録利用中　";
                    AutoAddAdmissionSwitch.IsChecked = true;
                }
            }
            if (Properties.Settings.Default.setting_otherlive_mode != "")
            {
                SettingOtherLiveSwitch.IsChecked = Boolean.Parse(Properties.Settings.Default.setting_otherlive_mode);
            }
            if (Properties.Settings.Default.autoadd_time != "")
            {
                AutoAddAdmissionTimeTextBox.Text = Properties.Settings.Default.autoadd_time;
            }
            if (Properties.Settings.Default.setting_filterstream_addadmission != "")
            {
                //FilterStreamで予約枠のツイートが流れてきた場合は予約枠自動入場に登録する設定。
                SettingFilterStreamAddAdmissionCheck.IsChecked = Boolean.Parse(Properties.Settings.Default.setting_filterstream_addadmission);
            }
        }

        private void AppInfoButton_Click(object sender, RoutedEventArgs e)
        {
            // Twiiter/Mastodon/GitHubリンク

            var twitterLink = "https://twitter.com/takusan__23";
            var mastodonLink = "https://best-friends.chat/@takusan_23";
            var githubLink = "https://github.com/takusan23/NicoLiveAlertTwitterWPF";

            var name = (sender as Button).Name;
            switch (name)
            {
                case "AppInfoTwitterButton":
                    System.Diagnostics.Process.Start(twitterLink);
                    break;
                case "AppInfoMastodonButton":
                    System.Diagnostics.Process.Start(mastodonLink);
                    break;
                case "AppInfoGitHubButton":
                    System.Diagnostics.Process.Start(githubLink);
                    break;
            }
        }

        private void SettingOtherLiveURLDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //他の配信サイトでも自動入場する機能（URL）の削除
            var pos = (sender as Button).Tag.ToString();
            otherLive.deleteOtherLiveURL(int.Parse(pos));
        }

        private void SettingOtherLiveClientDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //他の配信サイトでも自動入場する機能（クライアント）の削除
            var pos = (sender as Button).Tag.ToString();
            otherLive.deleteOtherLiveClient(int.Parse(pos));
        }

        private void SettingOtherLiveURLAddButton_Click(object sender, RoutedEventArgs e)
        {
            //他の配信サイトでも自動入場する機能（URL）の追加
            otherLive.addOtherLiveURL(SettingOtherLiveURLTextBox.Text);
        }

        private void SettingOtherLiveClientAddButton_Click(object sender, RoutedEventArgs e)
        {
            //他の配信サイトでも自動入場する機能（クライアント）の追加
            otherLive.addOtherLiveClient(SettingOtherLiveClientTextBox.Text);
        }

        private void HistoryClearButton_Click(object sender, RoutedEventArgs e)
        {
            //履歴全削除
            programHistory.clearHistory();
        }

        private void AutoAddAdmissionAllButton_Click(object sender, RoutedEventArgs e)
        {
            //削除ダイアログ
            var result = System.Windows.MessageBox.Show("参加中のコミュニティを全て追加しますか？", "参加コミュ全追加", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                var community = new NicoCommnityList();
                community.getFollowCommunity("https://com.nicovideo.jp/community", autoAddAdmission);
            }
        }

        private void AppInfoUpdateCheckButton_Click(object sender, RoutedEventArgs e)
        {
            //更新の確認
            var update = new AppUpdateCheck();
            update.checkNewVersion();
        }
    }
}
