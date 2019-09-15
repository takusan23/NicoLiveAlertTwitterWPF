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

namespace NicoLiveAlertTwitterWPF.niconico
{
    /// <summary>
    /// NicoLoginMailPassDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class NicoLoginMailPassDialog : Window
    {
        public NicoLoginMailPassDialog()
        {
            InitializeComponent();
        }
        public string MailAdress
        {
            get { return NicoLoginMailBox.Text; }
            set { NicoLoginMailBox.Text = value; }
        }
        public string Password
        {
            get { return NicoLoginPassBox.Password.ToString(); }
            set { NicoLoginPassBox.Password = value; }
        }

        private void NicoLoginButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

        }
    }
}
