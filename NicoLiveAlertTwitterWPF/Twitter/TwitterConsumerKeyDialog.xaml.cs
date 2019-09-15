using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NicoLiveAlertTwitterWPF.Twitter
{
    /// <summary>
    /// TwitterConsumerKeyDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class TwitterConsumerKeyDialog : Window
    {
        public TwitterConsumerKeyDialog()
        {
            InitializeComponent();
        }
        public string ConsumerKey
        {
            get { return TwitterConsumerKeyDialogBox.Text; }
            set { TwitterConsumerKeyDialogBox.Text = value; }
        }
        public string ConsumerSecret
        {
            get { return TwitterConsumerKeyDialogSecretBox.Text; }
            set { TwitterConsumerKeyDialogSecretBox.Text = value; }
        }
        private void TwitterCOnsumerKeyDialogButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
