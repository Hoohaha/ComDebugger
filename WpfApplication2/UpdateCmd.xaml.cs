using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;

namespace COM_DEBUGGER
{
    /// <summary>
    /// UpdateCmd.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateCmd : Window
    {
        public UpdateCmd()
        {
            InitializeComponent();
            textblock.Text = "CPLD_COMMAND.ini\r\n KABS_COMMAND.ini\r\nOpen file location to view";
        }


        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            string configpath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config";
            Process.Start("explorer.exe ", configpath);
        }
    }
}
