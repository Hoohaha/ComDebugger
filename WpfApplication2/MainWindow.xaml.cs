using System;
using System.Text;
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

        private bool ScriptsRuningState = false;
        private Thread ScriptsThread;
        private string FilePath = string.Empty;
        private string VirtualPort = "Virtual Port";
        private bool FirstConfig = true;
        private ObservableCollection<FileItem> ScriptsSource = new ObservableCollection<FileItem>();

        delegate void HanderInterfaceUpdataDelegate();
        private HanderInterfaceUpdataDelegate ToolBoxHandle;

        public MainWindow()
        {
            InitializeComponent();

            sp1 = new Serial(getData1);
            sp2 = new Serial(getData2);

            initial_combobox();
            //ScriptsList.ItemsSource = ScriptsSource;

            LoadCommandList();
            addPortNames(comboBox_Ports1, comboBox_Ports2);
            ScriptsList.ItemsSource = ScriptsSource;
        }


        // Serial initialization
        private List<string> getValidSerialPorts()
        {
            List<string> result = new List<string>();
            // use registry info to get com info
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey hs = hklm.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
            //int ccc = hs.ValueCount;
            try
            {
                for (int i = 0; i < hs.ValueCount; i++)
                {
                    result.Add(hs.GetValue(hs.GetValueNames()[i]).ToString());
                }
            }
            catch(Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
            hklm.Close();
            return result;
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
            if(PortListBox.Items.Count==0)
            {
                cbox1.SelectedIndex = 0;
                cbox2.SelectedIndex = 0;
            }
            cbox1.SelectedIndex = 1;
            cbox2.SelectedIndex = 2;
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
                sp.ReceivedBytesThreshold = 4096;
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
                sp.PanelObject = this;
                sp.ReadTimeout = 500;
                sp.WriteTimeout = 500;
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

            textbox1.AppendText(data);
            serial1.ScrollToEnd();
        }

        private void getData2(string data)
        {
            textbox2.AppendText(data);
            serial2.ScrollToEnd();
        }


        private void sendbox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox sendbox = (TextBox)sender;
                Serial sp;
                if (sendbox==sendbox1)
                {
                    sp = sp1;
                }
                else
                {
                    sp = sp2;
                }
                if (sp.IsOpening)
                {
                    sp.Send_Data(sendbox.Text);
                    sendbox.Text = "";
                }
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
            if (menu.Name == "Clear1")
            {
                textbox1.Text = "";
            }
            else
            {
                textbox2.Text = "";
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            if (menu.Name == "SelectAll1")
            {
                textbox1.SelectAll();
            }
            else
            {
                textbox2.SelectAll();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            if (menu.Name == "Copy1")
            {
                textbox1.Copy();
            }
            else
            {
                textbox2.Copy();
            }
            
        }
        private void Clear_All_Click(object sender, RoutedEventArgs e)
        {
            textbox1.Text = "";
            textbox2.Text = "";
        }
        #endregion



        #region Python Script Operation

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                String[] files = (String[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string s in files)
                {
                    AddItemToScriptSource(s);
                }
            }
        }

        private void ADDButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Python(*.py)|*.py|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                AddItemToScriptSource(dialog.FileName);
            }
        }

        private void AddItemToScriptSource(string filepath)
        {
            ScriptsSource.Add(new FileItem(filepath));
        }

        private void RUNButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptsList.SelectedItem != null)
            {
                Scripts_run();
            }
            else
            {
                MessageBox.Show("No file is selected");
            }
        }

        private void ScriptsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.ScriptsList.SelectedItem != null)
            {
                Scripts_run();
            }
        }

        private void Scripts_run()
        {
            if(!ScriptsRuningState)
            {
                if (!sp1.IsOpening || !sp2.IsOpening)
                {
                   MessageBoxResult res = MessageBox.Show("Make sure the port you wanted is open!\r\n  Click \"Yes\" to continue", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                   if (res != MessageBoxResult.Yes)
                       return;
                }
                FilePath = (string)ScriptsList.SelectedValue;
                ScriptsThread = new Thread(exectue_py);
                ScriptsThread.Start();
                ScriptsList.IsEnabled = false;
                Op_Button1.IsEnabled = false;
                Op_Button2.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                RUNButton.Content = "STOP";
                RUNButton.Background = colorblue;
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
            ScriptsList.IsEnabled = true;
            Op_Button1.IsEnabled = true;
            Op_Button2.IsEnabled = true;
            RefreshButton.IsEnabled = true;
            RUNButton.Content = "RUN";
            RUNButton.Background = colorwhite;
        }

        private void exectue_py()
        {
            ScriptsRuningState = true;
            ToolBoxHandle = new HanderInterfaceUpdataDelegate(EnableToolBoxs);

            string PythonPath = string.Empty;
            string PythonLibPath1 = string.Empty;
            string PythonLibPath2 = string.Empty;
            StreamReader file = new StreamReader("config/python_config.ini", Encoding.Default);
            PythonPath = file.ReadLine();
            PythonLibPath1 = file.ReadLine();
            PythonLibPath2 = file.ReadLine();
            file.Close();



            ScriptEngine engine = Python.CreateEngine();
            var PythonLibPaths = engine.GetSearchPaths();
            if (PythonPath!=string.Empty)
            {
                PythonLibPaths.Add(PythonPath+"/Lib/");
                PythonLibPaths.Add(PythonPath+"/Lib/site-packages/");
            }
            if (PythonLibPath1 != string.Empty)
            {
                PythonLibPaths.Add(PythonLibPath1);
            }
            if (PythonLibPath2 != string.Empty)
            {
                PythonLibPaths.Add(PythonLibPath2);
            }

            engine.SetSearchPaths(PythonLibPaths);
            ScriptScope scope = engine.CreateScope();
            sp1.SendNewLineIsEnabled=false;
            sp1.SendNewLineIsEnabled=false;
            scope.SetVariable("SEND1", (Action<string>)sp1.Send_Data);
            scope.SetVariable("SEND2", (Action<string>)sp2.Send_Data);
            scope.SetVariable("ScriptPath", FilePath);
            ScriptSource script = engine.CreateScriptSourceFromFile(@"interface/interface.py");
            try
            {
                var result = script.Execute(scope);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted.")
                {
                    string ErrorMessage = "Error: " + FilePath + "\r\n  " + ex.Message;
                    MessageBox.Show(ErrorMessage);
                }

            }
            Dispatcher.Invoke(ToolBoxHandle);
            ScriptsRuningState = false;
            sp1.SendNewLineIsEnabled = false;
            sp1.SendNewLineIsEnabled = false;
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

        private void LoadCommandList()
        {
            StreamReader file;
            file = new StreamReader("config/CPLD_COMMAND.ini", Encoding.Default);
            string s = "";
            while (s != null)
            {
                s = file.ReadLine();
                if (!string.IsNullOrEmpty(s))
                    CPLDList.Items.Add(s);
            }
            file.Close();

            file = new StreamReader("config/KABS_COMMAND.ini", Encoding.Default);
            s = "";
            while (s != null)
            {
                s = file.ReadLine();
                if (!string.IsNullOrEmpty(s))
                    KABSList.Items.Add(s);
            }
            file.Close();
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
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow ab = new AboutWindow();
            ab.Show();

        }


        private void SendNewLineChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            bool state = CheckReseult(checkbox);
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

        private bool CheckReseult(CheckBox checkbox)
        {

            if ((bool)(checkbox.IsChecked))
                return true;
            return false;
        }

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
            if (this.PortName == "Virtual Port")
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
            if (this.PortName == "Virtual Port")
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
            if (this.IsOpen)
            {
                n = this.BytesToRead;
                buf = new byte[n];
                this.Read(buf, 0, n);

                string abc = System.Text.Encoding.Default.GetString(buf);
                Thread.Sleep(60);
                panel.Dispatcher.BeginInvoke(DisplayHandle, new string[] { abc });
            }

        }

        public void Send_Data(string strSend)
        {
            try
            {
                if (SendNewLineState)
                {
                    if(strSend.IndexOf("\r\n")<0)
                        strSend += "\r\n";
                }
                if (VirtualOpen)
                {
                    panel.Dispatcher.Invoke(DisplayHandle, new string[] { strSend });
                    return;
                }
                else
                {
                    this.Write(strSend);
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
        public FileItem(string path)
        {
            _FileName = path.Substring(path.LastIndexOf("\\") + 1);
            _FilePath = path;
        }
        public string Name
        {
            get
            {
                return _FileName;
            }
        }
        public string Path
        {
            get
            {
                return _FilePath;
            }
        }

    }
}

