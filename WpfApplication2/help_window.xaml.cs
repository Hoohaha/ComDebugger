using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace COM_DEBUGGER
{
    /// <summary>
    /// help_window.xaml 的交互逻辑
    /// </summary>
    public partial class Help_window : Window
    {
        public Help_window()
        {
            InitializeComponent();  
        }



        private void English_IsEnabledChanged(object sender, RoutedEventArgs e)
        {
            FileStream fs = new FileStream("doc/Help-en.txt", FileMode.Open, FileAccess.Read);
            using (StreamReader streamReader = new StreamReader(fs, System.Text.Encoding.Default))
            {
                doc2.Text = streamReader.ReadToEnd();
            }
            fs.Close();
        }

        private void Chinese_IsEnabledChanged(object sender, RoutedEventArgs e)
        {
            FileStream fs = new FileStream("doc/Help-ch.txt", FileMode.Open, FileAccess.Read);
            using (StreamReader streamReader = new StreamReader(fs, System.Text.Encoding.Default))
            {
                doc1.Text = streamReader.ReadToEnd();
            }
            fs.Close();

        }
    }
}
