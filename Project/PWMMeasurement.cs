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
    public partial class PWMMeasurement : UserControl
    {
        bool thread_ended = true;
        bool thread_started = false;
        Thread threadCOMM;

        bool thread_busy = false;
        bool command_busy = false;

        string _port_name = "COM1";
        SerialPort portCOMM = null;
        bool comm_state = false;
        public PWMMeasurement()
        {
            InitializeComponent();
        }


        private double frequency;
        private double duty;
        private double peakVotage;
        private double peakVotageMin;

        public bool CommState { get { return comm_state; } }

        public double Frequency { get { return frequency; } }
        public double Duty { get { return duty; } }
        public double PeakVotage { get { return peakVotage; } }

        public double PeakVotageMin { get { return peakVotageMin; } }

        public void Start(string port_name)
        {
            _port_name = port_name;
            lblCaption.Text = "MEASURING BOARD-" + port_name;
            try
            {
                portCOMM = new SerialPort(_port_name, 9600, Parity.None, 8, StopBits.One);
                portCOMM.Open();
            }
            catch (Exception ex)
            {
                //ManageLog.WriteEvent("Lỗi kết nối với MEASURING BOARD" + "\r\n" + ex.ToString());
                MessageBox.Show(ex.Message, "Lỗi kết nối với MEASURING BOARD");
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
                                    lblCaption.Text = (portCOMM.IsOpen ? "MEASURING BOARD - " + _port_name + ":OK" : "MEASURING BOARD - " + _port_name + ":NG");
                                });
                            }
                            else
                            {
                                thread_busy = true;
                                this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                                //portCOMM.ReadExisting();
                                //Gửi lệnh đọc:
                                portCOMM.Write("AT+READALL\r\n");
                                Thread.Sleep(150);
                                if (portCOMM.BytesToRead > 0)
                                {
                                    string rx_buff = portCOMM.ReadExisting();
                                    rx_buff = rx_buff.Replace("\r\nOK\r\n", "");
                                    rx_buff = rx_buff.Replace("\r\n", "");
                                    rx_buff = rx_buff.Replace("OK", "");
                                    string[] arr_rx = rx_buff.Split(';');
                                    //rx_buff = rx_buff.Replace("\r\n", "");

                                    if (arr_rx.Length == 3)
                                    {
                                        frequency = Conv.atod(arr_rx[0].Replace("\n", ""));
                                        duty = Conv.atod(arr_rx[1].Replace("\n", ""));
                                        peakVotage = Conv.atod(arr_rx[2].Replace("\n", ""));
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            label4.Text = frequency.ToString("0.0");
                                            label5.Text = duty.ToString("0.0");
                                            label6.Text = peakVotage.ToString("0.00");
                                        });
                                        commOK = true;

                                    }
                                    else
                                    {
                                        Debug.Write("Frame error");
                                    }

                                    if (arr_rx.Length == 4)
                                    {
                                        frequency = Conv.atod(arr_rx[0].Replace("\n", ""));
                                        duty = Conv.atod(arr_rx[1].Replace("\n", ""));
                                        peakVotage = Conv.atod(arr_rx[2].Replace("\n", ""));
                                        peakVotageMin = Conv.atod(arr_rx[3].Replace("\n", ""));
                                        if (peakVotageMin == 0)
                                            Debug.Write("Frame error");
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            label4.Text = frequency.ToString();
                                            label5.Text = duty.ToString();
                                            label6.Text = peakVotage.ToString();
                                            label12.Text = peakVotageMin.ToString();
                                        });
                                        commOK = true;

                                    }
                                    else
                                    {
                                        Debug.Write("Frame error");
                                    }
                                }
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
    }
}
