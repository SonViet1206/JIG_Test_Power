using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;

namespace DefaultNS
{
    public partial class autonicsMT4 : UserControl
    {
        bool thread_ended = true;
        bool thread_started = false; 
        Thread threadCOMM;

        bool thread_busy = false;
        bool command_busy = false;

        string _port_name = "COM8";
        double _unit_factor = 1;
        SerialPort portCOMM = null;
        bool comm_state = false;

        public autonicsMT4()
        {
            InitializeComponent();
        }

        private void Control_Load(object sender, EventArgs e)
        {
            this.Font = new Font("Tahoma", 12);
        }

        public bool CommState { get { return comm_state; } }

        double _pvalue = 0;
        public double PValue { get { return _pvalue; } }

        public void Start(string port_name, string unit_label, double unit_factor)
        {
            _port_name = port_name;
            lblUnit.Text = unit_label;
            _unit_factor = unit_factor;

            lblCaption.Text = "MT4W METER - " + port_name;
            threadCOMM = new Thread(() => COMMProcess());
            threadCOMM.Start();
        }
        
        public void Start(string port_name)
        {
            Start(port_name, "VDC", 0.1);
        }

        public void Stop()
        {
            thread_started = false;
            while (!thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (!thread_started)
                Start(_port_name);
            else
                Stop();
        }

        void COMMProcess()
        {
            int rx_len;

            int p_value;
            
            byte[] CMD_ID1_READ_VALUE = null;

            string frame = "";

            //000000 - Tx:01 04 00 00 00 04 F1 C9
            //000001 - Rx:01 04 08 13 31 00 01 00 00 00 00 78 17


            frame += (char)0x01;               // Slave Address.
            frame += (char)0x04;               //FUN_05_WRITE_SINGLE_COIL;    // Function.
            frame += (char)0x00;               // H Coil Address.
            frame += (char)0x00;               // H Coil Address.
            frame += (char)0x00;               // L Coil Address.
            frame += (char)0x04;               // Number.
            frame += Conv.ModbusCRC(frame);

            CMD_ID1_READ_VALUE = Conv.stringToBytes(frame);
            
            portCOMM = new SerialPort(_port_name, 9600, Parity.None, 8, StopBits.One);
            try
            {
                thread_ended = false; 
                thread_started = true;
                
                while (thread_started)
                {
                    while (command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
                    p_value = 0;
                    try
                    {
                        if (!portCOMM.IsOpen)
                        {
                            try 
                            { 
                                portCOMM.Close();
                                portCOMM.Open();
                                //Dong tat ca ro le:
                                //portPLC.Write(":SETMOD:1\n");
                                Thread.Sleep(50);
                                portCOMM.ReadExisting();
                            } catch { }
                            this.Invoke((MethodInvoker)delegate
                            {
                                lblCaption.Text = (portCOMM.IsOpen ? "MT4W METER - " + _port_name + ":OK" : "MT4W METER - " + _port_name + ":NG");
                            });
                        }
                        else
                        {
                            thread_busy = true;
                            this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                            //Gửi lệnh đọc:
                            portCOMM.Write(CMD_ID1_READ_VALUE, 0, CMD_ID1_READ_VALUE.Length);
                            Thread.Sleep(150);
                            rx_len = portCOMM.BytesToRead;
                            if (rx_len >= 0)
                            {
                                byte[] rxBuff = new byte[rx_len];
                                portCOMM.Read(rxBuff, 0, rx_len);
                                //string sBuff = Conv.S2SH(Conv.bytesToString(rxBuff));
                                //    0  1  2  3 4  5 6      
                                //RX: 01 04 08 13 30 00 01 00 00 00 00 68 D7 
                                if (rxBuff[1] == 0x04 && rxBuff[2] == 0x08)
                                {
                                    p_value = rxBuff[3] * 0x100 + rxBuff[4];
                                }
                            }

                        }
                    }
                    catch 
                    {
                        
                    }
                    thread_busy = false;

                    _pvalue = (double)p_value * _unit_factor;
                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;
                        ledValue.Value = _pvalue.ToString();
                    });
                    //Dan cach lenh:
                    Thread.Sleep(100);
                }

            }
            catch (Exception)
            {


            }

            try 
            { 
                portCOMM.Close(); 
            } 
            catch { }
            portCOMM = null;

            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    btnOnOff.BackColor = Color.Black;
                    lblCaption.Text = "MT4W METER - " + _port_name + ":---";
                });
            }
            catch { }

            thread_ended = true;
        }

        
                
    }
}
