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
    public partial class PowerBoard : UserControl
    {
        bool thread_ended = true;
        bool thread_started = false; 
        Thread threadCOMM;

        bool thread_busy = false;
        bool command_busy = false;

        string _port_name = "COM8";
        SerialPort portCOMM = null;
        bool comm_state = false;

        public PowerBoard()
        {
            InitializeComponent();
        }

        private void Control_Load(object sender, EventArgs e)
        {
            //this.Width = pictureBox3.Width;
            //this.Height = pictureBox3.Height;
            this.Font = new Font("Tahoma", 12);
        }

        public bool CommState { get { return comm_state; } }

        int _states = 0;
        public int States { get { return _states; } }

        public void Start(string port_name)
        {
            _port_name = port_name;
            lblCaption.Text = "POWER BOARD - " + port_name;
            try
            {
                portCOMM = new SerialPort(_port_name, 9600, Parity.None, 8, StopBits.One);
                portCOMM.Open();
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("Lỗi kết nối với POWERBOARD" + "\r\n" + ex.ToString());
                MessageBox.Show(ex.Message, "Lỗi kết nối với POWERBOARD");
            }
            finally
            {
                portCOMM.Close();
            }

            threadCOMM = new Thread(() => COMMProcess());
            threadCOMM.Start();
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
            bool commOK = false;
            int new_states1;

            try
            {
                thread_ended = false; 
                thread_started = true;
                
                while (thread_started)
                {
                    while (command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
                    new_states1 = 0;
                    commOK = false;
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
                                portCOMM.DiscardInBuffer();
                            } catch { }
                            this.Invoke((MethodInvoker)delegate
                            {
                                lblCaption.Text = (portCOMM.IsOpen ? "POWER BOARD - " + _port_name + ":OK" : "POWER BOARD - " + _port_name + ":NG");
                            });
                        }
                        else
                        {
                            thread_busy = true;
                            this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                            //Gửi lệnh đọc:
                            portCOMM.Write("AT+READALL\r\n");
                            Thread.Sleep(100);
                            string rx_buff = portCOMM.ReadExisting();
                            rx_buff = rx_buff.Replace("\r\nOK\r\n", "");
                            rx_buff = rx_buff.Replace("\r\n", "");

                            if (rx_buff.Length == 12)
                            {
                                for (int i = rx_buff.Length-1; i >= 0; i--)
                                {
                                    new_states1 <<= 1;
                                    if (rx_buff[i] == '0') new_states1 += 1;
                                }
                                commOK = true;
                            }
                            //commOK = true; //DEBUG
                        }
                    }
                    catch 
                    {
                        
                    }
                    thread_busy = false;

                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;

                        btnINP1.BackColor = ((new_states1 & 0x01) != 0) ? Color.Red : Color.DimGray;
                        btnINP2.BackColor = ((new_states1 & 0x02) != 0) ? Color.Red : Color.DimGray;
                        btnINP3.BackColor = ((new_states1 & 0x04) != 0) ? Color.Red : Color.DimGray;
                        btnINP4.BackColor = ((new_states1 & 0x08) != 0) ? Color.Red : Color.DimGray;
                        btnINP5.BackColor = ((new_states1 & 0x10) != 0) ? Color.Red : Color.DimGray;
                        btnINP6.BackColor = ((new_states1 & 0x20) != 0) ? Color.Red : Color.DimGray;
                        btnINP7.BackColor = ((new_states1 & 0x40) != 0) ? Color.Red : Color.DimGray;
                        btnINP8.BackColor = ((new_states1 & 0x80) != 0) ? Color.Red : Color.DimGray;

                        btnOUT1.BackColor = ((new_states1 & 0x100) != 0) ? Color.Red : Color.DimGray;
                        btnOUT2.BackColor = ((new_states1 & 0x200) != 0) ? Color.Red : Color.DimGray;
                        btnOUT3.BackColor = ((new_states1 & 0x400) != 0) ? Color.Red : Color.DimGray;
                        btnOUT4.BackColor = ((new_states1 & 0x800) != 0) ? Color.Red : Color.DimGray;
                    });
                    _states = new_states1;
                    comm_state = commOK;
                    if (commOK)
                        Thread.Sleep(100);
                    else
                        Thread.Sleep(3000);


                }

            }
            catch (Exception)
            {


            }

            try 
            {
                if(portCOMM != null)    
                portCOMM.Close(); 
            } 
            catch { }
            portCOMM = null;

            try
            {
                this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Black; });
            }
            catch { }

            thread_ended = true;
        }

        private byte[] ExecuteCommand(string cmd_str, int timeout)
        {
            byte[] retBytes = null;
            while (thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
            command_busy = true;
            try
            {
                if (portCOMM != null && portCOMM.IsOpen)
                {
                    string res = "";
                    portCOMM.Write(cmd_str);
                    Conv.delay_ms(timeout);
                    res = portCOMM.ReadExisting().Replace("\r\n", string.Empty);
                    if (res == "OK")
                    {
                        retBytes = Conv.stringToBytes(res);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            command_busy = false;

            return retBytes;
        }

        public bool SetOutput(int index, bool state)
        {
            return SetOutput(index, state, 0);
        }

        public bool SetOutput(int index, bool state, int ms_delay)
        {
            bool retValue = false;
            string frame = "";
            if (index == 1)
            {
                frame = "AT+SETRED=" + (state ? "1" : "0") + "\r\n";
            }
            else if (index == 2)
            {
                frame = "AT+SETYEL=" + (state ? "1" : "0") + "\r\n";
            }
            else if (index == 3)
            {
                frame = "AT+SETGRE=" + (state ? "1" : "0") + "\r\n";
            }
            else if (index == 4)
            {
                frame = "AT+SETPOW=" + (state ? "1" : "0") + "\r\n";
            }

            if (frame != "")
            {
                retValue = (ExecuteCommand(frame, 100) != null);
                if (ms_delay > 0)
                {
                    Conv.delay_ms(ms_delay);
                }
            }
            return retValue;
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            if (chkTestMode.Checked)
            {
                if (sender == btnOUT1)
                    SetOutput(1, btnOUT1.BackColor == Color.DimGray);
                else if (sender == btnOUT2)
                    SetOutput(2, btnOUT2.BackColor == Color.DimGray);
                else if (sender == btnOUT3)
                    SetOutput(3, btnOUT3.BackColor == Color.DimGray);
                else if (sender == btnOUT4)
                    SetOutput(4, btnOUT4.BackColor == Color.DimGray);
            }
        }

        private void chkTestMode_CheckedChanged(object sender, EventArgs e)
        {
            btnOUT1.Enabled = chkTestMode.Checked;
            btnOUT2.Enabled = chkTestMode.Checked;
            btnOUT3.Enabled = chkTestMode.Checked;
            btnOUT4.Enabled = chkTestMode.Checked;
        }

        
    }
}
