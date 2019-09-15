using Newtonsoft.Json;
using NicoLiveAlertTwitterWPF.ListViewClass;
using NicoLiveAlertTwitterWPF.niconico;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var programList = Properties.Settings.Default.program_history;
                var programJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>(programList);
                programJSONArray.Add(new ProgramHistoryListViewData { Content = result.data.title, LiveId = liveId, BeginAt = result.data.beginAt });
                //JSON配列に変換
                Properties.Settings.Default.autoadd_community = JsonConvert.SerializeObject(programJSONArray);
            }
            else
            {
                //初めての履歴///
                var programJSONArray = JsonConvert.DeserializeObject<List<ProgramHistoryListViewData>>("[]");
                programJSONArray.Add(new ProgramHistoryListViewData { Content = result.data.title, LiveId = liveId, BeginAt = result.data.beginAt });
                //JSON配列に変換
                Properties.Settings.Default.autoadd_community = JsonConvert.SerializeObject(programJSONArray);
            }
            Properties.Settings.Default.Save();
        }

        public void clearHistory()
        {
            if (Properties.Settings.Default.program_history != "")
            {
                Properties.Settings.Default.program_history = "";
                Properties.Settings.Default.Save();
            }
        }

    }
}
