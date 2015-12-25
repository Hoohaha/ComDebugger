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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;

using System.IO.Ports;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;

namespace SerialOperations
{

    public class Serial : SerialPort
    {
        delegate void HanderInterfaceUpdataDelegate(string data);
        private int instance;

        public Serial(Action<string> DisplayData)
        {

            sp.PortName = portname;
            sp.BaudRate = baudrate;
            sp.DataBits = databits;
            sp.Parity = parity;
            sp.StopBits = stopbits;
            sp.Handshake = handshake;

            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;
            sp.ReceivedBytesThreshold = 1;
            sp.DtrEnable = true;
            sp.RtsEnable = true;
            HanderInterfaceUpdataDelegate DisplayHandle = new HanderInterfaceUpdataDelegate(DisplayData);
            sp.DataReceived += new SerialDataReceivedEventHandler(this.SP_DataReceived);
        }



        private void SP_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //IsReceving = true;
            SerialPort sp = (SerialPort)sender;

            if (sp.IsOpen)
            {
                int n = sp.BytesToRead;
                if (n < 1)
                {
                    IsReceving = false;
                    return;
                }
                byte[] buf = new byte[n];
                sp.Read(buf, 0, n);

                //IsReceving = false;
                string abc = System.Text.Encoding.Default.GetString(buf);
                Dispatcher.BeginInvoke(DisplayHandle, new string[] { abc });
            }

        }

        public void Send_Data(SerialPort sp, string strSend)
        {
            if (sp.IsOpen)
            {
                try
                {
                    sp.Write(strSend);
                    Dispatcher.Invoke(DisplayHandle, new string[] { strSend });
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

            }
            else
            {
                MessageBox.Show("Serial is not open");
            }
        }
    }
}