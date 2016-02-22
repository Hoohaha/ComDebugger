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
        private string editorEnableFlag;
        private void Load_config()
        {
            string[] keys = new string[20];
            string[] values = new string[20];

            path1.Text = IniOperations.Read("python", "python_path", "config/conf.ini");
            path2.Text = IniOperations.Read("python", "python_lib1", "config/conf.ini");
            path3.Text = IniOperations.Read("python", "python_lib2", "config/conf.ini");
            EditorPath.Text = IniOperations.Read("editor", "path", "config/conf.ini");
            editorEnableFlag = IniOperations.Read("editor", "enable", "config/conf.ini");
            if (editorEnableFlag == "1")
            {
                editorEnable.IsChecked = true;
            }
            else
            {
                editorEnable.IsChecked = false;
            }
        }


        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigState1&&ConfigState2&&ConfigState3)
            {
                IniOperations.Write("python", "python_path", path1.Text, "config/conf.ini");
                IniOperations.Write("python", "python_lib1", path2.Text, "config/conf.ini");
                IniOperations.Write("python", "python_lib2", path3.Text, "config/conf.ini");
                IniOperations.Write("editor", "path", EditorPath.Text, "config/conf.ini");
                IniOperations.Write("editor", "enable", editorEnableFlag, "config/conf.ini");
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
            if ((p == string.Empty) || (p==""))
            {
                Notice.Text = "";
                return true;
            }
            else if(Directory.Exists(p))
            {
                Notice.Text = "yes";
                Notice.Foreground = new SolidColorBrush(Color.FromArgb(255, 103, 232, 109));
                return true;
            }
            else
            {
                Notice.Text = "Invalid";
                Notice.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                return false;
            }
                    
      }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                EditorPath.Text = dialog.FileName;
            }
        }


        private void editorEnable_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)editorEnable.IsChecked)
            {
                editorEnableFlag = "1";
            }
            else
            {
                editorEnableFlag = "0";
            }
        }
    }

}
