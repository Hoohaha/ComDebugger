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
        private string VirtualPort = "Virtual";
        private PortConfig PortCfgInfo2 = new PortConfig();
        private ObservableCollection<FileItem> ScriptsSource;
        private ObservableCollection<CMDNode> CMDTree;

        delegate void HanderInterfaceUpdataDelegate();

        private string IniPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "app_info.ini";
        private List<string> ScriptsRunQueue = new List<string>();

        public MainWindow()
        {
            
            InitializeComponent();

            ScriptsSource = new ObservableCollection<FileItem>();

            //ScriptsList.ItemsSource = ScriptsSource;

            //load cpld command
            LoadCPLDCommand();
            //load kabs command
            LoadKABSCommand();

            //add ports from regex
            addPortNames();
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


        private bool addPortNames()
        {
            List<string> serialPorts = getValidSerialPorts();
            List<string> oldPorts = new List<string>();
            SP1.ports_list.Clear();
            SP2.ports_list.Clear();
            PortListBox.Items.Clear();

            SP1.ports_list.Add(VirtualPort);
            SP2.ports_list.Add(VirtualPort);

            foreach (string s in serialPorts)
            {
                SP1.ports_list.Add(s);
                SP2.ports_list.Add(s);
                PortListBox.Items.Add(s);
            }
            SP1.PortsComboBox.SelectedIndex = 0;
            SP2.PortsComboBox.SelectedIndex = 0;
            return true;
        }



        private void ClosePorts()
        {
            SP1.Close_Port();
            SP2.Close_Port();
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
                SP1.ClearDisplay();
            }
            else
            {
                SP2.ClearDisplay();
            }
        }


        private void Clear_All_Click(object sender, RoutedEventArgs e)
        {
            SP1.ClearDisplay();
            SP2.ClearDisplay();
        }

        
        private void SendBreak_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;

            if (menu.Name.Contains("2"))
            {
                SP2.SendBreak();
            }
            else
            {
                SP1.SendBreak();
            }
           
        }
        #endregion



        #region Python Script Operation
        //private void ListBox_Drop(object sender, DragEventArgs e)
        //{
        //    if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        //    {
        //        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

        //        foreach (string p in files)
        //        {
        //            ScriptsSource.Add(new FileItem(p));
        //        }
        //    }
        //}

      


        //private void LoadTextFile(RichTextBox richTextBox, string filename)
        //{
        //    richTextBox.Document.Blocks.Clear();
        //    using (StreamReader streamReader = File.OpenText(filename))
        //    {
        //        Paragraph paragraph = new Paragraph(new Run(streamReader.ReadToEnd()));
        //        richTextBox.Document.Blocks.Add(paragraph);
        //    }
        //}



        //private void YamlPaserProcess(string YamlPath)
        //{
        //    Process GProcess = new Process();
        //    GProcess.StartInfo.FileName = @"python ";
        //    GProcess.StartInfo.Arguments = @"interface/YamlPaser.py " + YamlPath;
        //    GProcess.StartInfo.UseShellExecute = false;
        //    GProcess.StartInfo.CreateNoWindow = true;
        //    GProcess.Start();
        //    GProcess.WaitForExit();
        //}

        #endregion






        private void RefreshButton_Click_1(object sender, RoutedEventArgs e)
        {

            ClosePorts();
            addPortNames();
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
                    SP2.SendBox.AddItem(s);
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
                    SP2.SendBox.AddItem(keys[j]);
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
                SP2.SendData(text);
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

            if (checkbox==NewLineCheck1)
            {
                SP1.SendNewLine = state;
            }
            else
            {
                SP2.SendNewLine = state;
            }
        }


        private void HexDisplayChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            bool state = (bool)(checkbox.IsChecked);

            if (checkbox.Name.Contains("1"))
            {
                SP1.ShowHexString(state);
            }
            else
            {
                SP2.ShowHexString(state);
            }
        }


        private void treeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var tree = sender as TreeView;
                CMDNode item = tree.SelectedItem as CMDNode;
                if (item.ID == 2)
                    SP2.SendData(item.NodeName);
            }
            catch
            { }
        }



        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help_window Hw = new Help_window();
            Hw.Show();
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
        private bool SCRIPTRUN = false;
        private bool VirtualOpen = false;
        private List<byte> receivedBytesQueue;


        public Serial(Action<string> DisplayData)
        {
            ReadBufferSize = 1024;
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
                VirtualOpen = false;
                this.Open();
                start_read();
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
                byte[]  buf = new byte[n];
                Read(buf, 0, n);
                string abc = Encoding.Default.GetString(buf);
                Thread.Sleep(10);
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, DisplayHandle, abc);
            }
        }
        

        public void start_read()
        {
            byte[] buffer = new byte[2000];
            Action kickoffRead = null;

            if (!IsOpen)
                return;

            kickoffRead = (Action)(() => 
                this.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
            {
                if (!IsOpen)
                    return;

                try
                {
                    int count = this.BaseStream.EndRead(ar);
                    byte[] dst = new byte[count];
                    Buffer.BlockCopy(buffer, 0, dst, 0, count);

                    //receivedBytesQueue.AddRange(dst);
                    //ThreadPool.QueueUserWorkItem(handleReceivedBytes);


                    Thread.Sleep(20);
                    RaiseAppSerialDataEvent(dst);
                }
                catch (Exception e)
                {
                    MessageBox.Show("ERROR == > " + e.ToString());
                }
                try
                {
                    kickoffRead();
                }
                catch { }
                
            }, null));


            kickoffRead();

           
        }

        private void RaiseAppSerialDataEvent(byte[] Data)
        {
            string Result = Encoding.Default.GetString(Data);
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, DisplayHandle, Result);
            //panel.Dispatcher.Invoke(DisplayHandle, Result);
        }


        public void Send_Break()
        {
            if (VirtualOpen)
            {
                return;
            }

            if (!IsOpen)
            {
                MessageBox.Show("Port is not open");
                return;
            }
            BreakState = true;
            Thread.Sleep(100);
            BreakState = false;
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
                    Application.Current.Dispatcher.BeginInvoke(DisplayHandle, new string[] { strSend });
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

