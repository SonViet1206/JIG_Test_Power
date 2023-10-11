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
    public partial class DUT_PLC_AC : UserControl
    {
        const byte CAN_DUT_ADDRESS = 0x01;
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
        Thread threadCOMM;
        Thread threadCAN;

        bool tcp_thread_busy = false;
        bool tcp_command_busy = false;
        bool tcp_comm_started = false;

        string _IP_address = "192.168.1.1";
        //SerialPort portCOMM = null;

        string _port_name = "COM8";
        double _unit_factor = 1;
        SerialPort portCOMM = null;

        string can_serial = "---";

        TcpClient client = null;
        
        public DUT_PLC_AC()
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

        public void Start(string port_name)
        {
            _port_name = port_name;
            lblCaption.Text = "DEVICE UNDER TEST - " + port_name;

            threadCOMM = new Thread(() => COMMProcess());
            threadCOMM.Start();

            threadCAN = new Thread(() => CANProcess());
            threadCAN.Start();
        }

        public void Stop()
        {
            tcp_thread_started = false;
            while (!tcp_thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }

            thread_started = false;
            while (!thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (!tcp_thread_started && !thread_started)
                Start(_port_name);
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
                                            retBytes = new byte[rxFrame.DataLen - 3];
                                            for (int i = 0; i < retBytes.Length; i++) retBytes[i] = rxFrame.data[i + 3];

                                            break;
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
                            //if (txtCAN_LOG.Text.Length > 100000) txtCAN_LOG.Text = txtCAN_LOG.Text.Substring(txtCAN_LOG.Text.Length - 100000);
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
        string lastFrame = "";
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
                                            //if (mMsg.ID == CAN_DUT_ADDRESS && rxData[0] == CAN_SOFT_ADDRESS && mMsg.DataLen > 3 && rxData[2] == CAN_CMDID_RESPONSE)
                                            if (mMsg.ID == CAN_DUT_ADDRESS && rxData[0] == CAN_SOFT_ADDRESS)
                                            {
                                                //ThreadFrameProcess(rxData);
                                                lastFrame = Conv.Br2S(rxData);
                                            }
                                            //Xử lý frame heartbeat của IMX:
                                            if (rxData.Length == 8 && rxData[0] == 0x55 && rxData[1] == 0x55 && rxData[2] == 0x55 && rxData[3] == 0x55 && rxData[4] == 0x55 && rxData[5] == 0x55 && rxData[6] == 0x55 && rxData[7] == 0x55)
                                            {
                                                label8.Text = "OK";
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

            
            
            try
            {
                tcp_thread_ended = false;
                tcp_thread_started = true;

                while (tcp_thread_started)
                {
                    while (tcp_command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
                    
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
                this.Invoke((MethodInvoker)delegate
                {
                    btnOnOff.BackColor = Color.Black;
                    lblCaption.Text = "DEVICE UNDER TEST - " + _port_name + ":---";
                });
            }
            catch { }

            tcp_thread_ended = true;
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            if (txtIP.Text.Trim() == "") return;
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
                sBuff = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/jig/" + sBuff, 5000);
            }
            btnSend.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtTCP_LOG.Text = "";
        }

        private string ExcuteHtml(string url, int ms_timeout)
        {
            string html = "";
            try
            {
                Application.DoEvents();

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
                //txtTCP_LOG.AppendText("\r\n[Response][" + ((int)(DateTime.Now - cmd_starttime).TotalMilliseconds).ToString() + "ms]:" + html);
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
            Application.DoEvents();
            txtTCP_LOG.AppendText("\r\n[Command][" + DateTime.Now.ToString("HH:mm:ss") + "]:" + cmd);

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

        private string ExecuteTCPCommand(string cmd, int ms_timeout)
        {
            string res = "";
            txtTCP_LOG.AppendText("\r\n[Command][" + DateTime.Now.ToString("HH:mm:ss") + "]:" + cmd);
            //res = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/jig/" + cmd, ms_timeout);
            Thread t1 = new Thread(() => res = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/jig/" + cmd, ms_timeout));
            t1.Start();
            //Chờ cho các luồng kết thúc:
            while (t1.IsAlive) Application.DoEvents();

            //{"serial number": "\r\n"}
            //{"hw version": "RevC"}
            if (res.Length > 0)
            {
                return res;
            }
            return "";
        }

        public void ClearLog()
        {
            txtCAN_LOG.Text = "";
            txtTCP_LOG.Text = "";
            label6.Text = "---";
        }

        public bool BootingOK()
        {
            return (label6.Text == "OK");
        }
        
        public bool CAN_StartTest()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 06 01 00 00 00 B3 79", 1000);
            return (retBytes != null);
        }

        public bool CAN_StopTest()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 01 0D", 1000);
            return (retBytes != null);
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
            byte[] retBytes = null; //Lệnh này cần thực hiện hai lần do firm
            //ExecuteCANCommand("01 02 05 01", 1000); //Request enable PWM signal
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

        public int XMC_CalibrationADCStateE(int value)
        {
            int retValue = 0;
            byte[] retBytes = null;
            //[13:51:34.443][TX][6][01040745048B][0][0]
            //[13:51:34.457][RX][7][0A0508450EF300][0][0]
            retBytes = ExecuteCANCommand("01 04 07 45 " + ((value >> 8) & 0xFF).ToString("X2") + " " + (value & 0xFF).ToString("X2"), 1000); //
            if (retBytes != null && retBytes.Length == 4 && retBytes[0] == 0x45)
            {
                retValue = (int)retBytes[3] * 0x10000 + (int)retBytes[1] * 0x100 + (int)retBytes[2];
            }
            return retValue;
        }

        public long XMC_CalibrationADCStateA(int value)
        {
            ///
            //[16:29:49.222][TX][6][010407450458][0][0]
            //[16:29:49.251][RX][7][0A050845081F01][0][0]
            //[16:29:56.645][TX][6][01040741044E][0][0]

            //[16:29:56.705][RX][8][0A060841 0E FE F6 00][0][0]
            //[16:29:56.986][DX][8][0A060846 01 2A F5 01][0][0]

            long retValue = 0;
            byte[] retBytes = null;
            lastFrame = "";
            retBytes = ExecuteCANCommand("01 04 07 41 " + ((value >> 8) & 0xFF).ToString("X2") + " " + (value & 0xFF).ToString("X2"), 1000); //
            if (retBytes != null && retBytes.Length == 5 && retBytes[0] == 0x41)
            {
                int count = 0;
                retValue = (long)retBytes[4] * 0x1000000 + (long)retBytes[3] * 0x10000 + (long)retBytes[1] * 0x100 + (long)retBytes[2];
                while (lastFrame == "" && ++count < 100) Conv.delay_ms(10);
                if (lastFrame != "")
                {
                    retBytes = Conv.S2Br(lastFrame);
                    if (retBytes != null && retBytes.Length == 8 && retBytes[3] == 0x46)
                    {
                        retValue <<= 32;
                        retValue += (long)retBytes[7] * 0x1000000 + (long)retBytes[6] * 0x10000 + (long)retBytes[4] * 0x100 + (long)retBytes[5];
                    }
                    else
                        retValue = 0;
                }
                else 
                    retValue = 0;
            }
            return retValue;
        }

        public bool XMC_TriggerInterruptEnter()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 09 00", 1000); //Request test control pilot PWM
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_TriggerInterruptA2B_On(int ms_delay)
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 09 01", ms_delay); 
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_TriggerInterruptA2B_Off(int ms_delay)
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 09 02", ms_delay);
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public bool XMC_TriggerInterruptD2E_On(int ms_delay)
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 09 03", ms_delay);
            if (retBytes != null)
            {
                return true;
            }
            return false;
        }

        public long XMC_TriggerInterruptD2E_Off(int ms_delay)
        {
            long retValue = 0;
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 09 04", ms_delay);
            if (retBytes != null && retBytes.Length == 5)
            {
                retValue = (long)retBytes[0] * 0x1000000 + (long)retBytes[1] * 0x10000 + (long)retBytes[2] * 0x100 + retBytes[3];
                retValue += (long)retBytes[4] * 0x100000000;
            }
            return retValue;
        }

        public int XMC_PMWInterrupt()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 01 0B", 2000); //Request test control pilot PWM
            if (retBytes != null && retBytes.Length == 1)
            {
                return retBytes[0];
            }
            return 0;
        }
        /// <summary>
        /// ////////04052021
        /// </summary>
        /// <returns></returns>
        /// 
        //[14:55:09.722][TX][3][01010B][0][0]
        //[14:55:09.826][RX][4][0A020C01][0][0]
        //[14:55:11.948][TX][4][01020E00][0][0]
        //[14:55:11.964][RX][3][0A010F][0][0]
        //[14:55:12.959][TX][4][01020E01][0][0]
        //[14:55:13.088][RX][6][0A040F018100][0][0]
        //[14:55:14.103][TX][4][01020E01][0][0]
        //[14:55:14.112][RX][3][0A010F][0][0]
        //[14:55:16.353][TX][3][010110][0][0]
        //[14:55:16.361][RX][6][0A0411010217][0][0]



        public bool XMC_PilotPWMInterrupt()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 01 0B", 1000); 
            if (retBytes != null)
            {
                if(retBytes.Length >=1)
                return retBytes[0] == 0x00 ? false : true;
            }
            return false;
        }
        public bool XMC_ProximitySensor_Req()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 0E 00", 1000);
            if (retBytes != null)
            {
                return retBytes[0] == 0x0F ? true : false;
            }
            return false;
        }

        public bool XMC_ProximitySensor_Read( out int value)
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 02 0E 01", 300);
            if (retBytes != null)
            {
                if(retBytes.Length >= 3)
                {
                    value = (int)retBytes[0] * 256 + (int)retBytes[1];
                    return retBytes[2] == 0x00 ? false : true;
                }
            }
            value = 0;
            return false;
        }

        public string XMC_Read_SoftwareVersion()
        {
            byte[] retBytes = null;
            retBytes = ExecuteCANCommand("01 01 10", 1000);
            if (retBytes != null)
            {
                //010217
                if (retBytes.Length == 3)
                {
                    string sBuff = retBytes[0].ToString() + "." + retBytes[1].ToString() + "." + retBytes[2].ToString();
                    this.Invoke((MethodInvoker)delegate { label8.Text = sBuff; });
                    return sBuff;
                }
            }
            return "";
        }

        public bool TCP_StartTest()
        {
            string res = "";
            //if (ScanIP() != "")
            //{
            //    res = ExecuteTCPCommand("start", "current mode", 2000);
            //}

            if (ScanIP("10.128.128.76") != "")
            {
                Conv.delay_ms(1000);
                res = ExecuteTCPCommand("start", "current mode", 2000);
            }
            return (res == "jig");
        }

        public bool TCP_StopTest()
        {
            string res;
            res = ExecuteTCPCommand("stop", "current mode", 15000);
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

        public string TCP_FlashTest()
        {
            return ExecuteTCPCommand("test_flash", "flash test", 2000);
        }

        public string TCP_DDRTest()
        {
            return ExecuteTCPCommand("test_ddr", "ddr test", 2000);
        }

        public string TCP_EMMCTest()
        {
            return ExecuteTCPCommand("test_emmc", "emmc test", 2000);
        }

        
        public string TCP_CANTest()
        {
            int s_count = 0;
            
            this.Invoke((MethodInvoker)delegate { label8.Text = "---"; });
            while (label8.Text != "PASS" && ++s_count < 2)
            {
                Conv.delay_ms(1000);
                if (ExecuteTCPCommand("test_can", "can test", 3000) == "PASS")
                {
                    this.Invoke((MethodInvoker)delegate { label8.Text = "PASS"; });
                    
                    break;
                }
            }          
            return label8.Text;    
            //return 
        }

        public string TCP_I2CTest()
        {
            return ExecuteTCPCommand("test_i2c", "i2c test", 2000);
        }

        public string TCP_EthernetTest()
        {
            return ExecuteTCPCommand("test_eth", "eth test", 20000);
        }

        public string TCP_HLCSlacTest()
        {
            return ExecuteTCPCommand("test_hlc", "hlc test", 40000);
        }

        public string TCP_TempTest(string key_word)
        {
            string value =  ExecuteTCPCommand("test_gun_temp", 3000);
            if(value.Length > 0)
            {

                value = value.Replace(": ", ":");
                value = value.Replace("{\"", "");
                value = value.Replace("\"}", "");
                //temp2":"206.4", "temp1":"62.5
                value = value.Replace("\"", "");
                //temp2:206.4, temp1:62.5
                string[] temparr = value.Split(',');
                if(temparr.Length > 1)
                {
                    string[] separatingStrings = { ":" };
                    string[] sItems = temparr[0].Trim().Split(separatingStrings, StringSplitOptions.None);
                    if (sItems.Length > 1)
                    {
                        if (sItems[0] == key_word)
                        {
                            return sItems[1];
                        }
                    }
                    sItems = temparr[1].Trim().Split(separatingStrings, StringSplitOptions.None);
                    if (sItems.Length > 1)
                    {
                        if (sItems[0] == key_word)
                        {
                            return sItems[1];
                        }
                    }
                }
            }
            return "";
        }

        
        public string TCP_USBTest()
        {
            return ExecuteTCPCommand("test_usb", "usb info", 5000);
        }

        public string TCP_PLCICInfoTest()
        {
            //[Response][3335ms]:{"MAC": "00:01:87:00:44:0", "SID": "02:6B:CB:A5:35:4E:08", "mode": "Norm", "NMK": "B5:93:19:D7:E8:15:7B:A0:01:B0:18:66:9C:CE:E3:"}
            //Trả lại OK khi "mode": "Norm"
            string res = ExecuteTCPCommand("get_plc_ic_info", 5000);
            if (res.IndexOf("\"mode\": \"Normal\"") > 0)
                return "PASS";
            return "";
        }

        public string TCP_UART_Test()
        {
            return ExecuteTCPCommand("test_uart", "uart test", 5000);
        }

        public string TCP_UART4_Test()
        {
            return ExecuteTCPCommand("test_uart/4", "uart test", 5000);
        }

        public string TCP_UART5_Test()
        {
            return ExecuteTCPCommand("test_uart/5", "uart test", 5000);
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
            //ScanIP("10.150.92.88");
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

        public string ScanIP(string ip)
        {
            int s_count = 0;
            this.Invoke((MethodInvoker)delegate { txtIP.Text = ""; });
            while(++s_count < 5 && txtIP.Text == "")
            {
                if (txtIP.Text != "") break;
                TestConnection(ip, 13150, 500);

            }          
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

        protected void TestConnection(string ip_master, int port, int ms_timeout)
        {
            TcpClientWithTimeout t = new TcpClientWithTimeout();
            this.Invoke((MethodInvoker)delegate { txtTCP_LOG.AppendText("\r\nTest connection IP:" + ip_master + " ... "); });
            if (t.TestConnect(ip_master, port, ms_timeout))
            {
                this.Invoke((MethodInvoker)delegate { txtIP.Text = ip_master; });
            }
        }
    }
}
