using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicoLiveAlertTwitterWPF.Info
{
    class AppUpdateCheck
    {
        //GitHubのReleaseに追加していくのでGitHubのReleaseのAPIを叩く
        public async void checkNewVersion()
        {
            var url = "https://api.github.com/repos/takusan23/NicoLiveAlertTwitterWPF/releases/latest";
            // 指定したサイトのHTMLをストリームで取得する
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "NicoLiveAlert_Twitter;@takusan_23");
            using (var stream = await client.GetAsync(new Uri(url)))
            {
                if (stream.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = JsonConvert.DeserializeObject<JSONClass.GitHubReleaseRootObject>(await stream.Content.ReadAsStringAsync());

                    var name = json.name;
                    var created = json.created_at;
                    var body = json.body;

                    //ダウンロードリンク
                    var downloadLink = json.assets[0].browser_download_url;

                    //バージョンが違うときは出す
                    if (MainWindow.AppVersion != name)
                    {
                        //ダイアログ表示
                        var result = MessageBox.Show($"現在の最新バージョンは以下のとおりです。\n{name} {created}\n{body}\nダウンロードする場合は「はい」を押してね。", "更新の確認", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(downloadLink);
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("このバージョンは最新のバージョンです。", "更新の確認", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var result = MessageBox.Show("更新の確認に問題が発生しました。", "更新の確認", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
