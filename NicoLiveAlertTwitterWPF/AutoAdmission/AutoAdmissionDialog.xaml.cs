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

namespace NicoLiveAlertTwitterWPF.AutoAdmission
{
    /// <summary>
    /// AutoAdmissionDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AutoAdmissionDialog : Window
    {
        public AutoAdmissionDialog()
        {
            InitializeComponent();
        }
        public string ProgramID
        {
            get { return AutoAdmissionAddDialogProgramID.Text; }
            set { AutoAdmissionAddDialogProgramID.Text = value; }
        }

        private void AutoAdmissionAddDialogButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
