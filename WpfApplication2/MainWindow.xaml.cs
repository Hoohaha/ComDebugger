using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.IO;
using System.IO.Ports;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Documents;
using WPFAutoCompleteTextbox;

namespace COM_DEBUGGER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Serial sp1;
        private Serial sp2;
        private PortConfig PortCfgInfo1 = new PortConfig();
        private PortConfig PortCfgInfo2 = new PortConfig();

        private SolidColorBrush colorwhite = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        private SolidColorBrush colorblue = new SolidColorBrush(Color.FromArgb(255, 56, 150, 212));
        private SolidColorBrush colorblack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private SolidColorBrush colorred = new SolidColorBrush(Color.FromArgb(255, 255, 0,0));

        
        private Thread ScriptsThread;
        private string VirtualPort = "Virtual";
        private bool ScriptsRuningState = false;
        private bool FirstConfig = true;
        private bool TextMatchState = false;

        private ObservableCollection<FileItem> ScriptsSource;
        private ObservableCollection<CMDNode> CMDTree;

        delegate void HanderInterfaceUpdataDelegate();

        private string IniPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "app_info.ini";
        private List<string> ScriptsRunQueue = new List<string>();


        public MainWindow()
        {
            
            InitializeComponent();

            ScriptsSource = new ObservableCollection<FileItem>();
            sp1 = new Serial(getData1);
            sp2 = new Serial(getData2);

            ScriptsList.ItemsSource = ScriptsSource;

            sendbox1.TextBoxObject.UndoLimit = 20;
            sendbox2.TextBoxObject.UndoLimit = 20;
            sendbox2.ComboBoxBackground = colorblack;

            //initial port combobox
            initial_combobox();

            //load cpld command
            LoadCPLDCommand();
            //load kabs command
            LoadKABSCommand();

            //add ports from regex
            addPortNames(comboBox_Ports1, comboBox_Ports2);

        }


        // Serial initialization
        private List<string> getValidSerialPorts()
        {
            List<string> comlist = new List<string>();
            // use registry info to get com info
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey hs = hklm.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
            //int ccc = hs.ValueCount;
            try
            {
                for (int i = 0; i < hs.ValueCount; i++)
                {
                    comlist.Add(hs.GetValue(hs.GetValueNames()[i]).ToString());
                }
            }
            catch(Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
            hklm.Close();
            return comlist;
        }


        private bool addPortNames(ComboBox cbox1, ComboBox cbox2)
        {
            List<string> serialPorts = getValidSerialPorts();
            List<string> oldPorts = new List<string>();
            cbox1.Items.Clear();
            cbox2.Items.Clear();
            PortListBox.Items.Clear();

            cbox1.Items.Add(VirtualPort);
            cbox2.Items.Add(VirtualPort);

            foreach (string s in serialPorts)
            {
                cbox1.Items.Add(s);
                cbox2.Items.Add(s);
                PortListBox.Items.Add(s);
            }
            //if(PortListBox.Items.Count==0)
            //{
                cbox1.SelectedIndex = 0;
                cbox2.SelectedIndex = 0;
            //}
            //else if (PortListBox.Items.Count==1)
            //{
            //    cbox1.SelectedIndex = 1;
            //    cbox2.SelectedIndex = 0;
            //}
            //else if (PortListBox.Items.Count>=2)
            //{
            //    cbox1.SelectedIndex = 1;
            //    cbox2.SelectedIndex = 2;
            //}

            return true;
        }

        private void initial_combobox()
        {
            List<int> baudrate_collection = new List<int>{2400, 4800, 9600, 19200, 38400, 57600, 115200};
            List<int> databits_collection = new List<int> { 5, 6, 7, 8 };
            List<string> stopbits_collection = new List<string> { "1", "1.5", "2"};
            string[] ParityItemSource = System.Enum.GetNames(typeof(Parity));
            string[] HandShakeItemSource = System.Enum.GetNames(typeof(Handshake));
            //set items source
            comboBox_BaudRate1.ItemsSource = baudrate_collection;
            comboBox_Databits1.ItemsSource = databits_collection;
            comboBox_StopBits1.ItemsSource = stopbits_collection;
            comboBox_HandShake1.ItemsSource = HandShakeItemSource;
            comboBox_Parity1.ItemsSource = ParityItemSource;
            //init value
            comboBox_Databits1.SelectedItem = 8;
            comboBox_Parity1.SelectedItem = "None";
            comboBox_StopBits1.SelectedItem = "1";
            comboBox_HandShake1.SelectedItem = "None";
            comboBox_Parity1.SelectedItem = "None";
            comboBox_BaudRate1.SelectedItem = 115200;
            //set items source
            comboBox_BaudRate2.ItemsSource = baudrate_collection;
            comboBox_Databits2.ItemsSource = databits_collection;
            comboBox_StopBits2.ItemsSource = stopbits_collection;
            comboBox_HandShake2.ItemsSource = HandShakeItemSource;
            comboBox_Parity2.ItemsSource = ParityItemSource;
            //init value
            comboBox_Databits2.SelectedItem = 8;
            comboBox_Parity2.SelectedItem = "None";
            comboBox_StopBits2.SelectedItem = "1";
            comboBox_HandShake2.SelectedItem = "None";
            comboBox_Parity2.SelectedItem = "None";
            comboBox_BaudRate2.SelectedItem = 115200;
            FirstConfig = false;

            NewLineCheck1.IsChecked = true;
            NewLineCheck2.IsChecked = true;
        }
        private void ClosePorts()
        {
            Close_Serial1();
            Close_Serial2();
        }

        private void SetOpbuttonOpenState(Button button)
        {
            button.Background = colorwhite;
            button.Content = "Open";
            button.Foreground = colorblack;
            button.Focusable = false;
        }
        private void SetOpbuttonCloseState(Button button)
        {
            button.Background = colorblue;
            button.Content = "Close";
            button.Foreground = colorwhite;
            button.Focusable = false;
        }
        private void Button_Open1(object sender, RoutedEventArgs e)
        {
            Serial sp;
            PortConfig PortCfgInfo;
            Button button = (Button)sender;

            if (button == Op_Button1)
            {
                sp = sp1;
                PortCfgInfo = PortCfgInfo1;
            }
            else
            {
                sp = sp2;
                PortCfgInfo = PortCfgInfo2;
            }

            if (sp.IsOpening)
            {
                //sp.ReceivedBytesThreshold = 4096;
                sp.DataReceived -= new SerialDataReceivedEventHandler(sp.Data_Received);
                sp.Closes();
                SetOpbuttonOpenState(button);
            }
            else
            {
                if (PortCfgInfo.portname == null)
                {
                    MessageBox.Show("Please set Port!");
                    return;
                }
                if (PortCfgInfo.baudrate == 0)
                {
                    MessageBox.Show("Please set BaudRate!");
                    return;
                }
                sp.Config(PortCfgInfo);//config portname baudrate etc...
                sp.ReceivedBytesThreshold = 1024;
                sp.PanelObject = this;
                sp.ReadTimeout = 300;
                sp.WriteTimeout = 300;
                sp.ReceivedBytesThreshold = 1;
                sp.DtrEnable = true;
                sp.RtsEnable = true;
                sp.DataReceived += new SerialDataReceivedEventHandler(sp.Data_Received);
                
                try
                {
                    sp.Opens();
                    SetOpbuttonCloseState(button);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                

            }

        }

        private void getData1( string data)
        {

            textbox1.AppendText(data);//AppendText(data);
            serial1.ScrollToEnd();
        }

        private void getData2(string data)
        {
            textbox2.AppendText(data);
            serial2.ScrollToEnd();
        }


        private void sendbox1_KeyDown(object sender, KeyEventArgs e)
        {
            MenuItem ClearMenu;

            if (e.Key == Key.Enter)
            {
                var sendbox = (AutoCompleteTextBox)sender;
                string SendText = sendbox.Text;
                sendbox.Text = string.Empty;

                if (sendbox == sendbox1)
                {
                    ClearMenu = ClearLMenu;
                }
                else
                {
                    ClearMenu = ClearRMenu;
                }

                sendbox.IsSuggestionOpen = false;
                sendbox.TextBoxFocus();

                switch (SendText)
                {
                    case "cls":
                        ClearMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        return;
                    case "clss":
                        ClearAllMenu1.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        return;
                    case "cpld":
                        CPLD_Check.IsChecked = true;
                        CPLD_Check.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                        return;
                }

                if (sendbox == sendbox1)
                {
                    if (!sp1.IsOpening) return;
                    sp1.Send_Data(SendText);
                }
                else
                {
                    if (!sp2.IsOpening) return;
    
                    //serail port2 send data    
                    sp2.Send_Data(SendText);

                    //clear sendbox2 Text
                    if ((bool)CPLD_Check.IsChecked)
                    {  
                        //if cpld checked, text initial content is "$"
                        sendbox2.Text = "$";
                        sendbox2.TextBoxObject.Select(sendbox2.Text.Length, 0);
                    }

                }
            }
        }



        private void sendbox2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var sendbox = (AutoCompleteTextBox)sender;
            if  (e.Key == Key.Down)
            {
                if (sendbox.IsSuggestionOpen)
                {
                    sendbox.ComboBoxFocus();
                }
                else
                {
                    sendbox.TextBoxObject.Redo();
                }
            }
            else if (e.Key == Key.Up)
            {
                if (sendbox.TextBoxObject.IsFocused)
                     sendbox.TextBoxObject.Undo();
                
            }
            else if  (e.Key==Key.Right)
            {
                sendbox.IsSuggestionOpen = false;
            }
        }




        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }





        #region MenuItem_Event
        private void Clear_Serial1_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            
            if (menu.Header.ToString() == "Clear Left")
            {
                Paragraph paraa = new Paragraph();
                textbox1.Document.Blocks.Clear();
                textbox1.Document.Blocks.Add(paraa);
            }
            else
            {
                Paragraph parab = new Paragraph();
                textbox2.Document.Blocks.Clear();
                textbox2.Document.Blocks.Add(parab);
            }
        }


        private void Clear_All_Click(object sender, RoutedEventArgs e)
        {
            Paragraph paraa = new Paragraph();
            Paragraph parab = new Paragraph();
            textbox1.Document.Blocks.Clear();
            textbox1.Document.Blocks.Add(paraa);

            textbox2.Document.Blocks.Clear();
            textbox2.Document.Blocks.Add(parab);
        }
        #endregion



        #region Python Script Operation
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string p in files)
                {
                    ScriptsSource.Add(new FileItem(p));
                }
            }
        }

        private void ADDButton_Click(object sender, RoutedEventArgs e)
        {
            ADDButton.Focusable = false;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            //dialog.Filter = "Python(*.py)|*.py|YML(*.yml)|*.yml|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                foreach(string p in dialog.FileNames)
                {
                    ScriptsSource.Add(new FileItem(p));
                }
            }
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            AddFolder.Focusable = false;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                ScriptsSource.Add(new FileItem(fbd.SelectedPath));
            }
        }


        private void RUNButton_Click(object sender, RoutedEventArgs e)
        {
            RUNButton.Focusable = false;
            if (ScriptsList.SelectedItem != null)
            {
                Scripts_run();
            }
            else
            {
                MessageBox.Show("No file is selected");
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            ViewButton.Focusable = false;
            if (ScriptsList.SelectedIndex == -1)
            {
                MessageBox.Show("No file is selected!");
                return;
            }
            string EditorPath = string.Empty;
            string filepath = (string)ScriptsList.SelectedValue;
            string EnableFlag = IniOperations.Read("editor", "enable", "config/conf.ini");
            if (EnableFlag == "1")
            {
                EditorPath = IniOperations.Read("editor", "path", "config/conf.ini");
                if ((Path.GetExtension(EditorPath)==".exe")&&(File.Exists(EditorPath)))
                {
                    Process GProcess = new Process();
                    GProcess.StartInfo.FileName = EditorPath;
                    GProcess.StartInfo.Arguments = filepath;
                    GProcess.StartInfo.UseShellExecute = false;
                    GProcess.Start();
                }
                else
                {
                    MessageBox.Show("Editor path is invalid, please config the right value in Options panel!");
                }

            }
            else
            {
                LoadTextFile(textbox2, filepath);
            }
        }

        private void LoadTextFile(RichTextBox richTextBox, string filename)
        {
            richTextBox.Document.Blocks.Clear();
            using (StreamReader streamReader = File.OpenText(filename))
            {
                Paragraph paragraph = new Paragraph(new Run(streamReader.ReadToEnd()));
                richTextBox.Document.Blocks.Add(paragraph);
            }
        }

        private void ScriptsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.ScriptsList.SelectedItem != null)
            {
                Scripts_run();
            }
        }
        private void YamlPaserProcess(string YamlPath)
        {
            Process GProcess = new Process();
            GProcess.StartInfo.FileName = @"python ";
            GProcess.StartInfo.Arguments = @"interface/YamlPaser.py " + YamlPath;
            GProcess.StartInfo.UseShellExecute = false;
            GProcess.StartInfo.CreateNoWindow = true;
            GProcess.Start();
            GProcess.WaitForExit();
        }

        private void Scripts_run()
        {
            if (!ScriptsRuningState)
            {
                if (!sp1.IsOpening || !sp2.IsOpening)
                {
                   MessageBoxResult res = MessageBox.Show("Make sure the port you wanted is open!\r\n  Click \"Yes\" to continue", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                   if (res != MessageBoxResult.Yes)
                       return;
                }

                string CurrentPath = (string)ScriptsList.SelectedValue;
                ScriptsRunQueue.Clear();
                if ( Directory.Exists(CurrentPath))
                {
                    Paragraph paraa = new Paragraph();
                    textbox1.Document.Blocks.Clear();
                    textbox1.Document.Blocks.Add(paraa);

                    Paragraph parab = new Paragraph();
                    textbox2.Document.Blocks.Clear();
                    textbox2.Document.Blocks.Add(parab);

                    string infopath = CurrentPath + "/info.yml";
                    if (File.Exists(infopath))
                    {
                        YamlPaserProcess(infopath);
                        TextMatchState = true;

                        string assistant_init = IniOperations.Read("others", "assistant_init", IniPath);
                        if (assistant_init == "1")
                        {
                            string assistant_init_path = CurrentPath + "/assistant_init.py";
                            if (File.Exists(assistant_init_path))
                            {
                                ScriptsRunQueue.Add(assistant_init_path);
                            }
                        }
                        string interact_path = CurrentPath + "/interact.py";
                        if (File.Exists(interact_path))
                        {
                            ScriptsRunQueue.Add(interact_path);
                        }
                    }
                }
                else if (Path.GetExtension(CurrentPath)==".yml")
                {
                    YamlPaserProcess(CurrentPath);
                    TextMatch();
                    return;
                }
                else
                {
                    ScriptsRunQueue.Add(CurrentPath);
                    
                }
                
                ScriptsThread = new Thread(exectue_py);
                ScriptsThread.Start();
                ScriptsList.IsEnabled = false;
                Op_Button1.IsEnabled = false;
                Op_Button2.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                RUNButton.Content = "STOP";
                RUNButton.Foreground = colorwhite;
                RUNButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 121, 121));
                RUNButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 121, 121));
            }
            else
            {
                ScriptsRuningState = false;
                ScriptsThread.Abort();
                EnableToolBoxs();
            }
        }

        private void EnableToolBoxs()
        {
            while (true)
            {
                if (MessageBox.Show("Finished!") == MessageBoxResult.OK)
                    break;
            }

            ScriptsList.IsEnabled = true;
            Op_Button1.IsEnabled = true;
            Op_Button2.IsEnabled = true;
            RefreshButton.IsEnabled = true;
            RUNButton.Content = "Run";
            RUNButton.Background = colorwhite;
            RUNButton.Foreground = colorblack;
        }

        private void TextMatch()
        {
            FlowDocument mcFlowDoc = new FlowDocument();
            string[] keys = new string[50];
            string[] values = new string[50];
            bool r = false;
            Regex Pattern;

            StringBuilder TextTemp = new StringBuilder();
            TextRange textRange = new TextRange(textbox1.Document.ContentStart,textbox1.Document.ContentEnd);
            TextTemp.Append(textRange.Text);
            textRange = new TextRange(textbox2.Document.ContentStart, textbox2.Document.ContentEnd);
            TextTemp.Append(textRange.Text);

            Paragraph para = new Paragraph();
            para.Foreground = new SolidColorBrush(Color.FromArgb(255, 108, 68, 68));
            var Title = new Run("=====================================\r\n--------- Match Result -------\r\n");
            para.Inlines.Add(Title);
            para.Inlines.Add(new Run("Expect Pattern:\r\n"));

            //Get Pattern
            IniOperations.GetAllKeyValues("pattern", out keys, out values, IniPath);

            foreach (string pa in values)
            {
                Pattern = new Regex(pa);
                r = Pattern.IsMatch(TextTemp.ToString());

                if (r)
                {
                    var run = new Run("   " + pa + "   Match\r\n");
                    run.Foreground = new SolidColorBrush(Color.FromArgb(255, 102, 217, 239));
                    para.Inlines.Add(run);
                }
                else
                {
                    var run = new Run("   " + pa + "   Failed to Match\r\n");
                    run.Foreground = colorred;
                    para.Inlines.Add(run);
                }
            }

            //Get No Pattern
            para.Inlines.Add(new Run("Expect No Pattern:\r\n"));
            IniOperations.GetAllKeyValues("no_pattern", out keys, out values, IniPath);

            foreach (string npa in values)
            {
                Pattern = new Regex(npa);
                r = Pattern.IsMatch(TextTemp.ToString());

                if (r)
                {
                    var run = new Run("   " + npa + "   Appear\r\n");
                    run.Foreground = colorred;
                    para.Inlines.Add(run);
                }
                else
                {
                    var run = new Run("   " + npa + "   Not Appear\r\n");
                    run.Foreground = new SolidColorBrush(Color.FromArgb(255, 102, 217, 239));
                    para.Inlines.Add(run);
                }
            }
            textbox2.Document.Blocks.Add(para);
            TextMatchState = false;
        }

        private void exectue_py()
        {
            HanderInterfaceUpdataDelegate DeleEnableToolBox = new HanderInterfaceUpdataDelegate(EnableToolBoxs);
            HanderInterfaceUpdataDelegate DeleTextMatch = new HanderInterfaceUpdataDelegate(TextMatch);

            string PythonPath = IniOperations.Read("python", "python_path", "config/conf.ini");
            string PythonLibPath1 = IniOperations.Read("python", "python_lib1", "config/conf.ini");
            string PythonLibPath2 = IniOperations.Read("python", "python_lib2", "config/conf.ini");
            ScriptEngine engine;

            //init serial state
            ScriptsRuningState = true;

            //init python environment
            try
            {
                 engine = Python.CreateEngine();
            }
            catch
            {
                return;
            }
            var PythonLibPaths = engine.GetSearchPaths();
            if (Directory.Exists(PythonPath))
            {
                PythonLibPaths.Add(PythonPath+"/Lib/");
                PythonLibPaths.Add(PythonPath+"/Lib/site-packages/");
            }
            if (Directory.Exists(PythonLibPath1))
            {
                PythonLibPaths.Add(PythonLibPath1);
            }
            if (Directory.Exists(PythonLibPath2))
            {
                PythonLibPaths.Add(PythonLibPath2);
            }
            engine.SetSearchPaths(PythonLibPaths);
            ScriptScope scope = engine.CreateScope();
            scope.SetVariable("SEND1", (Action<string>)sp1.Send_Data);
            scope.SetVariable("SEND2", (Action<string>)sp2.Send_Data);
            foreach(string file in ScriptsRunQueue)
            {
                scope.SetVariable("ScriptPath", file);
                ScriptSource script = engine.CreateScriptSourceFromFile(@"interface/interface.py");
                try
                {
                    var result = script.Execute(scope);//run
                }
                catch (Exception ex)
                {
                    if (ex.Message != "Thread was being aborted.")
                    {
                        string ErrorMessage = "Error: " + file + "\r\n  " + ex.Message;
                        MessageBox.Show(ErrorMessage);
                    }
                }
            }
            //Match regular
            if (TextMatchState)
            {
                Dispatcher.Invoke(DeleTextMatch);
            }

            //enable tool box
            Dispatcher.Invoke(DeleEnableToolBox);
            //recover serail state
            ScriptsRuningState = false;


        }
        #endregion



        #region Get Port Configuration
        private Parity getSerialParity(ComboBox cbox)
        {
            int idx = cbox.SelectedIndex;

            switch (idx)
            {
                case 0:
                    return Parity.None;
                case 1:
                    return Parity.Odd;
                case 2:
                    return Parity.Even;
                case 3:
                    return Parity.Mark;
                case 4:
                    return Parity.Space;
                default:
                    return Parity.None;
            }
        }

        private StopBits getSerialStopBits(ComboBox cbox)
        {
            int idx = cbox.SelectedIndex;

            switch (idx)
            {
                case 0:
                    return StopBits.One;
                case 1:
                    return StopBits.OnePointFive;
                case 2:
                    return StopBits.Two;
                default:
                    return StopBits.One;
            }
        }

        private Handshake getSerialHandshake(ComboBox cbox)
        {
            int idx = cbox.SelectedIndex;

            switch (idx)
            {
                case 0:
                    return Handshake.None;
                case 1:
                    return Handshake.XOnXOff;
                case 2:
                    return Handshake.RequestToSend;
                case 3:
                    return Handshake.RequestToSendXOnXOff;
                default:
                    return Handshake.None;
            }
        }

        private void Close_Serial1()
        {
            if (sp1.IsOpening)
                Op_Button1.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void Close_Serial2()
        {
            if (sp2.IsOpening)
                Op_Button2.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        //Ports
        private void Ports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || sp1==null)
                return;
            if (cbox==comboBox_Ports1)
            {
                PortCfgInfo1.portname = cbox.SelectedItem.ToString();
                Close_Serial1();

                if (!FirstConfig)
                {
                    Op_Button1.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            else
            {
                PortCfgInfo2.portname = cbox.SelectedItem.ToString();
                Close_Serial2();

                if (!FirstConfig)
                {
                    Op_Button2.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }
        //BaudRate
        private void comboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || sp1 == null)
                return;
            if (cbox == comboBox_BaudRate1)
            {
                PortCfgInfo1.baudrate = (int)cbox.SelectedItem;
                Close_Serial1();

                if (!FirstConfig)
                {
                    Op_Button1.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                }
            }
            else
            {
                PortCfgInfo2.baudrate = (int)cbox.SelectedItem;
                Close_Serial2();
                if (!FirstConfig)
                {
                    Op_Button2.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                    
            }
        }
        //DataBits
        private void comboBox_Databits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox == comboBox_Databits1)
            {
                PortCfgInfo1.databits = (int)cbox.SelectedItem;
            }
            else
            {
                PortCfgInfo2.databits = (int)cbox.SelectedItem;
            }
            
        }
        //StopBits
        private void comboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox == comboBox_StopBits1)
            {
                PortCfgInfo1.stopbits = getSerialStopBits(cbox);
            }
            else
            {
                PortCfgInfo2.stopbits = getSerialStopBits(cbox);
            }
        }

        //Parity
        private void comboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox == comboBox_Parity1)
            {
                PortCfgInfo1.parity = getSerialParity(cbox);
            }
            else
            {
                PortCfgInfo2.parity = getSerialParity(cbox);
            }

        }
        //HandShake
        private void comboBox_HandShake_Selected(object sender, RoutedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox == comboBox_HandShake1)
            {
                PortCfgInfo1.handshake = getSerialHandshake(cbox);
            }
            else
            {
                PortCfgInfo2.handshake = getSerialHandshake(cbox);
            }
        }


        #endregion

        private void RefreshButton_Click_1(object sender, RoutedEventArgs e)
        {
            comboBox_Ports1.IsEnabled = false;
            comboBox_Ports2.IsEnabled = false;
            ClosePorts();
            addPortNames(comboBox_Ports1, comboBox_Ports2);
            comboBox_Ports1.IsEnabled = true;
            comboBox_Ports2.IsEnabled = true;
        }

        private void LoadCPLDCommand()
        {
            StreamReader file;
            file = new StreamReader("config/CPLD_COMMAND.ini", Encoding.Default);
            string s = "";
            while (s != null)
            {
                s = file.ReadLine();
                if (!string.IsNullOrEmpty(s))
                {
                    CPLDList.Items.Add(s);
                    sendbox2.AddItem(s);
                }
                    
            }
            file.Close();
        }

        private void LoadKABSCommand()
        {
            TreeViewItem item = new TreeViewItem();

            string[] keys = new string[50];
            string[] values = new string[50];
            string[] sections = new string[100];

            CMDTree = new ObservableCollection<CMDNode>();

            IniOperations.GetAllSectionNames(out sections, "config/KABS_COMMAND.ini");

            int i = 0, j = 0;

            for (i = 0; i < sections.Length; i++)
            {
                IniOperations.GetAllKeyValues(sections[i], out keys, out values, "config/KABS_COMMAND.ini");
                CMDNode node = new CMDNode() { NodeName = sections[i], ID = 1 };
                for (j = 0; j < keys.Length; j++)
                {
                    node.NextNode.Add(new CMDNode { NodeName = keys[j], ID = 2 });
                    sendbox2.AddItem(keys[j]);
                }

                CMDTree.Add(node);
            }
            treeView.ItemsSource = CMDTree;
        }


        private void CommandList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string text = "";
            ListBox commandlist = (ListBox)sender;
            if (commandlist.SelectedItem != null)
            {
                text = (string)commandlist.SelectedItem + "\r\n";
                sp2.Send_Data(text);
            }
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow opwin = new OptionWindow();
            opwin.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow ab = new AboutWindow();
            ab.Show();

        }


        private void SendNewLineChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            bool state = (bool)(checkbox.IsChecked);
            if (sp1 == null)
                return;
            if (checkbox==NewLineCheck1)
            {
                sp1.SendNewLineIsEnabled = state;
            }
            else
            {
                sp2.SendNewLineIsEnabled = state;
            }
        }



        private void treeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var tree = sender as TreeView;
                CMDNode item = tree.SelectedItem as CMDNode;
                if (item.ID == 2)
                    sp2.Send_Data(item.NodeName);
            }
            catch
            { }
        }



        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help_window Hw = new Help_window();
            Hw.Show();
        }



        private void ScriptsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string p = (string)ScriptsList.SelectedValue;
            if (File.Exists(p))
            {
                ViewButton.IsEnabled = true;
                if(Path.GetExtension(p)==".yml")
                {
                    RUNButton.Content = "Match";
                }
                else
                {
                    RUNButton.Content = "Run";
                }
                   
            }
            else
            {
                ViewButton.IsEnabled = false;
                RUNButton.Content = "Run";
            }
                
        }

        private void CPLD_Check_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            string temp = string.Empty;

            if ((bool)checkbox.IsChecked)
            {
                if (sendbox2.Text.StartsWith("$")) return;

                temp = "$" + sendbox2.Text;
                sendbox2.Text = temp;
                sendbox2.TextBoxObject.Select(sendbox2.Text.Length,0);
            }
            else
            {
                if (sendbox2.Text.StartsWith("$"))
                {
                    temp = sendbox2.Text;
                    sendbox2.Text = Regex.Replace(temp , @"\$","");
                    sendbox2.TextBoxObject.Select(sendbox2.Text.Length, 0);
                }
            }
        }

        private void Evaluation_Click(object sender, RoutedEventArgs e)
        {
            Evaluation ew = new Evaluation();
            ew.ShowDialog();
        }

        private void UpdateCommands_Click(object sender, RoutedEventArgs e)
        {
            string configpath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config";
            Process.Start("explorer.exe ", configpath);
        }
    }

    public class CMDNode
    {
        public CMDNode(){ NextNode = new ObservableCollection<CMDNode>(); }
        public int ID { set; get; }
        public string NodeName { set; get; }
        public ObservableCollection<CMDNode> NextNode { get; set; }
    }

    public class PortConfig
    {
        public string portname;
        public int baudrate;
        public int databits;
        public Parity parity;
        public StopBits stopbits;
        public Handshake handshake;

        public PortConfig()
        {}
    }


    #region Serial class
    public class Serial : SerialPort
    {
        delegate void HanderInterfaceUpdataDelegate(string data);
        private HanderInterfaceUpdataDelegate DisplayHandle;
        private Window panel;
        private bool SendNewLineState = true;
        int n;
        byte[] buf;
        private bool SCRIPTRUN = false;
        private bool VirtualOpen = false;

        public Serial(Action<string> DisplayData)
        {
            DisplayHandle = new HanderInterfaceUpdataDelegate(DisplayData);
        }

        public Window PanelObject
        {
            set { panel= value;}
        }

        public bool SendNewLineIsEnabled
        {
            set { SendNewLineState = value; }
        }

        private bool GetState()
        {
            if (this.IsOpen || this.VirtualOpen)
            {
                return true;
            }
            return false;
        }

        public bool IsOpening
        {
            get { return this.GetState(); }
        }

        public bool IsScriptRun
        {
            set { SCRIPTRUN = value; }
        }

        public void Config(PortConfig portcfg)
        {
            this.PortName = portcfg.portname;
            this.BaudRate = portcfg.baudrate;
            this.DataBits = portcfg.databits;
            this.Parity = portcfg.parity;
            this.StopBits = portcfg.stopbits;
            this.Handshake = portcfg.handshake;
        }


        public void Opens()
        {
            if (this.PortName == "Virtual")
            {
                VirtualOpen = true;
            }
            else
            {
                this.Open();
            }
        }

        public void Closes()
        {
            if (PortName == "Virtual")
            {
                VirtualOpen = false;
            }
            else
            {
                this.Close();
            }
        }

        public void Data_Received(object sender, SerialDataReceivedEventArgs e)
        {
            if (IsOpen)
            {
                n = BytesToRead;
                buf = new byte[n];
                Read(buf, 0, n);
                string abc =Encoding.Default.GetString(buf);
                Thread.Sleep(20);
                panel.Dispatcher.BeginInvoke(DisplayHandle, new string[] { abc });
            }

        }

        public void Send_Data(string strSend)
        {
            try
            {
                if ((SendNewLineState)&&(strSend.IndexOf("\r\n")<0))
                { 
                    strSend += "\r\n";
                }
                //virtual send
                if (VirtualOpen)
                {
                    panel.Dispatcher.Invoke(DisplayHandle, new string[] { strSend });
                    return;
                }
                else
                {
                    Write(strSend);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not to send data!\r\n"+e.Message);
            }
        }
    }
    #endregion

    public class FileItem
    {
        private string _FileName;
        private string _FilePath;
        private string _DirName;
        public FileItem(string path)
        {
           
            string[] elements = Regex.Split(path, @"\\");
            int l = elements.Length;
            _FileName = string.Format("{0} - {1}",elements[l - 2], elements[l - 1]);
            _FilePath = path;
            _DirName = Path.GetDirectoryName(path);
        }
        public string Name
        {
            get
            {
                return _FileName;
            }
        }
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }
        public string Dirname
        {
            get
            {
                return _DirName;
            }
        }
    }


}

