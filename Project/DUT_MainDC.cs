using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO.Ports;
using ECAN;

namespace DefaultNS
{
    public partial class DUT_MainDC : UserControl
    {
        const byte CAN_DUT_ADDRESS = 0x04;
        const byte CAN_SOFT_ADDRESS = 0x0A;

        const byte CAN_CMDID_START_TEST = 0x60;
        const byte CAN_CMDID_STOP_TEST = 0x61;
        const byte CAN_CMDID_REQUEST = 0x62;
        const byte CAN_CMDID_RESPONSE = 0x63;

        bool thread_busy = false;
        bool command_busy = false;
        bool comm_started = false;

        bool thread_started = false;
        bool thread_ended = true;

        bool tcp_thread_ended = true;
        bool tcp_thread_started = false;
        Thread threadSTM;
        Thread threadCOMM;
        Thread threadCAN;

        bool tcp_thread_busy = false;
        bool tcp_command_busy = false;
        bool tcp_comm_started = false;

        string _IP_address = "192.168.1.1";
        //SerialPort portCOMM = null;

        string _port_stm = "COM7";
        string _port_name = "COM8";
        double _unit_factor = 1;
        SerialPort portCOMM = null;
        SerialPort commSTM = null;

        string can_serial = "---";

        TcpClient client = null;
        
        public DUT_MainDC()
        {
            InitializeComponent();
        }

        private void Control_Load(object sender, EventArgs e)
        {
            this.Font = new Font("Tahoma", 12);
        }

        public bool CommState { get { return tcp_comm_started; } }

        double _pvalue = 0;
        public double PValue { get { return _pvalue; } }

        public void Start(string port_name, string port_name2)
        {
            _port_name = port_name;
            _port_stm = port_name2;
            lblCaption.Text = "DEVICE UNDER TEST";

            threadSTM = new Thread(() => STM_COMMProcess());
            threadSTM.Start();
            
            threadCOMM = new Thread(() => COMMProcess());
            threadCOMM.Start();

            threadCAN = new Thread(() => CANProcess());
            threadCAN.Start();
        }

        public void Stop()
        {
            tcp_thread_started = false;
            thread_started = false;
            while (!tcp_thread_ended || !thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (!tcp_thread_started && !thread_started)
                Start(_port_name, _port_stm);
            else
                Stop();
        }

        //private byte[] ExecuteCommand(string cmd_hex, int timeout)
        //{
        //    byte[] retBytes = null;
        //    while (tcp_thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
        //    tcp_command_busy = true;
        //    try
        //    {
        //        if (tcp_comm_started) //Kết nối với DUT tốt
        //        {

        //            this.Invoke((MethodInvoker)delegate
        //            {
        //                //if (txtCAN_LOG.Text.Length > 100000) txtCAN_LOG.Text = txtCAN_LOG.Text.Substring(txtCAN_LOG.Text.Length - 100000);

        //            });

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //    tcp_command_busy = false;

        //    return retBytes;
        //}

        private bool ExecuteCANCommand(string cmd_hex)
        {
            return (ExecuteCANCommand(cmd_hex, 0) != null);
        }

        private byte[] ExecuteCANCommand(byte[] cmd_data, int timeout)
        {
            string strCmd = "";

            strCmd += CAN_DUT_ADDRESS.ToString("X2");
            strCmd += cmd_data.Length.ToString("X2");
            for (int i = 0; i < cmd_data.Length; i++)
            {
                strCmd += cmd_data[i].ToString("X2");
            }

            return ExecuteCANCommand(strCmd, timeout);
        }

        private byte[] ExecuteCANCommand(string cmd_hex, int timeout)
        {
            byte[] retBytes = null;
            while (thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
            command_busy = true;
            try
            {
                if (comm_started)
                {
                    byte[] bytes = Conv.stringToBytes(Conv.SH2S(cmd_hex));
                    if (bytes.Length > 0) //Frame hợp lệ
                    {
                        uint mLen = 1;

                        CAN_OBJ txFrame = new CAN_OBJ();
                        CAN_OBJ rxFrame = new CAN_OBJ();

                        txFrame.SendType = 0;
                        txFrame.data = new byte[8];
                        txFrame.Reserved = new byte[3];
                        txFrame.ID = CAN_SOFT_ADDRESS; //Địa chỉ của PC
                        txFrame.ExternFlag = 0;
                        txFrame.RemoteFlag = 0;

                        txFrame.DataLen = (bytes.Length > 8) ? (byte)8 : (byte)bytes.Length;
                        for (int i = 0; i < txFrame.DataLen; i++)
                        {
                            txFrame.data[i] = bytes[i];
                        }

                        this.Invoke((MethodInvoker)delegate
                        {
                            txtCAN_LOG.AppendText("\r\n[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][TX][" + txFrame.DataLen.ToString() + "][" + cmd_hex.Replace(" ", "") + "][" + txFrame.ExternFlag.ToString() + "][" + txFrame.RemoteFlag.ToString() + "]");
                        });
                        //Conv.delay_ms(10);
                        if (ECANDLL.Transmit(1, 0, 0, ref txFrame, (ushort)mLen) == ECANStatus.STATUS_OK)
                        {
                            int ms_timeout = timeout; //Frame timeout
                            if (ms_timeout > 0)
                            {
                                Stopwatch stopWatch = new Stopwatch();
                                stopWatch.Start();
                                while (stopWatch.ElapsedMilliseconds < ms_timeout) // && string.IsNullOrEmpty(buffReceiver))
                                {
                                    Application.DoEvents();
                                    Thread.Sleep(1);
                                    mLen = 1;
                                    if (ECANDLL.Receive(1, 0, 0, out rxFrame, mLen, 1) == ECANStatus.STATUS_OK && mLen > 0)
                                    {
                                        string sRXH = "";
                                        for (int i = 0; i < rxFrame.DataLen; i++) sRXH += string.Format("{0:X2}", rxFrame.data[i]) + " ";

                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            txtCAN_LOG.AppendText("\r\n[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][RX][" + rxFrame.DataLen.ToString() + "][" + sRXH.Replace(" ", "") + "][" + rxFrame.ExternFlag.ToString() + "][" + rxFrame.RemoteFlag.ToString() + "]");
                                        });

                                        //this.gRecMsgBuf[this.gRecMsgBufHead].ID = mMsg.ID;
                                        //this.gRecMsgBuf[this.gRecMsgBufHead].DataLen = mMsg.DataLen;
                                        //this.gRecMsgBuf[this.gRecMsgBufHead].data = mMsg.data;
                                        //this.gRecMsgBuf[this.gRecMsgBufHead].ExternFlag = mMsg.ExternFlag;
                                        //this.gRecMsgBuf[this.gRecMsgBufHead].RemoteFlag = mMsg.RemoteFlag;
                                        if (rxFrame.ID == CAN_DUT_ADDRESS && rxFrame.data[0] == CAN_SOFT_ADDRESS && rxFrame.DataLen > 3)
                                        {
                                            if (rxFrame.DataLen == 8 && rxFrame.data[1] == 0x10) //Multi Frame
                                            {
                                                int data_len = rxFrame.data[2] - 1;
                                                string rxData = "";
                                                rxData += (char)rxFrame.data[4];
                                                rxData += (char)rxFrame.data[5];
                                                rxData += (char)rxFrame.data[6];
                                                rxData += (char)rxFrame.data[7];

                                                //TX: 00 03 62 03 00
                                                //RX: 0A 10 22 63 04 00 E6 00
                                                //TX: 00 30 00 00
                                                //RX: 0A 21 00 00 00 00 00 00
                                                //RX: 0A 22 E6 00 00 00 00 00
                                                //RX: 0A 23 00 00 E6 00 00 00
                                                //RX: 0A 25 00 00 00 00 E6 00
                                                //RX: 0A 25 00 00 00 00 00

                                                txFrame.DataLen = 4;
                                                txFrame.data[0] = CAN_DUT_ADDRESS;
                                                txFrame.data[1] = 0x30;
                                                txFrame.data[2] = 0x00;
                                                txFrame.data[3] = 0x00;

                                                this.Invoke((MethodInvoker)delegate
                                                {
                                                    txtCAN_LOG.AppendText("\r\n[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][TX][" + txFrame.DataLen.ToString() + "][00300000][" + txFrame.ExternFlag.ToString() + "][" + txFrame.RemoteFlag.ToString() + "]");
                                                });
                                                //Conv.delay_ms(10);
                                                if (ECANDLL.Transmit(1, 0, 0, ref txFrame, (ushort)mLen) == ECANStatus.STATUS_OK)
                                                {
                                                    stopWatch.Restart();
                                                    while (stopWatch.ElapsedMilliseconds < ms_timeout) // && string.IsNullOrEmpty(buffReceiver))
                                                    {
                                                        Application.DoEvents();
                                                        Thread.Sleep(1);
                                                        mLen = 1;
                                                        if (ECANDLL.Receive(1, 0, 0, out rxFrame, mLen, 1) == ECANStatus.STATUS_OK && mLen > 0)
                                                        {
                                                            sRXH = "";
                                                            for (int i = 0; i < rxFrame.DataLen; i++) sRXH += string.Format("{0:X2}", rxFrame.data[i]) + " ";

                                                            this.Invoke((MethodInvoker)delegate
                                                            {
                                                                txtCAN_LOG.AppendText("\r\n[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][RX][" + rxFrame.DataLen.ToString() + "][" + sRXH.Replace(" ", "") + "][" + rxFrame.ExternFlag.ToString() + "][" + rxFrame.RemoteFlag.ToString() + "]");
                                                            });

                                                            for (int i = 0; i < rxFrame.DataLen - 2; i++) rxData += (char)rxFrame.data[i + 2];
                                                        }
                                                    }
                                                }
                                                //Du lieu tra ve sau khi da ghep
                                                retBytes = Conv.stringToBytes(rxData);
                                            }
                                            else if (rxFrame.data[2] == CAN_CMDID_RESPONSE) //Single Frame
                                            {
                                                retBytes = new byte[rxFrame.DataLen - 3];
                                                for (int i = 0; i < retBytes.Length; i++) retBytes[i] = rxFrame.data[i + 3];

                                                break;
                                            }
                                            //ms_timeout = 0;

                                            //ms_timeout = 20;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                retBytes = new byte[txFrame.DataLen];
                                for (int i = 0; i < txFrame.DataLen; i++) retBytes[i] = txFrame.data[i];
                            }
                        }
                        this.Invoke((MethodInvoker)delegate
                        {
                            //if (txtLog.Text.Length > 100000) txtLog.Text = txtLog.Text.Substring(txtLog.Text.Length - 100000);
                            txtCAN_LOG.SelectionStart = txtCAN_LOG.Text.Length;
                        });
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

        void CANProcess()
        {
            bool commOK = false;
            CAN_ERR_INFO mErrInfo = new CAN_ERR_INFO();
            INIT_CONFIG init_config = new INIT_CONFIG();

            init_config.AccCode = 0;
            init_config.AccMask = 0xffffff;
            init_config.Filter = 0;
            //init_config.

            init_config.Timing0 = 0x01; //250kbps
            init_config.Timing1 = 0x1c; //250kbps

            //init_config.Timing0 = 0;    //500kbps
            //init_config.Timing1 = 0x1c; //500kbps

            init_config.Mode = 0;

            try
            {
                thread_ended = false;
                thread_started = true;
                while (thread_started)
                {
                    try
                    {
                        commOK = false;
                        this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                        if (!comm_started)
                        {
                            //try { ECANDLL.CloseDevice(1, 0); } catch {}
                            Thread.Sleep(200);
                            if (ECANDLL.OpenDevice(1, 0, 0) == ECAN.ECANStatus.STATUS_OK)
                            {
                                //Set can1 baud
                                if (ECANDLL.InitCAN(1, 0, 0, ref init_config) == ECAN.ECANStatus.STATUS_OK)
                                {
                                    Thread.Sleep(100);
                                    if (ECANDLL.StartCAN(1, 0, 0) == ECAN.ECANStatus.STATUS_OK)
                                    {
                                        BOARD_INFO mReadBoardInfo = new BOARD_INFO();
                                        //Thread.Sleep(200);
                                        //ECANDLL.ResetCAN(1, 0, 0);
                                        Thread.Sleep(100);
                                        if (ECANDLL.ReadBoardInfo(1, 0, out mReadBoardInfo) == ECANStatus.STATUS_OK)
                                        {
                                            this.Invoke((MethodInvoker)delegate { lblCANSerial.Text = Conv.bytesToString(mReadBoardInfo.str_Serial_Num); });
                                            commOK = true;
                                        }
                                        //else can_serial = "---";
                                        //Đọc các thông tin bo mạch:
                                        //StartTest();
                                        //ReadSerialNumber();
                                        //ReadSoftwareVersion();
                                        //ReadHardwareVersion();
                                        //SetAllOutput(false);

                                        //ReadAllInput();
                                        //ReadTemprature();
                                        //ReadADCValue();
                                        //FlashTest();
                                    }
                                }
                            }

                        }
                        else
                        {
                            while (command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
                            thread_busy = true;
                            try
                            {
                                CAN_OBJ mMsg = new CAN_OBJ();
                                uint mLen;
                                int sCount = 0;

                                //Đọc các bản tin tự do:
                                do
                                {
                                    mLen = 1;
                                    if (ECANDLL.Receive(1, 0, 0, out mMsg, mLen, 1) == ECANStatus.STATUS_OK && mLen > 0)
                                    {
                                        byte[] rxData = new byte[mMsg.DataLen];
                                        for (int i = 0; i < mMsg.DataLen; i++) rxData[i] = mMsg.data[i];
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            //Ghi LOG:
                                            txtCAN_LOG.AppendText("\r\n[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][DX][" + mMsg.DataLen.ToString() + "][" + Conv.S2SH(Conv.bytesToString(rxData)).Replace(" ", "") + "][" + mMsg.ExternFlag.ToString() + "][" + mMsg.RemoteFlag.ToString() + "]");
                                            txtCAN_LOG.SelectionStart = txtCAN_LOG.Text.Length;
                                            //Xu ly cac frame su kien: 
                                            if (mMsg.ID == CAN_DUT_ADDRESS && rxData[0] == CAN_SOFT_ADDRESS && mMsg.DataLen > 3 && rxData[2] == CAN_CMDID_RESPONSE)
                                            {
                                                //ThreadFrameProcess(rxData);
                                            }
                                        });
                                    }
                                    else
                                        break;
                                }
                                while (++sCount < 100); //Xu ly toi da 100 frame
                                //Đọc bản tin báo lỗi:
                                if (ECANDLL.ReadErrInfo(1, 0, 0, out mErrInfo) == ECANStatus.STATUS_OK)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        txtErr1.Text = string.Format("{0:X4}", mErrInfo.ErrCode) + string.Format("{0:X4}", mErrInfo.Passive_ErrData[1]) + string.Format("{0:X4}", mErrInfo.Passive_ErrData[2]);
                                    });
                                    commOK = true;
                                    //Nếu không có dữ liệu thì dãn cách lệnh:
                                    if (sCount == 0) Thread.Sleep(100);
                                }
                                //Đọc giá trị DIs:

                            }
                            catch { }
                            thread_busy = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }

                    comm_started = commOK;
                    //Cap nhat hien thi:
                    this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Black; });
                    //Di ngu:
                    if (comm_started)
                        Thread.Sleep(100);
                    else
                        Thread.Sleep(3000);
                }

            }
            catch (Exception)
            {


            }

            try { ECANDLL.CloseDevice(1, 0); } catch { }
            comm_started = false;
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    btnOnOff.BackColor = Color.Black;
                    lblCANSerial.Text = "---";
                    //ledVOLT.Value = "-----";
                    //ledAMPE.Value = "-----";
                    //ledKWH.Value = "-------";
                });
            }
            catch { }

            thread_ended = true;
        }


        void COMMProcess()
        {
            string rx_buffer = "";
            string frame = "";
            int rx_len;


            portCOMM = new SerialPort(_port_name, 115200, Parity.None, 8, StopBits.One);
            try
            {
                tcp_thread_ended = false;
                tcp_thread_started = true;

                while (tcp_thread_started)
                {
                    while (tcp_command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
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
                            }
                            catch { }
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (portCOMM.IsOpen) txtTCP_LOG.Text = "";
                                lblIMXStatus.Text = (portCOMM.IsOpen ? "IMX DEBUG - " + _port_name + ":OK" : "IMX DEBUG - " + _port_name + ":NG");
                            });
                        }
                        else
                        {
                            tcp_thread_busy = true;
                            this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                            //Gửi lệnh đọc:
                            //portCOMM.Write(CMD_ID1_READ_VALUE, 0, CMD_ID1_READ_VALUE.Length);
                            Thread.Sleep(150);
                            rx_len = portCOMM.BytesToRead;
                            if (rx_len > 0)
                            {
                                byte[] rxBuff = new byte[rx_len];
                                portCOMM.Read(rxBuff, 0, rx_len);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    txtTCP_LOG.AppendText(Conv.bytesToString(rxBuff));
                                });
                                //rx_buffer += Conv.bytesToString(rxBuff);
                                //int pos = rx_buffer.IndexOf()

                            }

                        }
                    }
                    catch
                    {

                    }
                    tcp_thread_busy = false;

                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;
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
                    lblIMXStatus.Text = "IMX DEBUG - " + _port_name + ":---";
                });
            }
            catch { }

            tcp_thread_ended = true;
        }

        void STM_COMMProcess()
        {
            string rx_buffer = "";
            string frame = "";
            int rx_len;

            
            commSTM = new SerialPort(_port_stm, 921600, Parity.None, 8, StopBits.One);
            try
            {
                tcp_thread_ended = false;
                tcp_thread_started = true;

                while (tcp_thread_started)
                {
                    while (tcp_command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
                    try
                    {
                        if (!commSTM.IsOpen)
                        {
                            try
                            {
                                commSTM.Close();
                                commSTM.Open();
                                //Dong tat ca ro le:
                                //portPLC.Write(":SETMOD:1\n");
                                Thread.Sleep(50);
                                commSTM.ReadExisting();
                            }
                            catch { }
                            this.Invoke((MethodInvoker)delegate
                            {
                                if(commSTM.IsOpen) txtSTM_LOG.Text = "";
                                lblSTMStatus.Text = (commSTM.IsOpen ? "STM DEBUG - " + _port_stm + ":OK" : "STM DEBUG - " + _port_stm + ":NG");
                            });
                        }
                        else
                        {
                            tcp_thread_busy = true;
                            this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                            //Gửi lệnh đọc:
                            //portCOMM.Write(CMD_ID1_READ_VALUE, 0, CMD_ID1_READ_VALUE.Length);
                            Thread.Sleep(150);
                            rx_len = commSTM.BytesToRead;
                            if (rx_len > 0)
                            {
                                byte[] rxBuff = new byte[rx_len];
                                commSTM.Read(rxBuff, 0, rx_len);
                                this.Invoke((MethodInvoker)delegate 
                                {
                                    txtSTM_LOG.AppendText(Conv.bytesToString(rxBuff)); 
                                });
                                //rx_buffer += Conv.bytesToString(rxBuff);
                                //int pos = rx_buffer.IndexOf()

                            }

                        }
                    }
                    catch
                    {

                    }
                    tcp_thread_busy = false;

                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;
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
                commSTM.Close();
            }
            catch { }
            commSTM = null;

            try
            {
                this.Invoke((MethodInvoker)delegate 
                { 
                    btnOnOff.BackColor = Color.Black;
                    lblSTMStatus.Text = "STM DEBUG - " + _port_stm + ":---";
                });
            }
            catch { }

            tcp_thread_ended = true;
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            Ping p1 = new Ping();
            PingReply PR;
            btnScan.Enabled = btnPing.Enabled = false;
            PR = p1.Send(txtIP.Text, 1000);
            MessageBox.Show(PR.Status.ToString());
            btnScan.Enabled = btnPing.Enabled = true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            bool found = false;
            string sBuff;

            btnSend.Enabled = false;
            sBuff = cboCMDs.Text.Trim();
            for (int i = 0; i < cboCMDs.Items.Count; i++)
            {
                if (sBuff == cboCMDs.Items[i].ToString())
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                cboCMDs.Items.Add(sBuff);
            }
            if (sBuff.Length > 0)
            {
                cboCMDs.Text = sBuff;
                txtTCP_LOG.AppendText("\r\n[Command][" + DateTime.Now.ToString("HH:mm:ss") + "]:" + sBuff);
                sBuff = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/jig/" + sBuff, 8000);
            }
            btnSend.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtTCP_LOG.Text = "";
            txtSTM_LOG.Text = "";
        }

        private string ExcuteHtml(string url, int ms_timeout)
        {
            string html = "";
            try
            {
                DateTime cmd_starttime = DateTime.Now;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.ReadWriteTimeout = request.Timeout = ms_timeout;
                
                // Set some reasonable limits on resources used by this request
                //request.MaximumAutomaticRedirections = 4;
                //request.MaximumResponseHeadersLength = 4;
                // Set credentials to use for this request.
                //request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Console.WriteLine("Content length is {0}", response.ContentLength);
                //Console.WriteLine("Content type is {0}", response.ContentType);

                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format.
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                html = readStream.ReadToEnd();
                
                this.Invoke((MethodInvoker)delegate
                {
                    txtTCP_LOG.AppendText("\r\n[Response][" + ((int)(DateTime.Now - cmd_starttime).TotalMilliseconds).ToString() + "ms]:" + html);
                });
                response.Close();
                readStream.Close();
            }
            catch (Exception ex)
            {
                html = url + "\r\n" + ex.Message;
            }
            return html;
        }

        private string ExecuteTCPCommand(string cmd, string key_word, int ms_timeout)
        {
            string res="";
            txtTCP_LOG.AppendText("\r\n[Command][" + DateTime.Now.ToString("HH:mm:ss") + "]:" + cmd);
           // res = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/jig/" + cmd, ms_timeout);
            Thread t1 = new Thread(() => res = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/jig/" + cmd, ms_timeout));
            t1.Start();
            //Chờ cho các luồng kết thúc:
            while (t1.IsAlive) Application.DoEvents();
            //{"serial number": "\r\n"}
            //{"hw version": "RevC"}
            if (res.Length > 0)
            {
                res = res.Replace(": ", ":");
                res = res.Replace("{\"", "");
                res = res.Replace("\"}", "");
                string[] separatingStrings = { "\":\"" };
                string[] sItems = res.Split(separatingStrings, StringSplitOptions.None);
                if (sItems.Length > 1)
                {
                    if (sItems[0] == key_word)
                    {
                        return sItems[1];
                    }
                }    
            }
            return "";
        }

        public bool CAN_StartTest()
        {
            byte[] byts;
            //ResetView();
            byts = ExecuteCANCommand(new byte[] { 0x60 }, 500); //=>RX: 0x0A 0x02 0x63 0x00
            if (byts != null && byts[0] == 0x00)
            {
                //ReadSerialNumber();
                //ReadSoftwareVersion();
                //ReadTonyVersion();
                //SetAllOutput(false);

                //ReadAllInput();
                //ReadTemprature();
                //ReadADCValue();
                //FlashTest();

                //SetAllRelay(false);
                return true;
            }
            return false;
        }

        public bool CAN_StopTest()
        {
            byte[] byts;
            //byts = ExecuteCANCommand("02 01 61", 500); //=>RX: 0x0A 0x02 0x63 0x00
            byts = ExecuteCANCommand(new byte[] { 0x61 }, 500); //=>RX: 0x0A 0x02 0x63 0x00
            if (byts == null) byts = ExecuteCANCommand(new byte[] { 0x61 }, 200);
            if (byts == null) byts = ExecuteCANCommand(new byte[] { 0x61 }, 200);
            return true;
        }

        public bool XMC_UartTestIMX()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 01 03", 1000);
            if (retBytes != null)
            {
                return (retBytes.Length == 1 && retBytes[0] == 1);
            }    
            return false;
        }

        public bool XMC_ControlPilotEnter()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 01 05", 1000); //Request test control pilot PWM
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_ControlPilotOn()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 05 01", 1000); //Request enable PWM signal
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_ControlPilotOff()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 05 02", 1000); //Request disable PWM signal
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_CalibrationADCEnter()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 07 00", 1000); //Request test calib ADC
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_CalibrationADCStateE(int value)
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 04 07 45 " + ((value >> 8) & 0xFF).ToString("X2") + " " + (value & 0xFF).ToString("X2"), 1000); //
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_CalibrationADCStateA(int value)
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 04 07 41 " + ((value >> 8) & 0xFF).ToString("X2") + " " + (value & 0xFF).ToString("X2"), 1000); //
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }




        public bool TCP_StartTest()
        {
            string res = "";
            int count = 0;
            string ip = "192.168.1.1";
            TcpClientWithTimeout t = new TcpClientWithTimeout();
            txtIP.Text = "";
            Thread t1 = new Thread(() =>
                {
                    while (txtIP.Text == "" && ++count < 4)
                    {
                        this.Invoke((MethodInvoker)delegate { txtTCP_LOG.AppendText("\r\nTest connection(" + count.ToString() + "): " + ip + ":13150 ... "); });
                        
                        if (t.TestConnect(ip, 13150, 2000))
                        {
                            this.Invoke((MethodInvoker)delegate {
                                txtTCP_LOG.AppendText("OK");
                                txtIP.Text = ip;
                            });                          
                            res = ExecuteTCPCommand("start", "current mode", 2000);
                        }
                    }
                }
            );
            t1.Start();
            //Chờ cho các luồng kết thúc:
            while (t1.IsAlive) Application.DoEvents();
            
            return (res == "jig");
        }

        public bool TCP_StopTest()
        {
            string res;
            res = ExecuteTCPCommand("stop", "current mode", 2000);
            return (res == "normal");
        }

        public string TCP_ReadSerialNumber()
        {
            string sBuff;
            this.Invoke((MethodInvoker)delegate { lblDUTSerial.Text = ""; });
            sBuff = ExecuteTCPCommand("get_serial_number", "serial number", 2000);
            if (sBuff.Length > 0)
            {
                this.Invoke((MethodInvoker)delegate { lblDUTSerial.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { lblDUTSerial.Text = "---"; });
            return "";
        }
        
        public string TCP_ReadHardwareVersion()
        {
            string sBuff;
            this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            sBuff = ExecuteTCPCommand("get_hw_version", "hw version", 2000);
            if (sBuff.Length > 0)
            {
                this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            return "";
        }

        public string TCP_ReadSoftwareVersion()
        {
            string sBuff;
            this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = ""; });
            sBuff = ExecuteTCPCommand("get_sw_version", "sw version", 2000);
            if (sBuff.Length > 0)
            {
                this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = "---"; });
            return "";
        }

        public bool TCP_FlashTest()
        {
            string res;
            res = ExecuteTCPCommand("test_flash", "flash test", 2000);
            return (res.ToUpper() == "PASS");
        }

        public bool TCP_DDRTest()
        {
            string res;
            res = ExecuteTCPCommand("test_ddr", "ddr test", 2000);
            return (res.ToUpper() == "PASS");
        }

        public bool TCP_EMMCTest()
        {
            string res;
            res = ExecuteTCPCommand("test_emmc", "emmc test", 2000);
            return (res.ToUpper() == "PASS");
        }

        public bool TCP_I2CTest()
        {
            string res;
            res = ExecuteTCPCommand("test_i2c", "i2c test", 2000);
            return (res.ToUpper() == "PASS");
        }

        public string TCP_UART_Test(int port_index)
        {
            //[Command][17:05:57]:test_uart/6
            //[Response][2141ms]:{ "uart6 test": "FAIL"}
            string res;
            res = ExecuteTCPCommand("test_uart/" + port_index.ToString(), "uart" + port_index.ToString() + " test", 3500);
            return res;
        }
        public string TCP_CAN_Test(int port_index)
        {
            //[Command][18:08:24]:test_can/0
            //[Response][1102ms]:{ "can0 test": "PASS"}
            string res = "";
            res = ExecuteTCPCommand("test_can/" + port_index.ToString(), "can0 test", 3500);
            return res;
        }

        public string TCP_Ethernet_Test()
        {
            //[Command][18:08:39]:test_eth
            //[Response][38ms]:{ "eth test": "FAIL"}
            string res = "";
            res = ExecuteTCPCommand("test_eth", "eth test", 40000);
            return res;
        }
        
        //MCU_RFID_EN:Chân này chưa dùng
        public bool TCP_GetGPIO_Input(int index) 
        {
            string res = "";
            //[Command] [16:14:49]:get_gpio/0
            //[Response] [1105ms]:{"gpio get 0": "0"}
            res = ExecuteTCPCommand("get_gpio/" + index.ToString(), "gpio get " + index.ToString(), 2000);
            return (res == "1");
        }
        

        private void label9_DoubleClick(object sender, EventArgs e)
        {
            TCP_ReadSerialNumber();
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            TCP_ReadHardwareVersion();
        }

        private void label3_DoubleClick(object sender, EventArgs e)
        {
            TCP_ReadSoftwareVersion();
        }

        private void btnCANSend_Click(object sender, EventArgs e)
        {
            bool found = false;
            string sBuff;

            btnCANSend.Enabled = false;
            sBuff = cboCANCmd.Text.Trim().ToUpper();
            for (int i = 0; i < cboCANCmd.Items.Count; i++)
            {
                if (sBuff == cboCANCmd.Items[i].ToString())
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                cboCANCmd.Items.Add(sBuff);
            }
            cboCANCmd.Text = sBuff;

            //if (!chkHex.Checked) sBuff = Conv.S2SH(cboCANCmd.Text);

            ExecuteCANCommand(sBuff, 200);

            btnCANSend.Enabled = true;
        }

        private void btnClearCAN_Click(object sender, EventArgs e)
        {
            txtCAN_LOG.Text = "";
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            btnPing.Enabled = btnScan.Enabled = false;
            ScanIP();
            btnPing.Enabled = btnScan.Enabled = true;
        }

        public string ScanIP()
        {
            DateTime cmd_starttime = DateTime.Now;
            
            txtIP.Text = "";
            Application.DoEvents();

            Thread t1 = new Thread(() => TestConnection("192.168.1", 1, 10, 13150, 3, 0, 200));
            Thread t2 = new Thread(() => TestConnection("192.168.1", 1, 10, 13150, 3, 1, 200));
            Thread t3 = new Thread(() => TestConnection("192.168.1", 1, 10, 13150, 3, 2, 200));

            t1.Start();
            t2.Start();
            t3.Start();
            //Chờ cho các luồng kết thúc:
            while (t1.IsAlive || t2.IsAlive || t3.IsAlive) Application.DoEvents();

            txtTCP_LOG.AppendText("\r\nScan completed in [" + ((int)(DateTime.Now - cmd_starttime).TotalMilliseconds).ToString() + "ms]");
            return txtIP.Text;
        }

        protected void TestConnection(string ip_master, int ip_low, int ip_high, int port, int num_scan, int num_mod, int ms_timeout)
        {
            string ip = "";
            TcpClientWithTimeout t = new TcpClientWithTimeout();
            for (int i = ip_low; i <= ip_high; i++)
            {
                if (i % num_scan == num_mod)
                {
                    ip = ip_master + "." + i.ToString();
                    this.Invoke((MethodInvoker)delegate { txtTCP_LOG.AppendText("\r\nScan IP:" + ip + " ... "); }); 
                    if (t.TestConnect(ip, port, ms_timeout))
                    {
                        this.Invoke((MethodInvoker)delegate { txtIP.Text = ip; });
                    }
                }
                if (txtIP.Text != "") break;
            }
        }

        public string CAN_ReadSerialNumber()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label5.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x02 }, 500);
            if (byts != null && byts.Length == 19)
            {
                string sBuff = "";
                for (int i = 0; i < 18; i++)
                {
                    sBuff += (char)byts[i + 1];
                }
                this.Invoke((MethodInvoker)delegate { label5.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { label5.Text = "---"; });
            return "";
        }

        private void label6_DoubleClick(object sender, EventArgs e)
        {
            CAN_ReadSerialNumber();
        }

        public string CAN_ReadSoftwareVersion()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label8.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x04 }, 500);
            if (byts != null && byts.Length == 4)
            {
                string sBuff = byts[1].ToString() + "." + byts[2].ToString() + "." + byts[3].ToString();
                this.Invoke((MethodInvoker)delegate { label8.Text = sBuff; });
                return sBuff;
            }
            this.Invoke((MethodInvoker)delegate { label8.Text = "---"; });
            return "";
        }

        private void label10_DoubleClick(object sender, EventArgs e)
        {
            CAN_ReadSoftwareVersion();
        }

        public string CAN_ReadHardwareVersion()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label11.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x03 }, 500);
            if (byts != null && byts.Length == 17)
            {
                string sBuff = "";
                for (int i = 0; i < 16; i++)
                {
                    sBuff += (char)byts[i + 1];
                }
                this.Invoke((MethodInvoker)delegate { label11.Text = sBuff; });
                return sBuff;
            }
            this.Invoke((MethodInvoker)delegate { label11.Text = "---"; });
            return "";
        }

        private void label7_DoubleClick(object sender, EventArgs e)
        {
            CAN_ReadHardwareVersion();
        }

        public bool CAN_FlashTest()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x07 }, 500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = (byts[1] == 0) ? "Pass" : "Fail"; });
                return (byts[1] == 0);
            }
            else
                this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = "---"; });
            return false;
        }

        private void label30_DoubleClick(object sender, EventArgs e)
        {
            CAN_FlashTest();
        }

        public bool CAN_RS485Test()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label12.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x08 }, 1500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { label12.Text = (byts[1] == 0) ? "Pass" : "Fail"; });
                return (byts[1] == 0);
            }
            else
                this.Invoke((MethodInvoker)delegate { label12.Text = "---"; });
            return false;
        }
       
        public bool SetOutput(int out_number, bool value, int ms_delay)
        {
            byte[] byts = null;

            byts = ExecuteCANCommand(new byte[] { 0x62, 0x05, (byte)out_number, (value ? (byte)1 : (byte)0) }, 500);
            if (byts != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (out_number == 0) checkBox1.Checked = value;
                    else if (out_number == 1) checkBox2.Checked = value;
                    else if (out_number == 2) checkBox3.Checked = value;
                    else if (out_number == 3) checkBox4.Checked = value;
                    else if (out_number == 4) checkBox5.Checked = value;
                    else if (out_number == 5) checkBox6.Checked = value;
                    else if (out_number == 6) checkBox7.Checked = value;
                    else if (out_number == 7) checkBox8.Checked = value;
                });
                return true;
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return false;
        }

        public bool SetOutput(int out_number, bool value)
        {
            return SetOutput(out_number, value, 0);
        }

        public void ClearAllLog()
        {
            txtCAN_LOG.Text = "";
            txtSTM_LOG.Text = "";
            txtTCP_LOG.Text = "";
        }

        public int ReadAllInput()
        {
            byte[] byts;
            //this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x06 }, 500);
            if (byts != null && byts.Length == 7)
            {
                int ret = 0;
                if ((byts[1] & 1) == 0) ret += 1;
                if ((byts[2] & 1) == 0) ret += 2;
                if ((byts[3] & 1) == 0) ret += 8;
                if ((byts[4] & 1) == 0) ret += 4;
                if ((byts[5] & 1) == 0) ret += 16;
                if ((byts[6] & 1) == 0) ret += 32;

                /////////////   FW   ////////////
                /*
                if ((byts[1] & 1) == 0) ret += 1;
                if ((byts[2] & 1) == 0) ret += 2;
                if ((byts[3] & 1) == 0) ret += 4;
                if ((byts[4] & 1) == 0) ret += 8;
                if ((byts[5] & 1) == 0) ret += 16;
                if ((byts[6] & 1) == 0) ret += 32;
                */
                ////////////////////////////////

                label18.Text = ((ret & 1) != 0) ? "1 " : "0 ";
                label18.Text += ((ret & 2) != 0) ? "1 " : "0 ";
                label18.Text += ((ret & 4) != 0) ? "1 " : "0 ";
                label18.Text += ((ret & 8) != 0) ? "1 " : "0 ";
                label18.Text += ((ret & 16) != 0) ? "1 " : "0 ";
                label18.Text += ((ret & 32) != 0) ? "1 " : "0 ";

                return ret;
            }
            return -1;
        }

        private void label13_DoubleClick(object sender, EventArgs e)
        {
            CAN_RS485Test();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if ((CheckBox)sender == checkBox1)
                SetOutput(0, checkBox1.Checked);
            else if ((CheckBox)sender == checkBox2)
                SetOutput(1, checkBox2.Checked);
            else if ((CheckBox)sender == checkBox3)
                SetOutput(2, checkBox3.Checked);
            else if ((CheckBox)sender == checkBox4)
                SetOutput(3, checkBox4.Checked);
            else if ((CheckBox)sender == checkBox5)
                SetOutput(4, checkBox5.Checked);
            else if ((CheckBox)sender == checkBox6)
                SetOutput(5, checkBox6.Checked);
            else if ((CheckBox)sender == checkBox7)
                SetOutput(6, checkBox7.Checked);
            else if ((CheckBox)sender == checkBox8)
                SetOutput(7, checkBox8.Checked);
        }

        private void label19_DoubleClick(object sender, EventArgs e)
        {
            int ret = ReadAllInput();
        }
    }
}
