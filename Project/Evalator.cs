using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DefaultNS
{
    public partial class Evalator : UserControl
    {
        bool thread_ended = true;
        bool thread_started = false;
        Thread threadCOMM;

        bool thread_busy = false;
        bool command_busy = false;

        string _port_name = "COM1";
        SerialPort portCOMM = null;
        bool comm_state = false;
        public Evalator()
        {
            InitializeComponent();
        }


        public bool CommState { get { return comm_state; } }

        public void Start(string port_name)
        {
            _port_name = port_name;
            lblCaption.Text = "EVALATOR BOARD-" + port_name;
            try
            {
                portCOMM = new SerialPort(_port_name, 115200, Parity.None, 8, StopBits.One);
                portCOMM.Open();
            }
            catch (Exception ex)
            {
                //ManageLog.WriteEvent("Lỗi kết nối với MEASURING BOARD" + "\r\n" + ex.ToString());
                MessageBox.Show(ex.Message, "Lỗi kết nối với EVALATOR BOARD");
            }
            finally
            {
                if (portCOMM != null)
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
                        if(portCOMM!= null)
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
                                }
                                catch { }
                                this.Invoke((MethodInvoker)delegate
                                {
                                    lblCaption.Text = (portCOMM.IsOpen ? "EVALATOR BOARD - " + _port_name + ":OK" : "EVALATOR BOARD - " + _port_name + ":NG");
                                });
                            }
                            else
                            {
                                thread_busy = true;
                                this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                                //portCOMM.ReadExisting();
                                //Gửi lệnh đọc:
                                portCOMM.Write("AT\r\n");
                                Thread.Sleep(50);
                                string rx_buff = portCOMM.ReadExisting();
                                //rx_buff = rx_buff.Replace("\r\nOK\r\n", "");
                                //rx_buff = rx_buff.Replace("\r\n", "");
                                //rx_buff = rx_buff.Replace("OK", "");
                                //string[] arr_rx = rx_buff.Split(';');
                                //rx_buff = rx_buff.Replace("\r\n", "");

                                
                            }
                        }
                        
                    }
                    catch
                    {

                    }
                    thread_busy = false;

                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;

                        
                    });
                    
                    comm_state = commOK;
                    if (commOK)
                        Thread.Sleep(200);
                    else
                        Thread.Sleep(1000);


                }

            }
            catch (Exception)
            {


            }

            try
            {
                if(portCOMM!=null)
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

        private string ExecuteATCommand(string cmd_str, int timeout)
        {
            string retValue = "";
            while (thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
            command_busy = true;
            try
            {
                if (portCOMM != null && portCOMM.IsOpen)
                {
                    portCOMM.Write(cmd_str);
                    Conv.delay_ms(timeout);
                    retValue = portCOMM.ReadExisting();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            command_busy = false;

            return retValue;
        }

        public bool SetMode(string mode, int ms_delay)
        {
            bool retOK = false;
            string res;
            btnModeE.BackColor = btnModeA.BackColor = btnModeB.BackColor = btnModeD.BackColor = Color.Gainsboro;
            res = ExecuteATCommand("AT+RELAYMODE=" + mode + "\r", 100);
            //"\r\n+RELAYMODE: E,1\r\n\r\nOK\r\n"
            if (res.IndexOf("\r\nOK\r\n") > 0)
            {
                retOK = true;
                if (mode == "E")
                    btnModeE.BackColor = Color.Lime;
                else if (mode == "A")
                    btnModeA.BackColor = Color.Lime;
                else if (mode == "B")
                    btnModeB.BackColor = Color.Lime;
                else if (mode == "D")
                    btnModeD.BackColor = Color.Lime;
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return retOK;
        }

        public bool SetMode(string mode)
        {
            return SetMode(mode, 0);
        }

        private void btnSetMode_Click(object sender, EventArgs e)
        {
            if ((Button)sender == btnModeE)
                SetMode("E");
            else if ((Button)sender == btnModeA)
                SetMode("A");
            else if ((Button)sender == btnModeB)
                SetMode("B");
            else if ((Button)sender == btnModeD)
                SetMode("D");
        }

        public int GetSLAC()
        {
            int slac = -1;
            string res;
            label4.Text = "";
            res = ExecuteATCommand("AT+SLAC?\r", 8000);
            //"\r\n+RELAYMODE: E,1\r\n\r\nOK\r\n"
            if (res.IndexOf("\r\nOK\r\n") > 0)
            {
                
            }
            else
                label4.Text = "---";
            return slac;
        }

        public int SentSLAC()
        {
            int slac = -1;
            string res;
            label4.Text = "";
            res = ExecuteATCommand("AT+SLAC?\r",100);
            //"\r\n+RELAYMODE: E,1\r\n\r\nOK\r\n"
            return slac;
        }

        public bool CheckEthernetStatus(int ms_delay)
        {
            bool retOK = false;
            string res;
            this.Invoke((MethodInvoker)delegate { richTextBox1.Text += "\r\n" + "[TX]-" + "AT+ETHERNET?\r"; });
            res = ExecuteATCommand("AT+ETHERNET?\r", 1000);
            this.Invoke((MethodInvoker)delegate { richTextBox1.Text += "\r\n" + "[RX]-" + res; });
            //"\r\n+ETHERNET: 1\r\n\r\nOK\r\n"
            //{0A}+ETHERNET: 1{0D}{0A}{0D}{0A}OK{0D}{0A}
            if (res.IndexOf("\r\nOK\r\n") > 0)
            {
                res = res.Replace("\r\nOK\r\n","");
                res = res.Replace("\r\n", "");
                string[] resarr = res.Split(':');
                if (resarr.Length >= 2)
                {
                    retOK = resarr[1].Trim() == "1";
                }
            }

            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return retOK;
        }



        private void label1_DoubleClick(object sender, EventArgs e)
        {
            GetSLAC();
        }

        private void btnCheckEthStatus_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { btnCheckEthStatus.BackColor = Color.Gainsboro; });
            if(CheckEthernetStatus(0)) this.Invoke((MethodInvoker)delegate { btnCheckEthStatus.BackColor = Color.Lime; });
        }
    }
}
