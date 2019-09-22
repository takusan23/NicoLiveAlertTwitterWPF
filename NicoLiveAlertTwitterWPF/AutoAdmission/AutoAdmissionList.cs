using Newtonsoft.Json;
using NicoLiveAlertTwitterWPF.JSONClass;
using NicoLiveAlertTwitterWPF.ListViewClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicoLiveAlertTwitterWPF.AutoAdmission
{
    class AutoAdmissionList
    {
        //ListViewのデータ
        public ObservableCollection<ProgramListViewData> list = new ObservableCollection<ProgramListViewData>();

        public void loadList(Boolean showDialog)
        {
            list.Clear();
            //追加
            if (Properties.Settings.Default.auto_admission_list != "")
            {
                //追加
                var account_list = Properties.Settings.Default.auto_admission_list;
                var admissionArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);
                //ListView
                foreach (var autoAdmission in admissionArray)
                {
                    //UnixTime→DateTime
                    var dateTime = FromUnixTime(autoAdmission.UnixTime);
                    var pos = admissionArray.IndexOf(autoAdmission);
                    var item = new ProgramListViewData { Name = autoAdmission.Name, Pos = pos, dateTime = "開場時間 : " + dateTime.ToString() };
                    list.Add(item);
                }
            }
            else
            {
                if (showDialog)
                {
                    //登録されてないよ！
                    showEmptyMessage();
                }
            }
        }

        private void showEmptyMessage()
        {
            //ダイアログ出す
            System.Windows.MessageBox.Show("予約枠自動入場とは？\n予約枠自動入場では予め予約枠を登録することで自動で入場する機能です。", "予約枠自動入場に登録されてないよ！", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        public void deleteAutoAdmission(int pos)
        {
            //削除ボタンおした

            var account_list = Properties.Settings.Default.auto_admission_list;
            var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);

            //確認ダイアログ
            var result = System.Windows.MessageBox.Show($"削除しますか？。\n{accountJSONArray[pos].Name}", "削除", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            //押したとき
            if (result == MessageBoxResult.OK)
            {
                //削除決行           
                accountJSONArray.RemoveAt(pos);
                //accountJSONArray.RemoveAt(delete_pos);
                //保存
                Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                Properties.Settings.Default.Save();
                //ListView更新
                loadList(false);
            }
        }
        private DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }

        public void addAdmission(string title, string id, long unix, Boolean showDialog)
        {
            //被らないようにする
            //今の予約枠自動登録リスト
            //設定読み込み
            var admissionList = new List<string>();
            //ないときもある
            if (Properties.Settings.Default.auto_admission_list != "")
            {
                var addAdmissionString = Properties.Settings.Default.auto_admission_list;
                var addAdmissionJsonArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(addAdmissionString);
                foreach (var admission in addAdmissionJsonArray)
                {
                    admissionList.Add(admission.ID);
                }
            }

            //UnixTime->DateTime
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;

            if (!admissionList.Contains(id))
            {
                //追加
                if (Properties.Settings.Default.auto_admission_list != "")
                {
                    //追加
                    var account_list = Properties.Settings.Default.auto_admission_list;
                    var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>(account_list);
                    accountJSONArray.Add(new AutoAdmissionJSON { Name = title, ID = id, UnixTime = unix });
                    //JSON配列に変換
                    Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                }
                else
                {
                    //初めて
                    var accountJSONArray = JsonConvert.DeserializeObject<List<AutoAdmissionJSON>>("[]");
                    accountJSONArray.Add(new AutoAdmissionJSON { Name = title, ID = id, UnixTime = unix });
                    //JSON配列に変換
                    Properties.Settings.Default.auto_admission_list = JsonConvert.SerializeObject(accountJSONArray);
                }
                Properties.Settings.Default.Save();
                loadList(false);
            }
            else
            {
                //被りダイアログ
                //自動追加のときは表示しないように
                if (showDialog)
                {
                    System.Windows.MessageBox.Show($"追加済みです。\n{title}  開場時間 : {dateTime.ToString()}", "追加", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
