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
using ECAN;
using ECanTest;
using System.Diagnostics;

namespace DefaultNS
{
    public partial class DUT_MainAC11 : UserControl
    {
        //const byte CAN_DUT_ADDRESS = 0x00;
        const byte CAN_DUT_ADDRESS = 0x04;
        const byte CAN_SOFT_ADDRESS = 0x0A;

        const byte CAN_CMDID_START_TEST = 0x60;
        const byte CAN_CMDID_STOP_TEST = 0x61;
        const byte CAN_CMDID_REQUEST = 0x62;
        const byte CAN_CMDID_RESPONSE = 0x63;

        public string STATUS_NORMAL_MODE = "NORMAL MODE";
        public string STATUS_UNDER_VOLTAGE = "UNDER VOLTAGE";
        public string STATUS_OVER_VOLTAGE = "OVER VOLTAGE";
        public string STATUS_OVER_CURRENT = "OVER CURRENT";
        public string STATUS_RESIDUAL_CURRENT = "RESIDUAL CURRENT";
        public string STATUS_NO_LOAD = "NO LOAD";
        public string STATUS_ON = "ON";
        public string STATUS_OFF = "OFF";


        ComProc mCan = new ComProc();

        bool thread_busy = false;
        bool command_busy = false;
        bool comm_started = false;

        bool thread_started = false;
        bool thread_ended = true;
        Thread threadCOMM;
        
        public double L1U, L1I, L1P, L1E;
        public double L2U, L2I, L2P, L2E;
        public double L3U, L3I, L3P, L3E;

        public double L1UValue { get { return L1U; } }
        public double L2UValue { get { return L2U; } }
        public double L3UValue { get { return L3U; } }

        public double L1IValue { get { return L1I; } }
        public double L2IValue { get { return L2I; } }
        public double L3IValue { get { return L3I; } }

        public string AlertState { get { return textBox1.Text; } }

        public DUT_MainAC11()
        {
            InitializeComponent();
        }

        private void DUT_MainAC11_Load(object sender, EventArgs e)
        {
            //this.Width = pictureBox3.Width;
            //this.Height = pictureBox3.Height;
            this.Font = new Font("Tahoma", 12);
            ResetView();
        }

        public bool CommState { get { return comm_started; } }
        
        public void Start()
        {
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
                Start();
            else
                Stop();
        }

        private void ResetView()
        {
            this.Invoke((MethodInvoker)delegate
            {
                lblDUTSerial.Text = "---";
                lblSoftwareVersion.Text = "---";
                lblTmprValue.Text = "---";
                lblADC.Text = "---";
                lblFlashTest.Text = "---";
                lblHardwareVersion.Text = "---";

            });
        }

        public bool StartTest()
        {
            byte[] byts;
            ResetView();
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

        public bool StopTest()
        {
            byte[] byts;
            //byts = ExecuteCANCommand("02 01 61", 500); //=>RX: 0x0A 0x02 0x63 0x00
            byts = ExecuteCANCommand(new byte[] { 0x61 }, 500); //=>RX: 0x0A 0x02 0x63 0x00
            if (byts == null) byts = ExecuteCANCommand(new byte[] { 0x61 }, 200);
            if (byts == null) byts = ExecuteCANCommand(new byte[] { 0x61 }, 200);
            return true;
        }

        public string ReadSerialNumber()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblDUTSerial.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0C, 0x00 }, 500);
            if (byts != null && byts.Length >= 19)
            {
                string sBuff = "";
                for (int i = 0; i < 18; i++)
                {
                    sBuff += (char)byts[i + 1];
                }
                this.Invoke((MethodInvoker)delegate { lblDUTSerial.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { lblDUTSerial.Text = "---"; });
            return "";
        }

        public string ReadTonyVersion()
        {
            //string sBuff = "";
            //byte[] byts;
            //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            //byts = ExecuteCANCommand(new byte[] { 0x62, 0x00, 0 }, 500); 
            //if (byts != null && byts.Length == 5)
            //{
            //    sBuff += byts[2].ToString() + "." + byts[3].ToString() + "." + byts[4].ToString();
            //    byts = ExecuteCANCommand(new byte[] { 0x62, 0x00, 1 }, 500);
            //    if (byts != null && byts.Length == 5)
            //    {
            //        sBuff += "|" + byts[2].ToString() + "." + byts[3].ToString() + "." + byts[4].ToString();
            //        byts = ExecuteCANCommand(new byte[] { 0x62, 0x00, 2 }, 500);
            //        if (byts != null && byts.Length == 5)
            //        {
            //            sBuff += "|" + byts[2].ToString() + "." + byts[3].ToString() + "." + byts[4].ToString();
            //            this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
            //            return sBuff;
            //        }
            //    }
            //}
            //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            //return "";

            string sBuff = "";
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x00, 0 }, 500);
            if (byts != null && byts.Length == 5)
            {
                sBuff += byts[2].ToString() + "." + byts[3].ToString() + "." + byts[4].ToString();
                
            }
            else sBuff += "-";
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x00, 1 }, 500);
            if (byts != null && byts.Length == 5)
            {
                sBuff += "|" + byts[2].ToString() + "." + byts[3].ToString() + "." + byts[4].ToString();
                
            }
            else sBuff += "|-";
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x00, 2 }, 500);
            if (byts != null && byts.Length == 5)
            {
                sBuff += "|" + byts[2].ToString() + "." + byts[3].ToString() + "." + byts[4].ToString();
                this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
                return sBuff;
            }
            else sBuff += "|-";
            this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            return "";
        }

        public string ReadSoftwareVersion()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0A, 0x00 }, 500);
            if (byts != null && byts.Length == 4)
            {
                string sBuff = byts[1].ToString() + "." + byts[2].ToString() + "." + byts[3].ToString();
                this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = sBuff; });
                return sBuff;
            }
            this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = "---"; });
            return "";
        }

        public bool ReadACParameter()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label11.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x03, 0x00 }, 200); //
            if (byts != null && byts.Length == 10)
            {
                L1U = byts[2] * 256 + byts[3];
                L1I = ((double)(byts[4] * 256 + byts[5])) / 1000;
                L1P = byts[6] * 256 + byts[7];
                L1E = byts[8] * 256 + byts[9];

                byts = ExecuteCANCommand(new byte[] { 0x62, 0x03, 0x01 }, 200); //
                if (byts != null && byts.Length == 10)
                {
                    L2U = byts[2] * 256 + byts[3];
                    L2I = ((double)(byts[4] * 256 + byts[5])) / 1000;
                    L2P = byts[6] * 256 + byts[7];
                    L2E = byts[8] * 256 + byts[9];

                    byts = ExecuteCANCommand(new byte[] { 0x62, 0x03, 0x02 }, 200); //
                    if (byts != null && byts.Length == 10)
                    {
                        L3U = byts[2] * 256 + byts[3];
                        L3I = ((double)(byts[4] * 256 + byts[5])) / 1000;
                        L3P = byts[6] * 256 + byts[7];
                        L3E = byts[8] * 256 + byts[9];
                        this.Invoke((MethodInvoker)delegate
                        {
                            //label11.Text = L1U.ToString() + "(" + L1I.ToString() + ")" + "|" + L2U.ToString() + "(" + L2I.ToString() + ")" + "|" + L3U.ToString() + "(" + L3I.ToString() + ")";
                            label11.Text = L1U.ToString() +  "|" + L2U.ToString() + "|" + L3U.ToString();
                        });
                        return true;
                    }
                }
            }
            this.Invoke((MethodInvoker)delegate { label11.Text = "---"; });
            return false;
        }

        public double ReadTemprature()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x07, 0x00 }, 500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = byts[1].ToString(); });
                return byts[1];
            }
            else
                this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = "---"; });
            return -1;
        }

        public long ReadADC()
        {
            long retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblADC.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x08, 0x00 }, 500);
            if (byts != null && byts.Length == 3)
            {
                retValue = byts[1] * 0x100 + byts[2];
                this.Invoke((MethodInvoker)delegate 
                { 
                    lblADC.Text = retValue.ToString(); 
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { lblADC.Text = "---"; });
            return retValue;
        }

        public int ReadResidualCurrent()
        {
            int retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label17.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x04, 0x00 }, 1000);
            if (byts != null && byts.Length == 3)
            {
                retValue = byts[1] * 0x100 + byts[2];
                this.Invoke((MethodInvoker)delegate
                {
                    label17.Text = retValue.ToString();
                    label17.Text = byts[1].ToString() + "|" + byts[2].ToString() + "[mA]";
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label17.Text = "---"; });
            return retValue;
        }

        public bool FlashTest()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x09, 0x00 }, 500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = (byts[1] == 0) ? "Pass" : "Fail"; });
                return (byts[1] == 0);
            }
            else
                this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = "---"; });
            return false;
        }

        public bool RFID_UART_Test()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label8.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0D, 0x00 }, 1500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { label8.Text = (byts[1] == 0) ? "Pass" : "Fail"; });
                return (byts[1] == 0);
            }
            else
                this.Invoke((MethodInvoker)delegate { label8.Text = "---"; });
            return false;
        }

        public bool SetOutput(int value, int ms_delay)
        {
            byte[] byts = null;
            
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0F, (byte)value }, 500);
            if (byts != null)
            {
                //this.Invoke ((MethodInvoker) delegate
                //{
                //    chkOut0.Checked = ((value & 1) != 0);
                //    chkOut1.Checked = ((value & 2) != 0);
                //    chkOut2.Checked = ((value & 4) != 0);
                //    chkOut3.Checked = ((value & 8) != 0);
                //});
                return true;
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return false;
        }

        public bool SetOutput(int value)
        {
            return SetOutput(value, 0);
        }
        
        private void lblTmprValue_DoubleClick(object sender, EventArgs e)
        {
            ReadTemprature();
        }

        public bool SetAllOutput(bool state, int ms_delay)
        {
            if (state)
            {
                if (ExecuteCANCommand(new byte[] { 0x62, 0x05, 0x01 }, 200) != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        chkOut0.Checked = chkOut1.Checked = chkOut2.Checked = chkOut3.Checked = true;
                    });
                    return true;
                }
            }
            else
            {
                if (ExecuteCANCommand(new byte[] { 0x62, 0x05, 0x00 }, 200) != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        chkOut0.Checked = chkOut1.Checked = chkOut2.Checked = chkOut3.Checked = false;
                    });
                    return true;
                }
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return false;
        }

        public bool SetAllOutput(bool state)
        {
            return SetAllOutput(state, 0);
        }

        public bool SetRelayOutput(int index, bool state)
        {
            return SetRelayOutput(index, state, 0);
        }

        public bool SetRelayOutput(int index, bool state, int ms_delay)
        {
            if (state)
            {
                if (ExecuteCANCommand(new byte[] { 0x62, 0x01, (byte)index }, 800) != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (index == 0)
                        {
                            btnOutN.BackColor = Color.Red;
                            btnOutL1.BackColor = Color.Red;
                        }
                        else if (index == 1)
                        {
                            btnOutL2.BackColor = Color.Red;
                            btnOutL3.BackColor = Color.Red;
                        }
                        else if (index == 2)
                        {
                            btnOutG.BackColor = Color.Red;
                        }
                    });
                    textBox1.Text = ""; //Reset alert state
                    return true;
                }
            }
            else
            {
                if (ExecuteCANCommand(new byte[] { 0x62, 0x02, (byte)index }, 200) != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (index == 0)
                        {
                            btnOutN.BackColor = Color.DimGray;
                            btnOutL1.BackColor = Color.DimGray;
                        }
                        else if (index == 1)
                        {
                            btnOutL2.BackColor = Color.DimGray;
                            btnOutL3.BackColor = Color.DimGray;
                        }
                        else if (index == 2)
                        {
                            btnOutG.BackColor = Color.DimGray;
                        }
                    });
                    textBox1.Text = ""; //Reset alert state
                    return true;
                }
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return false;
        }

        public int ReadAllInput()
        {
            byte[] byts;
            //this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x06, 0x00 }, 500);
            if (byts != null && byts.Length == 2)
            {
                int ret = 0;
                this.Invoke((MethodInvoker)delegate 
                {
                    chkInp0.Checked = ((byts[1] & 1) != 0);
                    chkInp1.Checked = ((byts[1] & 2) != 0);
                    chkInp2.Checked = ((byts[1] & 4) != 0);
                    chkInp3.Checked = ((byts[1] & 8) != 0);
                });
                
                return ret;
            }
            return -1;
        }

        public int ReadAllCapture()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label19.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x10, 0x00 }, 2000);
            if (byts != null && byts.Length == 4)
            {
                int ret = byts[1] * 0x10000 + byts[2] * 0x100 + byts[3];
                this.Invoke((MethodInvoker)delegate
                {
                    this.Invoke((MethodInvoker)delegate { label19.Text = byts[1].ToString() + "|" + byts[2].ToString() + "|" + byts[3].ToString() + "[Hz]"; });
                });
                return ret;
            }
            else
                this.Invoke((MethodInvoker)delegate { label19.Text = "---"; });
            return -1;
        }

        private void lblSoftVersion_DoubleClick(object sender, EventArgs e)
        {
            ReadSoftwareVersion();
        }

        private void lblDUTSerial_DoubleClick(object sender, EventArgs e)
        {
            ReadSerialNumber();
        }

        private void label14_DoubleClick(object sender, EventArgs e)
        {
            SetAllOutput(false);
        }

        private void label13_DoubleClick(object sender, EventArgs e)
        {
            SetAllOutput(true);
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            ReadSoftwareVersion();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            
        }

        private void label9_DoubleClick(object sender, EventArgs e)
        {
            ReadSerialNumber();
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {
            ReadTemprature();
        }

        private void label30_DoubleClick(object sender, EventArgs e)
        {
            FlashTest();
        }

        private void btnOutN_L1_Click(object sender, EventArgs e)
        {
            //SetRelayOutput(0, btnOutN.BackColor == Color.DimGray);
            SetRelayOutput(0, true);
        }

        private void btnOutL2_L3_Click(object sender, EventArgs e)
        {
            //SetRelayOutput(1, btnOutL2.BackColor == Color.DimGray);
            SetRelayOutput(1, true);
        }

        private void btnOutG_Click(object sender, EventArgs e)
        {
            SetRelayOutput(2, btnOutG.BackColor == Color.DimGray);
        }

        private void label10_DoubleClick(object sender, EventArgs e)
        {
            RFID_UART_Test();
        }

        private void label16_DoubleClick(object sender, EventArgs e)
        {
            ReadACParameter();
        }

        private void chkOut0_CheckedChanged(object sender, EventArgs e)
        {
            int state = 0;
            if (chkOut0.Checked) state += 1;
            if (chkOut1.Checked) state += 2;
            if (chkOut2.Checked) state += 4;
            if (chkOut3.Checked) state += 8;
            SetOutput(state);
            //SetAllOutput(chk.Checked);
        }

        private void btnReadAllInput_Click(object sender, EventArgs e)
        {
            ReadAllInput();
        }

        private void label18_DoubleClick(object sender, EventArgs e)
        {
            ReadResidualCurrent();
        }

        private void label2_DoubleClick(object sender, EventArgs e)
        {
            ReadTonyVersion();
        }

        private void btnOutL1_Click(object sender, EventArgs e)
        {
            SetRelayOutput(0, false);
        }

        private void btnOutL3_Click(object sender, EventArgs e)
        {
            SetRelayOutput(1, false);
        }

        private void label20_DoubleClick(object sender, EventArgs e)
        {
            ReadAllCapture();
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label45_DoubleClick(object sender, EventArgs e)
        {
            ReadADC();
        }

        private void btnReadAllDI_Click(object sender, EventArgs e)
        {
            ReadAllInput();
        }

        void ThreadFrameProcess(byte[] rxData)
        {
            if (rxData[3] == 0x06 || rxData[3] == 0x07 || rxData[3] == 0x08) //OVER_VOLTAGE_ALERT || UNDER_VOLTAGE_ALERT
            {
                btnOutN.BackColor = btnOutL1.BackColor = btnOutL2.BackColor = btnOutL3.BackColor = btnOutG.BackColor = Color.DimGray;
                if (rxData[3] == 0x06)
                    textBox1.Text = STATUS_OVER_VOLTAGE;
                else if (rxData[3] == 0x07)
                    textBox1.Text = STATUS_NORMAL_MODE;
                else if (rxData[3] == 0x08)
                    textBox1.Text = STATUS_UNDER_VOLTAGE;
            }
        }
        
        void COMMProcess()
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
                                        StartTest();
                                        ReadSerialNumber();
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
                                            txtLog.AppendText("\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][DX][" + mMsg.DataLen.ToString() + "][" + Conv.S2SH(Conv.bytesToString(rxData)).Replace(" ", "") + "][" + mMsg.ExternFlag.ToString() + "][" + mMsg.RemoteFlag.ToString() + "]");
                                            txtLog.SelectionStart = txtLog.Text.Length;
                                            //Xu ly cac frame su kien: 
                                            if (mMsg.ID == CAN_DUT_ADDRESS && rxData[0] == CAN_SOFT_ADDRESS && mMsg.DataLen > 3 && rxData[2] == CAN_CMDID_RESPONSE)
                                            {
                                                ThreadFrameProcess(rxData);
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
                                        txtErr1.Text = string.Format("{0:X4}", mErrInfo.ErrCode);
                                        txtErr2.Text = string.Format("{0:X4}", mErrInfo.Passive_ErrData[1]);
                                        txtErr3.Text = string.Format("{0:X4}", mErrInfo.Passive_ErrData[2]);
                                    });
                                    commOK = true;
                                    //Nếu không có dữ liệu thì dãn cách lệnh:
                                    if (sCount == 0) Thread.Sleep(100);
                                }
                                //Đọc giá trị DIs:

                            }
                            catch {}
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

        private void btnSend_Click(object sender, EventArgs e)
        {
            bool found = false;
            string sBuff;

            btnSend.Enabled = false;
            sBuff = cboCMDs.Text.Trim().ToUpper();
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
            cboCMDs.Text = sBuff;

            if (!chkHex.Checked) sBuff = Conv.S2SH(cboCMDs.Text);
            
            ExecuteCANCommand(sBuff, 200);
            
            btnSend.Enabled = true;
        }

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
                            txtLog.AppendText("\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][TX][" + txFrame.DataLen.ToString() + "][" + cmd_hex.Replace(" ","") + "][" + txFrame.ExternFlag.ToString() + "][" + txFrame.RemoteFlag.ToString() + "]");
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
                                            txtLog.AppendText("\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][RX][" + rxFrame.DataLen.ToString() + "][" + sRXH.Replace(" ", "") + "][" + rxFrame.ExternFlag.ToString() + "][" + rxFrame.RemoteFlag.ToString() + "]");
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


                                                //TX: 04 03 62 01 00
                                                //RX: 0A 10 0B 63 04 00 00 D3
                                                //TX: 00 30 00 00
                                                //RX: 0A 21 01 CB 00 60 00 14


                                                txFrame.DataLen = 4;
                                                txFrame.data[0] = CAN_DUT_ADDRESS;
                                                txFrame.data[1] = 0x30;
                                                txFrame.data[2] = 0x00;
                                                txFrame.data[3] = 0x00;

                                                this.Invoke((MethodInvoker)delegate
                                                {
                                                    txtLog.AppendText("\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][TX][" + txFrame.DataLen.ToString() + "][00300000][" + txFrame.ExternFlag.ToString() + "][" + txFrame.RemoteFlag.ToString() + "]");
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
                                                                txtLog.AppendText("\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][RX][" + rxFrame.DataLen.ToString() + "][" + sRXH.Replace(" ", "") + "][" + rxFrame.ExternFlag.ToString() + "][" + rxFrame.RemoteFlag.ToString() + "]");
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
                                                retBytes = new byte[rxFrame.DataLen-3];
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
                            txtLog.SelectionStart = txtLog.Text.Length;
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
        }

        
    }
}
