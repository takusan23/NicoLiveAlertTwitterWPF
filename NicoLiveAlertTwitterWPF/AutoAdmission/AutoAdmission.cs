using Newtonsoft.Json;
using NicoLiveAlertTwitterWPF.JSONClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NicoLiveAlertTwitterWPF.AutoAdmission
{
    class AutoAdmission
    {

        //定期実行
        public DispatcherTimer Timer;

        //リスト
        List<String> liveIdList = new List<String>();
        List<String> nameList = new List<String>();
        List<long> unixTimeList = new List<long>();
        //1分前通知
        List<long> unixTimeOneMinutesList = new List<long>();

        //予約枠自動入場JSON
        List<AutoAdmissionJSON> autoAdmissionJSON;

        //履歴機能
        ProgramHistory.ProgramHistory programHistory = new ProgramHistory.ProgramHistory();



        MainWindow window;

        public void startAutoAdmission(MainWindow window)
        {

            this.window = window;

            //クリア
            liveIdList.Clear();
            nameList.Clear();
            unixTimeList.Clear();
            unixTimeOneMinutesList.Clear();

            //読み込み
            if (Properties.Settings.Default.auto_admission_list != "")
            {
                var account_list = Properties.Settings.Default.auto_admission_list;
                autoAdmissionJSON = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);

                //ListView
                foreach (var autoAdmission in autoAdmissionJSON)
                {
                    //配列に入れる
                    nameList.Add(autoAdmission.Name);
                    liveIdList.Add(autoAdmission.ID);
                    unixTimeList.Add(autoAdmission.UnixTime);
                }
            }

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(1000); //間隔
            Timer.Tick += TickTimer;
            Timer.Start();
        }

        private void TickTimer(object sender, object e)
        {
            //ここで定期実行される
            //クリア
            liveIdList.Clear();
            nameList.Clear();
            unixTimeList.Clear();
            unixTimeOneMinutesList.Clear();

            if (Properties.Settings.Default.auto_admission_list != "")
            {
                //読み込み
                var account_list = Properties.Settings.Default.auto_admission_list;
                autoAdmissionJSON = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);

                //ListView
                foreach (var autoAdmission in autoAdmissionJSON)
                {
                    //配列に入れる
                    nameList.Add(autoAdmission.Name);
                    liveIdList.Add(autoAdmission.ID);
                    unixTimeList.Add(autoAdmission.UnixTime);
                    //１分前通知
                    unixTimeOneMinutesList.Add(autoAdmission.UnixTime - 60);
                }

                //今のUnixTime取得
                long nowUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (unixTimeList.Contains(nowUnixTime))
                {

                    //始まった！
                    var index = unixTimeList.IndexOf(nowUnixTime);
                    var name = nameList[index];
                    var liveId = liveIdList[index];

                    //ブラウザ起動
                    lunchBrowser(liveId);

                    //通知出す
                    showNotification("番組が開始しました。入場します！", name);

                    //履歴追加
                    programHistory.addHistory(liveId);

                    //Console.WriteLine(index);

                    //Console.WriteLine(JsonConvert.SerializeObject(autoAdmissionJSON));

                    //開場したので配列から
                    autoAdmissionJSON.RemoveAt(index);
                    //保存
                    Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(autoAdmissionJSON);
                    Properties.Settings.Default.Save();
                }

                //１分前に通知する？
                //設定で選べるように
                if (Properties.Settings.Default.setting_autoadmission_one_minute_notify != "")
                {
                    if (Boolean.Parse(Properties.Settings.Default.setting_autoadmission_one_minute_notify))
                    {
                        if (unixTimeOneMinutesList.Contains(nowUnixTime))
                        {
                            var index = unixTimeOneMinutesList.IndexOf(nowUnixTime);
                            var name = nameList[index];
                            //通知出す
                            showNotification("１分後に番組に自動入場します。", name);
                        }
                    }
                }
            }
        }


        //通知
        private void showNotification(string title, string value)
        {
            window.NotifyIcon.ShowBalloonTip(title, value, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None);
        }

        //ブラウザ起動
        private void lunchBrowser(string programId)
        {
            System.Diagnostics.Process.Start("https://live2.nicovideo.jp/watch/" + programId);
        }
    }
}
