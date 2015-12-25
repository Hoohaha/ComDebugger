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
using Microsoft.Win32;
using System.IO;

namespace COM_DEBUGGER
{
    /// <summary>
    /// Interaction logic for OptionWindow.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        public OptionWindow()
        {
            InitializeComponent();
            Load_config();
        }
        private bool ConfigState1 = true;
        private bool ConfigState2 = true;
        private bool ConfigState3 = true;
        private void Load_config()
        {
            StreamReader file = new StreamReader("config/python_config.ini", Encoding.Default);
            path1.Text = file.ReadLine();
            path2.Text = file.ReadLine();
            path3.Text = file.ReadLine();
            file.Close();
        }


        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigState1&&ConfigState2&&ConfigState3)
            {
                StreamWriter file = new StreamWriter("config/python_config.ini");
                file.WriteLine(path1.Text);
                file.WriteLine(path2.Text);
                file.WriteLine(path3.Text);
                file.Close();
                this.Close();
            }
        }

        private void Cancle_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void Path_Judge_Event(object sender, RoutedEventArgs e)
        {
            TextBox path = (TextBox)sender;

            if (path == path1)
            {
               ConfigState1 = PathExists(path.Text, Notice1);

            }
            else if (path == path2)
            {

               ConfigState2 = PathExists(path.Text, Notice2);
            }
            else 
            {
                ConfigState3 = PathExists(path.Text, Notice3);
            }
        }

            private bool PathExists(string p, TextBlock Notice)
            {
                if (Directory.Exists(p))
                {
                    Notice.Text = "";
                    return true;
                }
                else if(p==string.Empty)
                {
                    Notice.Text = "";
                   return true;
                }
                else
                {
                    Notice.Text = "Invalid Path, please check!";
                    return false;
                }
                    
            }
        }

}
