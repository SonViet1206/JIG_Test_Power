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
    public partial class DUT_RelayCtrl : UserControl
    {
        const byte CAN_DUT_ADDRESS = 0x07;
        const byte CAN_SOFT_ADDRESS = 0x0A;

        const byte CAN_CMDID_START_TEST = 0x60;
        const byte CAN_CMDID_STOP_TEST = 0x61;
        const byte CAN_CMDID_REQUEST = 0x62;
        const byte CAN_CMDID_RESPONSE = 0x63;

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
        
        string can_serial = "---";

        public double L1U, L1I, L1P, L1E;
        public double L2U, L2I, L2P, L2E;
        public double L3U, L3I, L3P, L3E;
        public double L4U, L4I, L4P, L4E;

        public double temprature = 0;

        public DUT_RelayCtrl()
        {
            InitializeComponent();
        }

        private void SPM91_Load(object sender, EventArgs e)
        {
            //this.Width = pictureBox3.Width;
            //this.Height = pictureBox3.Height;
            this.Font = new Font("Tahoma", 12);
            ResetView();
        }

        public bool CommState { get { return comm_started; } }
        
        public string CANSerial { get { return can_serial; } }

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
                lblACVolt.Text = "---";
                lblDCVolt.Text = "---";
                lblFlashTest.Text = "---";
                lblHardwareVersion.Text = "---";

                btnDI1.BackColor = Color.DimGray;
                btnDI2.BackColor = Color.DimGray;
                btnDI3.BackColor = Color.DimGray;
                btnDI4.BackColor = Color.DimGray;
                btnDI5.BackColor = Color.DimGray;
                btnDI6.BackColor = Color.DimGray;
                btnDI7.BackColor = Color.DimGray;
                btnDI8.BackColor = Color.DimGray;

                btnOut1.BackColor = Color.DimGray;
                btnOut2.BackColor = Color.DimGray;
                btnOut3.BackColor = Color.DimGray;
                btnOut4.BackColor = Color.DimGray;

            });
        }

        public bool StartTest()
        {
            byte[] byts;
            ResetView();

            ExitSeltTest();

            byts = ExecuteCANCommand(new byte[] { 0x60 }, 500); //=>RX: 0x0A 0x02 0x63 0x00
            if (byts != null && byts[0] == 0x00)
            {
                //ReadSerialNumber();
                //ReadSoftwareVersion();
                //ReadHardwareVersion();
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
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x02 }, 500);
            if (byts != null && byts.Length >=19)
            {
                string sBuff = "";
                //for (int i = 0; i < 25; i++)
                //{
                //    sBuff += (char)byts[i + 1];
                //}
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

        public string ReadHardwareVersion()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x03 }, 500); 
            if (byts != null && byts.Length == 17)
            {
                string sBuff = "";
                for (int i = 0; i < 16; i++)
                {
                    sBuff += (char)byts[i + 1];
                }
                this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            return "";
        }

        public string ReadSoftwareVersion()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x04, 0x00 }, 500);
            if (byts != null && byts.Length == 4)
            {
                string sBuff = byts[1].ToString() + "." + byts[2].ToString() + "." + byts[3].ToString();
                this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
                this.Invoke((MethodInvoker)delegate { lblSoftwareVersion.Text = "---"; });
            return "";
        }

        public double ReadTemprature()
        {
            byte[] byts;
            ExitSeltTest();
            Global.Delay_ms(100);
            this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x10 }, 500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = byts[1].ToString(); });
                return byts[1];
            }
            else
                this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = "---"; });
            return -1;
        }

        public long ReadACVoltage()
        {
            long retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblACVolt.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x05 }, 500);
            if (byts != null && byts.Length == 7)
            {
                retValue = byts[1] * 0x10000000000 + byts[2] * 0x100000000 + byts[3] * 0x1000000 + byts[4] * 0x10000 + byts[5] * 0x100 + byts[6];
                this.Invoke((MethodInvoker)delegate 
                { 
                    lblACVolt.Text = ((retValue >> 32) & 0xFFFF).ToString() + "|" + ((retValue >> 16) & 0xFFFF).ToString() + "|" + (retValue & 0xFFFF).ToString(); 
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { lblACVolt.Text = "---"; });
            return retValue;
        }

        public long ReadDCVoltage()
        {
            long retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblDCVolt.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x06 }, 500);
            if (byts != null && byts.Length == 5)
            {
                retValue = byts[1] * 0x1000000 + byts[2] * 0x10000 + byts[3] * 0x100 + byts[4];
                this.Invoke((MethodInvoker)delegate
                {
                    lblDCVolt.Text = ((retValue >> 16) & 0xFFFF).ToString() + "|" + (retValue & 0xFFFF).ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { lblDCVolt.Text = "---"; });
            return retValue;
        }

        //public int ReadHV1InsulationInternal()
        //{
        //    int retValue = -1;
        //    byte[] byts;
        //    this.Invoke((MethodInvoker)delegate { label32.Text = ""; });
        //    byts = ExecuteCANCommand(new byte[] { 0x62, 0x07 }, 5000);
        //    if (byts != null && byts.Length == 5)
        //    {
        //        retValue = byts[1] * 0x100 + byts[2];
        //        this.Invoke((MethodInvoker)delegate
        //        {
        //            label32.Text = retValue.ToString();
        //        });
        //    }
        //    else
        //        this.Invoke((MethodInvoker)delegate { label32.Text = "---"; });
        //    return retValue;
        //}

        public int[] ReadHV1InsulationInternal()
        {
            int retValue = -1;
            int retValue2 = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label32.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x07 }, 5000);
            if (byts != null && byts.Length == 5)
            {
                retValue = byts[1] * 0x100 + byts[2];
                retValue2 = byts[3] * 0x100 + byts[4];
                this.Invoke((MethodInvoker)delegate
                {
                    label32.Text = retValue.ToString() + "|" + retValue2.ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label32.Text = "---"; });
            return new int[2] { retValue,retValue2};
        }

        public int ReadHV1LowInsulation()
        {
            int retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label34.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x08 }, 5000);
            if (byts != null && byts.Length == 3)
            {
                retValue = byts[1] * 0x100 + byts[2];
                this.Invoke((MethodInvoker)delegate
                {
                    label34.Text = retValue.ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label34.Text = "---"; });
            return retValue;
        }

        public int ReadHV1HighInsulation()
        {
            int retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label39.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x09 }, 5000);
            if (byts != null && byts.Length == 3)
            {
                retValue = byts[1] * 0x100 + byts[2];
                this.Invoke((MethodInvoker)delegate
                {
                    label39.Text = retValue.ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label39.Text = "---"; });
            return retValue;
        }

        //public int ReadHV2InsulationInternal()
        //{
        //    int retValue = -1;
        //    byte[] byts;
        //    this.Invoke((MethodInvoker)delegate { label36.Text = ""; });
        //    byts = ExecuteCANCommand(new byte[] { 0x62, 0x0A }, 2500);
        //    if (byts != null && byts.Length == 5)
        //    {
        //        retValue = byts[1] * 0x100 + byts[2];
        //        this.Invoke((MethodInvoker)delegate
        //        {
        //            label36.Text = retValue.ToString();
        //        });
        //    }
        //    else
        //        this.Invoke((MethodInvoker)delegate { label36.Text = "---"; });
        //    return retValue;
        //}

        public int[] ReadHV2InsulationInternal()
        {
            int retValue = -1;
            int retValue2 = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label36.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0A }, 5000);
            if (byts != null && byts.Length == 5)
            {
                retValue = byts[1] * 0x100 + byts[2];
                retValue2 = byts[3] * 0x100 + byts[4];
                this.Invoke((MethodInvoker)delegate
                {
                    label36.Text = retValue.ToString() + "|" + retValue2.ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label36.Text = "---"; });
            return new int[2] { retValue,retValue2};
        }

        public int ReadHV2LowInsulation()
        {
            int retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label35.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0B }, 5000);
            if (byts != null && byts.Length == 3)
            {
                retValue = byts[1] * 0x100 + byts[2];
                this.Invoke((MethodInvoker)delegate
                {
                    label35.Text = retValue.ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label35.Text = "---"; });
            return retValue;
        }

        public int ReadHV2HighInsulation()
        {
            int retValue = -1;
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { label38.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0C }, 5000);
            if (byts != null && byts.Length == 3)
            {
                retValue = byts[1] * 0x100 + byts[2];
                this.Invoke((MethodInvoker)delegate
                {
                    label38.Text = retValue.ToString();
                });
            }
            else
                this.Invoke((MethodInvoker)delegate { label38.Text = "---"; });
            return retValue;
        }

        public bool FlashTest()
        {
            byte[] byts;
            this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0F }, 500);
            if (byts != null && byts.Length == 2)
            {
                this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = (byts[1] == 0) ? "Pass" : "Fail"; });
                return (byts[1] == 0);
            }
            else
                this.Invoke((MethodInvoker)delegate { lblFlashTest.Text = "---"; });
            return false;
        }

        public bool RS485Test()
        {
            byte[] byts;
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x11 }, 500);
            if (byts != null && byts.Length == 2)
            {
                return (byts[1] == 1);
            }
            return false;
        }

        public bool UARTDebugTest()
        {
            byte[] byts;
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x12 }, 500);
            if (byts != null && byts.Length == 2)
            {
                return (byts[1] == 1);
            }
            return false;
        }

        public bool SetOutput(int index, bool state, int ms_delay)
        {
            byte[] byts = ExecuteCANCommand(new byte[] { 0x62, 0x0D, (byte)(index-1), (byte)(state ? 1 : 0) }, 200);
            if (byts != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (index == 1) 
                        btnOut1.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 2)
                        btnOut2.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 3)
                        btnOut3.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 4)
                        btnOut4.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 5)
                        btnOut5.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 6)
                        btnOut6.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 7)
                        btnOut7.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 8)
                        btnOut8.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 9)
                        btnOut9.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 10)
                        btnOut10.BackColor = state ? Color.Red : Color.DimGray;
                    else if (index == 11)
                        btnOut11.BackColor = state ? Color.Red : Color.DimGray;
                });
                if (ms_delay > 0)
                    Conv.delay_ms(ms_delay);
                return true;
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return false;
        }

        public bool SetOutput(int index, bool state)
        {
            return SetOutput(index, state, 0);
        }
        
        private void lblTmprValue_DoubleClick(object sender, EventArgs e)
        {
            ReadTemprature();
        }

        public bool SetAllOutput(bool state, int ms_delay)
        {
            //Tắt tất cả rơ le
            for (int i = 0; i < 11; i++)
            {
                SetOutput(i + 1, state);
            }
            if (ms_delay > 0)
                Conv.delay_ms(ms_delay);
            return false;
        }

        public bool SetAllOutput(bool state)
        {
            return SetAllOutput(state, 0);
        }
        
        public int ReadAllInput()
        {
            byte[] byts;
            //this.Invoke((MethodInvoker)delegate { lblTmprValue.Text = ""; });
            byts = ExecuteCANCommand(new byte[] { 0x62, 0x0E }, 500);
            if (byts != null && byts.Length == 10)
            {
                int ret = 0;
                this.Invoke((MethodInvoker)delegate 
                { 
                    btnDI1.BackColor = (byts[1] == 0) ? Color.Red : Color.DimGray;
                    btnDI2.BackColor = (byts[2] == 0) ? Color.Red : Color.DimGray;
                    btnDI3.BackColor = (byts[3] == 0) ? Color.Red : Color.DimGray;
                    btnDI4.BackColor = (byts[4] == 0) ? Color.Red : Color.DimGray;
                    btnDI5.BackColor = (byts[5] == 0) ? Color.Red : Color.DimGray;
                    btnDI6.BackColor = (byts[6] == 0) ? Color.Red : Color.DimGray;
                    btnDI7.BackColor = (byts[7] == 0) ? Color.Red : Color.DimGray;
                    btnDI8.BackColor = (byts[8] == 0) ? Color.Red : Color.DimGray;
                    btnDI9.BackColor = (byts[9] == 0) ? Color.Red : Color.DimGray;
                });
                if (byts[9] == 0) ret += 0b100000000;
                if (byts[8] == 0) ret += 0b10000000;
                if (byts[7] == 0) ret += 0b1000000;
                if (byts[6] == 0) ret += 0b100000;
                if (byts[5] == 0) ret += 0b10000;
                if (byts[4] == 0) ret += 0b1000;
                if (byts[3] == 0) ret += 0b100;
                if (byts[2] == 0) ret += 0b10;
                if (byts[1] == 0) ret += 0b1;
                return ret;
            }
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

        void ThreadFrameProcess(byte[] rxData)
        {
            if (rxData[3] == 0x06 || rxData[3] == 0x08) //OVER_VOLTAGE_ALERT || UNDER_VOLTAGE_ALERT
            {
                btnOut1.BackColor = btnOut2.BackColor = btnOut3.BackColor = btnOut4.BackColor = Color.DimGray;
            }
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

        private void btnSetAllOutput_Click(object sender, EventArgs e)
        {
            SetAllOutput(btnOut1.BackColor == Color.DimGray);
        }

        private void btnOut1_Click(object sender, EventArgs e)
        {
            int state = btnOut1.BackColor == Color.DimGray ? 1 : 0;
            if ((Button)sender == btnOut1)
            {
                SetOutput(1, btnOut1.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut2)
            {
                SetOutput(2, btnOut2.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut3)
            {
                SetOutput(3, btnOut3.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut4)
            {
                SetOutput(4, btnOut4.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut5)
            {
                SetOutput(5, btnOut5.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut6)
            {
                SetOutput(6, btnOut6.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut7)
            {
                SetOutput(7, btnOut7.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut8)
            {
                SetOutput(8, btnOut8.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut9)
            {
                SetOutput(9, btnOut9.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut10)
            {
                SetOutput(10, btnOut10.BackColor == Color.DimGray);
            }
            else if ((Button)sender == btnOut11)
            {
                SetOutput(11, btnOut11.BackColor == Color.DimGray);
            }
        }

        private void label2_DoubleClick(object sender, EventArgs e)
        {
            ReadHardwareVersion();
        }

        private void label45_DoubleClick(object sender, EventArgs e)
        {
            ReadACVoltage();
        }

        private void label22_DoubleClick(object sender, EventArgs e)
        {
            ReadDCVoltage();
        }

        private void btnReadAllDI_Click(object sender, EventArgs e)
        {
            ReadAllInput();
        }

        private void label33_DoubleClick(object sender, EventArgs e)
        {
            ReadHV1InsulationInternal(); 
            ReadHV1LowInsulation();
            ReadHV1HighInsulation();
        }

        private void label37_DoubleClick(object sender, EventArgs e)
        {
            ReadHV2InsulationInternal(); 
            ReadHV2LowInsulation();
            ReadHV2HighInsulation();
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
                                            can_serial = Conv.bytesToString(mReadBoardInfo.str_Serial_Num);
                                            this.Invoke((MethodInvoker)delegate { lblCANSerial.Text = can_serial; });
                                            comm_started = true;
                                        }
                                        else can_serial = "---";
                                        comm_started = true; //DEBUG
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
                                            //if (mMsg.ID == CAN_ADDRESS_DUT && rxData[0] == CAN_ADDRESS_SOFT && mMsg.DataLen > 3 && rxData[2] == CAN_CMDID_RESPONSE) ThreadFrameProcess(rxData);
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
                    
                    //comm_started = commOK;
                    //Cap nhat hien thi:
                    this.Invoke((MethodInvoker)delegate
                    {
                        //lblHardwareVersion.Text = tony_version;
                        //lblHardVersion.Text = hardware_version;

                        if (commOK)
                        {
                            //ledVOLT.Value = u_value.ToString("0.00");
                            //ledAMPE.Value = i_value.ToString("0.000");
                            //ledKWH.Value = e_value.ToString("0.0");
                        }
                        else
                        {
                            //ledVOLT.Value = "-----";
                            //ledAMPE.Value = "-----";
                            //ledKWH.Value = "-------";
                        }
                        
                    });
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


        public void ExitSeltTest()
        {
            ExecuteCANCommandExitSeltTest(1512704, "00 00 00 00 00 00 00 00");
            ExecuteCANCommandExitSeltTest(1512448, "11 11 00 00 00 00 00 01");
            ExecuteCANCommandExitSeltTest(1512192, "11 11 00 00 00 00 00 00");
            Global.Delay_ms(500);
            ExecuteCANCommandExitSeltTest(1512192, "00 00 00 00 00 00 00 00");
        }

        private void ExecuteCANCommandExitSeltTest(uint FrameID,string cmd_hex)
        {
            
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
                        
                        txFrame.SendType = 0;
                        txFrame.data = new byte[8];
                        txFrame.Reserved = new byte[3];
                        txFrame.ID = FrameID; //Địa chỉ của PC
                        txFrame.ExternFlag = 1;
                        txFrame.RemoteFlag = 0;
                        

                        txFrame.DataLen = (bytes.Length > 8) ? (byte)8 : (byte)bytes.Length;
                        for (int i = 0; i < txFrame.DataLen; i++)
                        {
                            txFrame.data[i] = bytes[i];
                        }

                        this.Invoke((MethodInvoker)delegate
                        {
                            txtLog.AppendText("\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "][TX][" + txFrame.DataLen.ToString() + "][" + cmd_hex.Replace(" ", "") + "][" + txFrame.ExternFlag.ToString() + "][" + txFrame.RemoteFlag.ToString() + "]");
                        });
                        //Conv.delay_ms(10);
                        if (ECANDLL.Transmit(1, 0, 0, ref txFrame, (ushort)mLen) == ECANStatus.STATUS_OK)
                        {
                            
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
