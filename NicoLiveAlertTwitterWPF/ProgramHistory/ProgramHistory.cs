using CoreTweet;
using Newtonsoft.Json;
using NicoLiveAlertTwitterWPF.ListViewClass;
using NicoLiveAlertTwitterWPF.niconico;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicoLiveAlertTwitterWPF.ProgramHistory
{
    class ProgramHistory
    {
        //ListViewにいれるやつ
        public ObservableCollection<ProgramHistoryListViewData> list = new ObservableCollection<ProgramHistoryListViewData>();

        public void loadHisotry()
        {
            list.Clear();
            if (Properties.Settings.Default.program_history != "")
            {
                //追加
                var programList = Properties.Settings.Default.program_history;
                var programJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>(programList);
                foreach (var json in programJSONArray)
                {
                    var dateTime = DateTimeOffset.FromUnixTimeSeconds(json.BeginAt).LocalDateTime;
                    list.Add(new ProgramHistoryListViewData { Content = json.Content, dateTime = dateTime.ToString(), LiveId = json.LiveId });
                }
            }
        }

        public async void addHistory(string liveId)
        {
            //番組情報取得API
            var nicoLiveProgramInfo = new NicoLiveProgramInfo();
            var result = await nicoLiveProgramInfo.getProgramInfo(liveId);
            //履歴機能
            if (Properties.Settings.Default.program_history != "")
            {
                //追加
                var historyList = Properties.Settings.Default.program_history;
                var historyJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>(historyList);
                historyJSONArray.Insert(0, new ProgramHistoryListViewData { Content = result.data.title, LiveId = liveId, BeginAt = result.data.beginAt });
                //JSON配列に変換
                Properties.Settings.Default.program_history = JsonConvert.SerializeObject(historyJSONArray);
            }
            else
            {
                //初めての履歴///
                var historyJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>("[]");
                historyJSONArray.Insert(0, new ProgramHistoryListViewData { Content = result.data.title, LiveId = liveId, BeginAt = result.data.beginAt });
                //JSON配列に変換
                Properties.Settings.Default.program_history = JsonConvert.SerializeObject(historyJSONArray);
            }
            Properties.Settings.Default.Save();
            loadHisotry();
        }

        public void addOtherLiveHistory(Status tw, string url)
        {
            //履歴機能
            var unix = new DateTimeOffset(tw.CreatedAt.AddHours(9).Ticks, new TimeSpan(+09, 00, 00));
            if (Properties.Settings.Default.program_history != "")
            {
                //追加
                var historyList = Properties.Settings.Default.program_history;
                var historyJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>(historyList);
                historyJSONArray.Insert(0, new ProgramHistoryListViewData { Content = tw.Text, LiveId = url, BeginAt = unix.ToUnixTimeSeconds() });
                //JSON配列に変換
                Properties.Settings.Default.program_history = JsonConvert.SerializeObject(historyJSONArray);
            }
            else
            {
                //初めての履歴///
                var historyJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>("[]");
                historyJSONArray.Insert(0, new ProgramHistoryListViewData { Content = tw.Text, LiveId = url, BeginAt = unix.ToUnixTimeSeconds() });
                //JSON配列に変換
                Properties.Settings.Default.program_history = JsonConvert.SerializeObject(historyJSONArray);
            }
            Properties.Settings.Default.Save();
            loadHisotry();
        }

        public void clearHistory()
        {
            //確認ダイアログ
            var result = System.Windows.MessageBox.Show("履歴を全削除しますか？", "履歴", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
            {
                if (Properties.Settings.Default.program_history != "")
                {
                    Properties.Settings.Default.program_history = "";
                    Properties.Settings.Default.Save();
                }
                loadHisotry();
            }
        }
    }
}
