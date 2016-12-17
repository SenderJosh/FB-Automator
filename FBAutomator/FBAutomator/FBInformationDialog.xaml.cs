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

namespace FBAutomator
{
    /// <summary>
    /// Interaction logic for FBInformationDialog.xaml
    /// </summary>
    public partial class FBInformationDialog : Window
    {
        public FBInformationDialog()
        {
            InitializeComponent();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        
        public string Email
        {
            get { return textBox.Text; }
        }
        public string Pass
        {
            get { return passwordBox.Password; }
        }
    }
}
