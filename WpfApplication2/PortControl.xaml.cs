﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using WPFAutoCompleteTextbox;
using System.Collections.ObjectModel;
using System.Threading;


namespace COM_DEBUGGER
{
    /// <summary>
    /// Interaction logic for PortControl.xaml
    /// </summary>
    public partial class PortControl : UserControl
    {
        private Serial serialport;
        private PortConfig portconfig = new PortConfig();
        public ObservableCollection<string> ports_list = new ObservableCollection<string>();
        private bool FirstConfig = true;

        private SolidColorBrush colorwhite = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        private SolidColorBrush colorblue = new SolidColorBrush(Color.FromArgb(255, 56, 150, 212));
        private SolidColorBrush colorblack = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private SolidColorBrush colorred = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        public PortControl()
        {
            InitializeComponent();

            List<int> baudrate_collection = new List<int> { 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
            List<int> databits_collection = new List<int> { 5, 6, 7, 8 };
            List<string> stopbits_collection = new List<string> { "1", "1.5", "2" };
            string[] ParityItemSource = System.Enum.GetNames(typeof(Parity));
            string[] HandShakeItemSource = System.Enum.GetNames(typeof(Handshake));

            //set items source
            comboBox_BaudRate.ItemsSource = baudrate_collection;
            comboBox_Databits.ItemsSource = databits_collection;
            comboBox_StopBits.ItemsSource = stopbits_collection;
            comboBox_HandShake.ItemsSource = HandShakeItemSource;
            comboBox_Parity.ItemsSource = ParityItemSource;
        
            comboBox_Ports.ItemsSource = ports_list;
            serialport = new Serial(UpdateUI);


            //set default value
            comboBox_Databits.SelectedItem = 8;
            comboBox_Parity.SelectedItem = "None";
            comboBox_StopBits.SelectedItem = "1";
            comboBox_HandShake.SelectedItem = "None";
            comboBox_Parity.SelectedItem = "None";
            comboBox_BaudRate.SelectedItem = 115200;
        }

        private void UpdateUI(string data)
        {
            DisplayBox.AppendText(data);
            DisplayScroll.ScrollToEnd();
        }

        public ComboBox PortsComboBox
        {
            get{ return comboBox_Ports; }
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


        public bool SendNewLine
        {
            set { serialport.SendNewLineIsEnabled = value; }
        }


        public Button OpenButton
        {
            get { return Open_Button; }
            set { Open_Button = value; }
        }


        public AutoCompleteTextBox SendBox
        {
            get { return sendbox; }
        }

        private void sendbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var sendbox = (AutoCompleteTextBox)sender;
            if (e.Key == Key.Enter)
            {
                string SendText = sendbox.Text;
                sendbox.Text = string.Empty;
                sendbox.IsSuggestionOpen = false;
                sendbox.TextBoxFocus();

                switch (SendText)
                {
                    case "cls":
                        ClearDisplay();
                        return;
                        //case "clss":
                        //    ClearAllMenu1.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        //    return;
                        //case "cpld":
                        //    CPLD_Check.IsChecked = true;
                        //    CPLD_Check.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                        //    return;
                }
                SendData(SendText);
            }

            else if (e.Key == Key.Down)
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
            else if (e.Key == Key.Right)
            {
                sendbox.IsSuggestionOpen = false;
            }
        }

        private void Button_Open1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (serialport.IsOpening)
            {
                //sp.ReceivedBytesThreshold = 4096;
                serialport.DataReceived -= new SerialDataReceivedEventHandler(serialport.Data_Received);
                serialport.Closes();
                SetOpbuttonOpenState(button);
            }
            else
            {
                if (portconfig.portname == null)
                {
                    MessageBox.Show("Please set Port!");
                    return;
                }
                if (portconfig.baudrate == 0)
                {
                    MessageBox.Show("Please set BaudRate!");
                    return;
                }

                serialport.Config(portconfig);//config portname baudrate etc...
                serialport.ReceivedBytesThreshold = 1024;
                serialport.ReadTimeout = 300;
                serialport.WriteTimeout = 300;
                serialport.ReceivedBytesThreshold = 1;
                //sp.DtrEnable = true;
                //sp.RtsEnable = true;
                //sp.DataReceived += new SerialDataReceivedEventHandler(sp.Data_Received);

                try
                {
                    serialport.Opens();
                    SetOpbuttonCloseState(button);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


            }

        }



        public void SendData(string data)
        {
            serialport.Send_Data(data);
        }

        public void ClearDisplay()
        {
            Paragraph paraa = new Paragraph();
            DisplayBox.Document.Blocks.Clear();
            DisplayBox.Document.Blocks.Add(paraa);
        }

        public void SendBreak()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                serialport.Send_Break();
            }).Start();
            
        }

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

        public void Close_Port()
        {
            if (serialport.IsOpening)
                Open_Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }


        //Ports
        private void Ports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || serialport == null)
                return;

            portconfig.portname = cbox.SelectedItem.ToString();
            Close_Port();
            Open_Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        }

        //BaudRate
        private void comboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || serialport == null)
                return;

            portconfig.baudrate = (int)cbox.SelectedItem;
            Close_Port();

            if (!FirstConfig)
            {
                Open_Button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }

        }


        //DataBits
        private void comboBox_Databits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || serialport == null)
                return;


            portconfig.databits = (int)cbox.SelectedItem;
        }


        //StopBits
        private void comboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || serialport == null)
                return;

            portconfig.stopbits = getSerialStopBits(cbox);

        }

        //Parity
        private void comboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || serialport == null)
                return;

            portconfig.parity = getSerialParity(cbox);

        }


        //HandShake
        private void comboBox_HandShake_Selected(object sender, RoutedEventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            if (cbox.SelectedItem == null || serialport == null)
                return;


            portconfig.handshake = getSerialHandshake(cbox);

        }
        #endregion
    }
}
