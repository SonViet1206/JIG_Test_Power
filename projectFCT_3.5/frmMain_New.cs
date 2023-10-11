using DefaultNS;
using Microsoft.VisualBasic;
using projectRU_New;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projectFCT_3._5
{
    public partial class frmMain : Form
    {
        bool thread_ended = true;
        bool thread_started = false;
        Thread threadCOMM;
        bool giaodong1 = false;
        bool giaodong = false;

        private TimeSpan lastCPU_TS;
        private System.DateTime lastCPU_Time = DateTime.Now;

        string DataLogPath;

        string SerialComPortPowerboard = "COM4";
        string SerialComPortRelayboard = "COM5";
        string SerialComPortMeasureboard = "COM3";
        bool stepFishned=true;

        private int TotalCount = 0;
        private int PassCount = 0;
        private int LastCountTime = 0;
        DCL6104 dCL6104;
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            IniFile iniFile = new IniFile(Application.StartupPath + "\\AppConfig.ini");
            
            Global.delta = double.Parse(iniFile.IniReadValue("SaiSo", "SAISO", "0.1"));
            panelConnection.Visible = false;
            ReloadCounter();
            //byte[] test = new byte[] { 0x68,0x91,0x36,0x00,0x53,0x41,0x18,0x68,0x01,0x02,0x52,0xC3 };
            //MessageBox.Show(LRC(test).ToString("X2"));

            lbProgramName.Text += " - V" + Global.PctoolVersion;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");
            string portDCL6104 = ini.IniReadValue("COMMUNICATION", "DCL6104ComPort");
            dCL6104 = new DCL6104(portDCL6104);
            AddLogWindow("Khởi động chương trình");
            ReadConFig();

            ReloadMapdataInit();
            ReInitBoardType(); //COM2
            btnReload_Click(null, null);

         
            DMM.Start();

            btnMaximize_Click(null, null);
            //cboOperator.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Operator'"));
            numTemprature.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Temperature'"));

            Conv.delay_ms(2000);

            if (!powerBoard1.CommState || !relayBoards1.CommState)
            {
               
            }
            else
            {
                AddLogWindow("Hoàn tất khởi động chương trình");
            }

            timer1s.Enabled = true;

        }

        List<Point> lstPositionMapNormal = new List<Point>();
        List<Point> lstPositionMapMax = new List<Point>();
        List<Point> lstPositionMapMaxWithPanelConnection = new List<Point>();
        List<Point> lstPositionMapNormalWithPanelConnection = new List<Point>();

        private void RefreshMapData()
        {
            for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
            {
                pnBlockBorder1.Controls[i].BackColor = Color.White;
            }
        }

        private void ReloadMapdataInit()
        {
            string mapdata = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='MapData'"));
            string[] arrMapData = mapdata.Split(';');
            if (arrMapData.Length >= 2)
            {
                string[] arrMapdata0 = arrMapData[0].Split('x');
                if (arrMapdata0.Length == 2)
                {
                    int row = Conv.atoi32(arrMapdata0[0]);
                    int cloumns = Conv.atoi32(arrMapdata0[1]);

                    string mapvalue = arrMapData[1];

                    ReloadMapInit(row, cloumns, mapvalue);
                }
            }
        }


        private void ReloadMapInit(int RowNumber, int ColumnsNumber, string mapvalue)
        {
            pnBlockBorder1.SizeChanged -= pnBlockBorder1_SizeChanged;


            int LeftOfset = 40;
            int TopOfset = 10;
            int RightOfset = 10;
            int BottomOfset = 10;

            

            lstPositionMapNormal.Clear();
            lstPositionMapMax.Clear();
            lstPositionMapMaxWithPanelConnection.Clear();
            lstPositionMapNormalWithPanelConnection.Clear();

            string[] arrMapvalue = mapvalue.Split(',');

            this.WindowState = FormWindowState.Maximized;
            panelConnection.Visible = false;

            pnBlockBorder1.Controls.Clear();
            double deltaX = (double)(pnBlockBorder1.Width - 120) / (ColumnsNumber - 1);
            double deltaY = (double)(pnBlockBorder1.Height - 40) / (RowNumber - 1);
            if (ColumnsNumber == 1) deltaX = 0;
            if (RowNumber == 1) deltaY = 0;

            for (int i = 0; i < RowNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    var txt = new TextBox();
                    txt.Size = new Size(45, 20);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.TextAlign = HorizontalAlignment.Center;
                    txt.BackColor = Color.White;
                    txt.Font = new Font("Microsoft Sans Serif", 9F);
                    txt.Name = "lbViewPoint" + (ColumnsNumber * i + j + 1).ToString();
                    txt.Text = arrMapvalue.Length > (ColumnsNumber * i + j) ? arrMapvalue[ColumnsNumber * i + j] : "";
                    //label.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    //new ToolTip().SetToolTip(label, "Đây là số " + label.Text + " ban nhé :D");
                    //button.Click += button_Click;//function
                    //label.Click += button_Click;

                    lstPositionMapMax.Add(new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY)));
                    txt.Location = new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY));
                    pnBlockBorder1.Controls.Add(txt);
                }

            }

            this.WindowState = FormWindowState.Maximized;
            panelConnection.Visible = true;

            pnBlockBorder1.Controls.Clear();
            deltaX = (double)(pnBlockBorder1.Width - 120) / (ColumnsNumber - 1);
            deltaY = (double)(pnBlockBorder1.Height - 40) / (RowNumber - 1);
            for (int i = 0; i < RowNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    var txt = new TextBox();
                    txt.Size = new Size(45, 20);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.TextAlign = HorizontalAlignment.Center;
                    txt.BackColor = Color.White;
                    txt.Font = new Font("Microsoft Sans Serif", 9F);
                    txt.Name = "lbViewPoint" + (ColumnsNumber * i + j + 1).ToString();
                    txt.Text = arrMapvalue.Length > (ColumnsNumber * i + j) ? arrMapvalue[ColumnsNumber * i + j] : "";
                    //label.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    //new ToolTip().SetToolTip(label, "Đây là số " + label.Text + " ban nhé :D");
                    //button.Click += button_Click;//function
                    //label.Click += button_Click;

                    lstPositionMapMaxWithPanelConnection.Add(new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY)));
                    txt.Location = new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY));
                    pnBlockBorder1.Controls.Add(txt);
                }

            }

            this.WindowState = FormWindowState.Normal;
            panelConnection.Visible = true;

            pnBlockBorder1.Controls.Clear();
            deltaX = (double)(pnBlockBorder1.Width - 120) / (ColumnsNumber - 1);
            deltaY = (double)(pnBlockBorder1.Height - 40) / (RowNumber - 1);
            if (ColumnsNumber == 1) deltaX = 0;
            if (RowNumber == 1) deltaY = 0;

            for (int i = 0; i < RowNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    var txt = new TextBox();
                    txt.Size = new Size(45, 20);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.TextAlign = HorizontalAlignment.Center;
                    txt.BackColor = Color.White;
                    txt.Font = new Font("Microsoft Sans Serif", 9F);
                    txt.Name = "lbViewPoint" + (ColumnsNumber * i + j + 1).ToString();
                    txt.Text = arrMapvalue.Length > (ColumnsNumber * i + j) ? arrMapvalue[ColumnsNumber * i + j] : "";
                    //label.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    //new ToolTip().SetToolTip(label, "Đây là số " + label.Text + " ban nhé :D");
                    //button.Click += button_Click;//function
                    //label.Click += button_Click;

                    lstPositionMapNormalWithPanelConnection.Add(new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY)));
                    txt.Location = new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY));
                    pnBlockBorder1.Controls.Add(txt);
                }

            }
            this.WindowState = FormWindowState.Normal;
            panelConnection.Visible = false;

            pnBlockBorder1.Controls.Clear();
            deltaX = (double)(pnBlockBorder1.Width - 120) / (ColumnsNumber - 1);
            deltaY = (double)(pnBlockBorder1.Height - 40) / (RowNumber - 1);

            if (ColumnsNumber == 1) deltaX = 0;
            if (RowNumber == 1) deltaY = 0;

            for (int i = 0; i < RowNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    var txt = new TextBox();
                    txt.Size = new Size(45, 20);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.TextAlign = HorizontalAlignment.Center;
                    txt.BackColor = Color.White;
                    txt.Font = new Font("Microsoft Sans Serif", 9F);
                    txt.Name = "lbViewPoint" + (ColumnsNumber * i + j + 1).ToString();
                    txt.Text = arrMapvalue.Length > (ColumnsNumber * i + j) ? arrMapvalue[ColumnsNumber * i + j] : "";
                    //label.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    //new ToolTip().SetToolTip(label, "Đây là số " + label.Text + " ban nhé :D");
                    //button.Click += button_Click;//function
                    //label.Click += button_Click;

                    lstPositionMapNormal.Add(new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY)));
                    txt.Location = new Point(LeftOfset + (int)(j * deltaX), TopOfset + (int)(i * deltaY));
                    pnBlockBorder1.Controls.Add(txt);
                }

            }
            pnBlockBorder1.SizeChanged += pnBlockBorder1_SizeChanged;
        }


        private void pnBlockBorder1_SizeChanged(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Maximized)
            {
                if (panelConnection.Visible)
                {
                    for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
                    {
                        if (pnBlockBorder1.Controls[i].Name == $"lbViewPoint" + (i + 1).ToString())
                        {
                            pnBlockBorder1.Controls[i].Location = new Point(lstPositionMapMaxWithPanelConnection[i].X, lstPositionMapMaxWithPanelConnection[i].Y);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
                    {
                        if (pnBlockBorder1.Controls[i].Name == $"lbViewPoint" + (i + 1).ToString())
                        {
                            pnBlockBorder1.Controls[i].Location = new Point(lstPositionMapMax[i].X, lstPositionMapMax[i].Y);
                        }
                    }
                }
            }
            else if(this.WindowState == FormWindowState.Normal)
            {
                if (!panelConnection.Visible)
                {
                    for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
                    {
                        if (pnBlockBorder1.Controls[i].Name == $"lbViewPoint" + (i + 1).ToString())
                        {
                            pnBlockBorder1.Controls[i].Location = new Point(lstPositionMapNormal[i].X, lstPositionMapNormal[i].Y);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
                    {
                        if (pnBlockBorder1.Controls[i].Name == $"lbViewPoint" + (i + 1).ToString())
                        {
                            pnBlockBorder1.Controls[i].Location = new Point(lstPositionMapNormalWithPanelConnection[i].X, lstPositionMapNormalWithPanelConnection[i].Y);
                        }
                    }
                }
            }
            
            lblSize.Text = $"{pnBlockBorder1.Width};{pnBlockBorder1.Height}";
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            dCL6104.OFF_LOAD();
            dCL6104.Disconnect();
            DMM.Stop();
           // powerBoard1.Stop();
            relayBoards1.Stop();
            if (threadCOMM != null)
            {
                if (threadCOMM.IsAlive) threadCOMM.Abort();
                threadCOMM = null;
            }
            //btnCommStop_Click(null, null);
            timer1s.Enabled = false;
            if (cboOperator.Text != "")
                SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + cboOperator.Text + "' WHERE ItemName='Operator'");
            if (numTemprature.Text != "")
                SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numTemprature.Text + "' WHERE ItemName='Temperature'");
            AddLogWindow("Kết thúc chương trình");
        }

        void ReInitBoardType()
        {
            BoardType = Conv.atoi32(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='BoardType'"), 0);
            if (BoardType > -1 && BoardType < 2)
            {
                lblCaption.Text = Global.TITLES[BoardType];
                string[] rls = Global.RELAY_BOARDS[BoardType].Split('|');
                if (rls.Length > 0)
                {
                    relayBoards1.Stop();
                    relayBoards1.Start(SerialComPortRelayboard, rls[0]);
                    //2|DO1:TP16_GND|DO2:TP1_5V_IN|DO3:TP12_3V3|DO4:TP13_5V_VDD_TX|DO5:TP14_5V_VMID|DO6:TP15_5V_VDDA|DO7:FIRMWARE_1|DO8>FIRMWARE_2
                    for (int i = 1; i < rls.Length; i++)
                    {
                        string[] r = rls[i].Split(':');
                        if (r.Length > 1 && r[0][0] == 'D')
                        {
                            if (r[0][1] == 'O')
                                relayBoards1.SetDOCaption(Conv.atoi32(r[0].Replace("DO", "")), r[1]);
                            else if (r[0][1] == 'I')
                                relayBoards1.SetDICaption(Conv.atoi32(r[0].Replace("DI", "")), r[1]);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy thông tin BOARD mạch phù hợp","MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        int BoardType = 0;
        private void btnReload_Click(object sender, EventArgs e)
        {
            string strSQL;
            DataTable dtab;
            DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
            if (!Directory.Exists(DataLogPath)) DataLogPath = Application.StartupPath + @"\DataLogs";
            dgvTestList.Rows.Clear();

            if (BoardType > -1 && BoardType < 7)
            {
                strSQL = "SELECT * FROM tbl_test_list WHERE jig_name='" + Global.BOARD_NAMES[BoardType] + "' AND test_enable = 1 ORDER BY test_order";
                dtab = SQLite.ExecuteDataTable(strSQL);
                if (dtab != null)
                {
                    dgvTestList.Rows.Clear();
                    for (int r = 0; r < dtab.Rows.Count; r++)
                    {
                        dgvTestList.Rows.Add();

                        dgvTestList.Rows[r].Cells["TestOrder"].Value = r + 1;
                        dgvTestList.Rows[r].Cells["TestName"].Value = dtab.Rows[r]["test_name"];
                        dgvTestList.Rows[r].Cells["Description"].Value = dtab.Rows[r]["description"];
                        dgvTestList.Rows[r].Cells["MinValue"].Value = dtab.Rows[r]["min_value"];
                        dgvTestList.Rows[r].Cells["MaxValue"].Value = dtab.Rows[r]["max_value"];
                        dgvTestList.Rows[r].Cells["Unit"].Value = dtab.Rows[r]["unit"];
                        dgvTestList.Rows[r].Cells["Unit"].Value = dtab.Rows[r]["unit"];
                        dgvTestList.Rows[r].Cells["PopupCheck"].Value = dtab.Rows[r]["popup_check"];
                        dgvTestList.Rows[r].Cells["PopupMessage"].Value = dtab.Rows[r]["popup_message"];
                        dgvTestList.Rows[r].Cells["PopupImage"].Value = dtab.Rows[r]["popup_image"];

                        dgvTestList.Rows[r].Cells["DMMFunction"].Value = dtab.Rows[r]["dmm_function"];
                        dgvTestList.Rows[r].Cells["DMMRange"].Value = dtab.Rows[r]["dmm_range"];
                        dgvTestList.Rows[r].Cells["RelayOutputs"].Value = dtab.Rows[r]["relay_outputs"];
                        dgvTestList.Rows[r].Cells["StabMsTime"].Value = dtab.Rows[r]["stab_mstime"];
                        dgvTestList.Rows[r].Cells["MeasMsTime"].Value = dtab.Rows[r]["meas_mstime"];

                        dgvTestList.Rows[r].Cells["PcbIndex"].Value = dtab.Rows[r]["pcb_index"];

                    }
                }
                txtTotalSteep.Text = dgvTestList.Rows.Count.ToString();
            }
            RefreshMapData();
        }
      
        private void AddLogWindow(string mess)
        {
            //lstLogWindow.Items.Insert(0, "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "] " + mess);
            if (lstLogWindow.Items.Count > 10000) lstLogWindow.Items.RemoveAt(0);
            lstLogWindow.Items.Add("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "] " + mess);
            lstLogWindow.TopIndex = lstLogWindow.Items.Count - 1;
            lstLogWindow.SelectedIndex = lstLogWindow.Items.Count - 1;
            Global.WriteLogFile(mess);
        }

        bool isTestOffline = false;
        string MachineName = "";

        private bool ReadConFig()
        {
            bool res = true;
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            IniFile ini = new IniFile(path + "\\AppConfig.ini");

            isTestOffline = ini.IniReadValue("API_CONNECTION", "isTestOffLine", "1") == "1" ? true : false;
            lbMeshStatus.BackColor = isTestOffline ? Color.DarkSlateGray : Color.Lime;
            MachineName = ini.IniReadValue("API_CONNECTION", "MachineName", "TEST01_PT_1");
            RestApiHelper.InitGlobalVarial();


            //MessageBox.Show(MaterialCodeSplitString);

            SerialComPortPowerboard = ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM4");
            SerialComPortRelayboard = ini.IniReadValue("COMMUNICATION", "RelaysBoardCommPort", "COM5");
            //SerialComPortMeasureboard = ini.IniReadValue("COMMUNICATION", "MeasureBoardCommPort", "COM3");


            return res;
        }

        

        private void ReloadCounter()
        {
            string[] tempstr = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password3'")).Split('|');
            if (tempstr.Length >= 3)
            {
                TotalCount = Conv.atoi32(tempstr[0]);
                PassCount = Conv.atoi32(tempstr[1]);
                LastCountTime = Conv.atoi32(tempstr[2]);

                if (LastCountTime != Conv.atoi32(DateTime.Now.Day))
                {
                    TotalCount = 0;
                    PassCount = 0;
                    LastCountTime = Conv.atoi32(DateTime.Now.Day);
                    SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + LastCountTime + "' WHERE ItemName='Password3'");
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (thread_ended)
            {
                pnBlockBorder1.SizeChanged -= pnBlockBorder1_SizeChanged;
                this.Close();
            }
                
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }



        private void lbMonitorStatus_Click(object sender, EventArgs e)
        {
            if (!panelConnection.Visible)
            {
                //string password = Global.PasswordInput();
                //if (password.Length > 0)
                //{
                //    if (password == Conv.atos(SQLiteFun.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
                //    {
                //        panelConnection.Visible = true;
                //        //panelLog.Visible = false;
                //        //jig1.Visible = true;
                //        dut1.SetViewDebug(true);
                //    }
                //    else MessageBox.Show("Sai mật khẩu.", "LỖI THAO TÁC", MessageBoxButtons.OK);

                    
                //}
                panelConnection.Visible = true;
                dgvTestList.Columns["Description"].Visible = false;
            }
            else
            {
                dgvTestList.Columns["Description"].Visible = true;
                panelConnection.Visible = false;
                //panelLog.Visible = true;
            }
        }

        DateTime startTime = DateTime.Now;
        int count1s = 0;
        private void timer1s_Tick(object sender, EventArgs e)
        {
            
            lbPassRate.Text = PassCount.ToString() + "/" + TotalCount.ToString();
            count1s++;

           
            if (txtSerialNo.Enabled == false)
            {
                TimeSpan ts = DateTime.Now - startTime;
                lblTestTime.Text = ((int)(ts.TotalSeconds / 60)).ToString("00") + ":" + ((int)(ts.TotalSeconds % 60)).ToString("00");
            }

            tslbDateTime.Text = DateTime.Now.ToString("HH:mm:ss");

            //Thong ke tai nguyen da su dung:
            tslbThreadCount.Text = System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString();

            TimeSpan currCPU_TS = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime - lastCPU_TS;
            lastCPU_TS = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
            TimeSpan currTotal_TS = DateTime.Now - lastCPU_Time;
            lastCPU_Time = DateTime.Now;
            tslbCPUUsage.Text = (((double)currCPU_TS.Ticks / System.Environment.ProcessorCount) * 100 / currTotal_TS.Ticks).ToString("0.0") + "%";
            tslbRAMUsage.Text = (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1048576).ToString("0.0") + "MB";

            if (relayBoards1.CommState)
            {
                lbStatus.Text = "READY";
                lbStatus.BackColor = Global.COLOR_OK;
            }
            else
            {
                if (!relayBoards1.CommState && lbStatus.BackColor != Global.COLOR_NG)
                {
                    AddLogWindow("Kết nối cổng COM RELAY BOARD không thành công! Hãy kiểm tra lại kết nối!");
                }

              
                lbStatus.Text = "ERROR";
                lbStatus.BackColor = Global.COLOR_NG;

            }

           
            
        }

        private void btnReady_Click(object sender, EventArgs e)
        {
            stepFishned = true;
            if (lbStatus.BackColor == Global.COLOR_OK )
            {
                if (cboOperator.Text == "")
                {
                    MessageBox.Show("Chưa nhập mã người vận hành", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                //if (!cboOperator.Text.StartsWith("OP"))
                //{
                //    MessageBox.Show("Mã người vận hành không đúng định dạng!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                if (txtSerialNo.Text != "")
                {
                    AddLogWindow("Bắt đầu kiểm");
                    AddLogWindow("Operator: " + cboOperator.Text);
                    AddLogWindow("Serial Number: " + txtSerialNo.Text);
                    btnReload_Click(null, null);
                    startTime = DateTime.Now;
                
                    EnterTestSteep(0);             
                }
                else
                {
                    MessageBox.Show("Chưa nhập số chế tạo","MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Lỗi kết nối đến thiết bị trên JIG!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        enum STATUS
        {
            Ready, // = 0;
            Start,
            Stopping,
            Stop
        }
        private void SetControlStatus(STATUS _status)
        {
            cboOperator.Enabled = false;
            txtSerialNo.Enabled = false;
            btnReady.Enabled = false;
            btnStop.Enabled = false;
            btnTest.Enabled = false;
            btnNext.Enabled = false;
            btnEdit.Enabled = false;
            btnReload.Enabled = false;

            picTesting.Visible = false;
            btnSave.Enabled = false;
            btnConfig.Enabled = false;

            switch (_status)
            {
                case STATUS.Ready:

                    relayBoards1.SetAllState(false);
                    cboOperator.Enabled = btnReady.Enabled = btnConfig.Enabled = btnReload.Enabled = true;
                    btnEdit.Enabled = true;
                    btnStop.Text = "STOP";

                    txtSerialNo.Enabled = true;
                    txtSerialNo.Text = "";
                    txtSerialNo.Focus();
                    break;
                case STATUS.Start:
                    btnStop.Text = "PAUSE";
                    picTesting.Visible = btnStop.Enabled = true;
                    break;
                case STATUS.Stopping:
                    btnStop.Text = "...";
                    break;
                case STATUS.Stop:
                    btnStop.Text = "STOP";
                    btnSave.Enabled = btnStop.Enabled = btnTest.Enabled = btnNext.Enabled = true;

                    
                    relayBoards1.SetAllState(false);
                    break;
                default:
                    break;
            }


        }


        string[] LastRelays = new string[0];
        private double delta=0;

        private void EnterTestSteep(int test_steep)
        {
            if (test_steep > -1 && test_steep < dgvTestList.Rows.Count)
            {
                SetControlStatus(STATUS.Start);
                thread_ended = false;
                thread_started = true;
                int retryCount = 4;
                try
                {
                    while (thread_started)
                    {
                        bool testOK = true;
                        bool relayOK = true;
                        string sFUNCTION = Conv.atos(dgvTestList.Rows[test_steep].Cells["DMMFunction"].Value);
                        string sRANGE = Conv.atos(dgvTestList.Rows[test_steep].Cells["DMMRange"].Value);
                        string[] Relays = Conv.atos(dgvTestList.Rows[test_steep].Cells["RelayOutputs"].Value).Split(new char[] { ',' });
                        int ms_time = Conv.atoi32(dgvTestList.Rows[test_steep].Cells["StabMsTime"].Value);
                        int meas_time = Conv.atoi32(dgvTestList.Rows[test_steep].Cells["MeasMsTime"].Value);

                        string test_unit = Conv.atos(dgvTestList.Rows[test_steep].Cells["Unit"].Value);
                        string pcb_index = Conv.atos(dgvTestList.Rows[test_steep].Cells["PcbIndex"].Value);


                        DateTime BeginTest = DateTime.Now;
                        bool powerRequest = false;
                        for (int i = 0; i < Relays.Length; i++)
                        {
                            if (Relays[i] == "0")
                            {
                                powerRequest = true;
                                break;
                            }
                        }


                        dgvTestList.CurrentCell = dgvTestList.Rows[test_steep].Cells[0];

                        dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = Color.LightBlue;
                        dgvTestList.Rows[test_steep].Cells["BeginTest"].Value = BeginTest.ToString("yyyy-MM-dd HH:mm:ss");

                        txtTestSteep.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestOrder"].Value);
                        txtTestName.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestName"].Value);

                        ledResult.Text = "---";
                        ledResult.BackColor = Global.COLOR_OFF;

                        //btnTest.Enabled = false;
                        //btnSave.Enabled = false;

                        AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text + " - " + pcb_index);

                        //ket noi đén DMM


                        //Xử lý tắt rơ le:
                        if (relayOK)
                        {
                            
                            //Tắt các rơ-le của bước trước:
                            relayOK = relayBoards1.SetAllState(false);
                            if (!relayOK) AddLogWindow("Lỗi tắt rơ-le all");
                        }
                        testOK = relayOK;

                        // lấy trạng thái rơ le của bước trước
                        LastRelays = Relays;


                       

                        if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 3) //Nhap ket qua
                        {
                            //Có yêu cầu thay đổi thang đo:
                            if (sFUNCTION != "")
                            {
                                if (testOK)
                                {
                                    testOK = DMM.SetFunction(sFUNCTION);
                                    if (testOK)
                                    {
                                        if (sRANGE != "")
                                        {
                                            testOK = DMM.SetRange(sRANGE);
                                            if (!testOK) AddLogWindow("Lỗi đặt thang đo cho DMM");
                                        }
                                    }
                                    else
                                    {
                                        AddLogWindow("Lỗi đặt chế độ đo cho DMM");
                                    }
                                }
                            }
                            //Xử lý bật rơ le:
                            if (relayOK)
                            {
                                
                                //Bật các rơ-le của bước này:
                                if (Relays[0] != "" && relayOK)
                                {
                                    for (int i = 0; i < Relays.Length; i++)
                                    {
                                        if (Relays[i] != "0")
                                        {

                                            
                                            
                                            bool isFailRes = false;
                                            for(int j=0; j < test_steep; j++)
                                            {
                                                string resultTemp = Conv.atos(dgvTestList.Rows[j].Cells["Result"].Value);
                                                string[] relayTemp = Conv.atos(dgvTestList.Rows[j].Cells["RelayOutputs"].Value).Split(new char[] { ',' });
                                                string pcbIndexTemp = Conv.atos(dgvTestList.Rows[j].Cells["PcbIndex"].Value);
                                                if (pcbIndexTemp==pcb_index && resultTemp != "OK" && relayTemp.Contains(Relays[i]) && powerRequest)
                                                {
                                                    isFailRes = true;
                                                }

                                            }
                                            if(!isFailRes)
                                                relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                            if (!relayOK)
                                            {
                                                AddLogWindow("Lỗi bật rơ-le");
                                            }
                                            //04-04-2021
                                            Conv.delay_ms(1);
                                        }
                                    }
                                }
                               
                            }
                            else //Trong mọi tình huống lỗi tắt cấp nguồn
                            {
                                //SetPower(false);
                                relayBoards1.SetOutput(142,false);
                                relayBoards1.SetOutput(143, false);
                                relayBoards1.SetOutput(144, false);


                            }

                            if (!relayOK) testOK = false;

                            if (testOK)
                            {
                                //Hiển thị form nhập
                                frmShowImage frm = new frmShowImage();
                                frm.lblTestSteep.Text = txtTestSteep.Text;
                                frm.lblTestName.Text = txtTestName.Text;
                                frm.txtMessage.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupMessage"].Value);
                                //frm.picHelp.Image = Image.FromFile(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                                //frm.ShowDialog();
                                frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value),test_steep);
                            }
                            
                        }
                        
                        
                        else if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 4) //Nhap ket qua
                        {
                            
                            //Có yêu cầu thay đổi thang đo:

                            if (sFUNCTION != "")
                            {
                               
                                if (sFUNCTION.CompareTo("F3")==0)// thang đo trở tải cài 20 omh
                                {
                                    dCL6104.Set_Res(20.00);
                                    
                                    
                                }
                                else if (sFUNCTION.CompareTo("F1") == 0)// chế độ đo áp
                                {
                                    dCL6104.Set_Res(20.00);
                                    //dCL6104.MODE("CV");
                                    

                                }
                                else if (sFUNCTION.CompareTo("F5") == 0)// chế độ đo dòng
                                {
                                    dCL6104.Set_Res(20.00);
                                    //dCL6104.MODE("CC");
                                    

                                }
                                else if (sFUNCTION.CompareTo("CP") == 0)
                                {
                                    //dCL6104.Set_Res(10.00);
                                    //dCL6104.ON_LOAD();
                                    
                                }
                                else
                                {
                                    //dCL6104.Set_Res(20.00);
                                }    



                            }
                            //Xử lý bật rơ le:
                            if (relayOK)
                            {

                                //Bật các rơ-le của bước này:
                                if (Relays[0] != "" && relayOK)
                                {
                                    for (int i = 0; i < Relays.Length; i++)
                                    {
                                        if (Relays[i] != "0")
                                        {

                                            bool isFailRes = false;
                                            for (int j = 0; j < test_steep; j++)
                                            {
                                                string resultTemp = Conv.atos(dgvTestList.Rows[j].Cells["Result"].Value);
                                                string[] relayTemp = Conv.atos(dgvTestList.Rows[j].Cells["RelayOutputs"].Value).Split(new char[] { ',' });
                                                string pcbIndexTemp = Conv.atos(dgvTestList.Rows[j].Cells["PcbIndex"].Value);
                                                if (pcbIndexTemp == pcb_index && resultTemp != "OK" && relayTemp.Contains(Relays[i]) && powerRequest)
                                                {
                                                    isFailRes = true;
                                                }
                                            }
                                            if (!isFailRes)
                                                relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                            if (!relayOK)
                                            {
                                                AddLogWindow("Lỗi bật rơ-le");
                                            }
                                            //04-04-2021
                                            Conv.delay_ms(1);
                                        }
                                    }
                                }
                               
                            }
                            else //Trong mọi tình huống lỗi tắt cấp nguồn
                            {
                                //SetPower(false);
                                //relayBoards1.SetAllState(false);
                                relayBoards1.SetOutput(142, false);
                                relayBoards1.SetOutput(143, false);
                                relayBoards1.SetOutput(144, false);

                            }

                            if (!relayOK) testOK = false;
                            if (testOK)
                            {
                                //Lấy kết quả đo và tính toán:
                                if (test_unit != "") //Có yêu cầu đọc từ DMM
                                {
                                    string min = Conv.atos(dgvTestList.Rows[test_steep].Cells["MinValue"].Value);
                                    string max = Conv.atos(dgvTestList.Rows[test_steep].Cells["MaxValue"].Value);
                                    string check_unit = Conv.atos(dgvTestList.Rows[test_steep].Cells["Unit"].Value);
                                    double check_min = Double.MinValue;
                                    double check_max = Double.MaxValue;
                                    double read_value = 0;
                                    double check_factor = 1;

                                    if (check_unit.IndexOf("M") > -1)
                                        check_factor = 1000000;
                                    else if (check_unit.IndexOf("K") > -1)
                                        check_factor = 1000;
                                    else if (check_unit.IndexOf("m") > -1)
                                        check_factor = 0.001;
                                    else if (check_unit.IndexOf("u") > -1)
                                        check_factor = 0.000001;

                                    if (min != "") check_min = Conv.atod(min);
                                    if (max != "") check_max = Conv.atod(max);

                                    //Chờ mạch đo ổn định:
                                    if (ms_time > 0) Conv.delay_ms(ms_time);

                                    testOK = false;
                                    meas_time /= 100;
                                    //Có yêu cầu dừng chờ ổn định:
                                    
                                    if (sFUNCTION.CompareTo("F1") == 0 || sFUNCTION.CompareTo("F5") == 0|| sFUNCTION.CompareTo("CP") == 0)// chế độ đo áp hoạc dòng,xa tu
                                    {
                                        dCL6104.ON_LOAD();
                                        Thread.Sleep(500);
                                        dCL6104.ON_LOAD();
                                        dCL6104.ON_LOAD();
                                        dCL6104.ON_LOAD();

                                    }
                                    List<double> listVal = new List<double>();
                                    do
                                    {
                                        dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");

                                        --meas_time;
                                        Conv.delay_ms(100);

                                        if (sFUNCTION.CompareTo("CP") == 0)
                                        {
                                            testOK = true;
                                            continue;
                                        }
                                        
                                        if ( sFUNCTION.CompareTo("F1") == 0|| sFUNCTION.CompareTo("CP") == 0)//  đo điện áp bằng Dcl,xa tu
                                        {
                                            
                                                read_value = dCL6104.Read_Vol("CURRENT") / check_factor;
                                                
                                           
                                            

                                        }
                                        if (sFUNCTION.CompareTo("F5") == 0)//  đo dòng bằng Dcl
                                        {
                                            
                                            read_value = dCL6104.Read_Current("CURRENT")/ check_factor;

                                        }
                                        if ( sFUNCTION.CompareTo("F3") == 0)//  đo điện áp bằng Dcl
                                        {
                                            
                                            read_value = dCL6104.Read_Res()/ check_factor;

                                        }
                                        dgvTestList.Rows[test_steep].Cells["PValue"].Value =  read_value.ToString();

                                        //Không quan tâm đến dấu nếu đơn vị là điện trở:
                                        if (check_unit.IndexOf("Ω") > -1)
                                        {
                                            read_value = Math.Abs(read_value);
                                            dgvTestList.Rows[test_steep].Cells["PValue"].Value = (DMM.TextValue == "0.L  ") ? "0.L" : read_value.ToString();
                                        }
                                        testOK = (read_value >= check_min && read_value <= check_max);
                                        
                                        //Nếu OK chờ thêm 200ms nưữa đọc kết quả lần cuối
                                        if (testOK && meas_time > 1) meas_time = 1;
                                        
                                    }
                                    while (meas_time > 0);
                                    //if (testOK&& sFUNCTION.CompareTo("F1") == 0)
                                    //{
                                    //    for (int i = 0; i < 30; i++)
                                    //    {
                                    //        read_value = dCL6104.Read_Vol("CURRENT") / check_factor;
                                            
                                    //        listVal.Add(read_value);
                                    //    }
                                    //    for (int i = 0; i < listVal.Count; i++)
                                    //    {
                                    //        for (int j = 0; j < listVal.Count; j++)
                                    //        {
                                    //            if (listVal[i] - listVal[j] > Global.delta || listVal[i] - listVal[j] < -Global.delta)
                                    //            {
                                                    
                                                    
                                    //                label7.ForeColor = Color.Red;
                                    //                testOK = false;
                                    //                dgvTestList.Rows[test_steep].Cells["PValue"].Value =  read_value.ToString()+"~";
                                    //                break;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                }
                            }
                           
                            dCL6104.OFF_LOAD();
                            if (testOK)
                            {
                                //Hiển thị form nhập
                                //frmShowImage frm = new frmShowImage();
                                //frm.lblTestSteep.Text = txtTestSteep.Text;
                                //frm.lblTestName.Text = txtTestName.Text;
                                //frm.txtMessage.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupMessage"].Value);
                                //frm.picHelp.Image = Image.FromFile(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                                //frm.ShowDialog();
                                //frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value), test_steep);
                                dCL6104.OFF_LOAD();
                            }
                           
                        }
                        else if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 2)
                        {
                            frmShowChoice frm = new frmShowChoice();
                            frm.btnSetOK.Enabled = true;
                            frm.btnSetNG.Enabled = true;
                            frm.lblMessage.Text = "Kiểm tra đèn có chuyển màu hay không ?";
                            frm.ShowDialog();
                            testOK = frm.State == 1;
                        }
                        else if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 5) //XaTu
                        {

                          
                            if (relayOK)
                            {

                                //Bật các rơ-le của bước này:
                                if (Relays[0] != "" && relayOK)
                                {
                                    for (int i = 0; i < Relays.Length; i++)
                                    {
                                        if (Relays[i] != "0")
                                        {

                                            
                                            bool isFailRes = false;
                                            for (int j = 0; j < test_steep; j++)
                                            {
                                                string resultTemp = Conv.atos(dgvTestList.Rows[j].Cells["Result"].Value);
                                                string[] relayTemp = Conv.atos(dgvTestList.Rows[j].Cells["RelayOutputs"].Value).Split(new char[] { ',' });
                                                string pcbIndexTemp = Conv.atos(dgvTestList.Rows[j].Cells["PcbIndex"].Value);
                                                if (pcbIndexTemp == pcb_index && resultTemp != "OK" && relayTemp.Contains(Relays[i]) && powerRequest)
                                                {
                                                    isFailRes = true;
                                                }
                                            }
                                            if (!isFailRes)
                                                relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                            if (!relayOK)
                                            {
                                                AddLogWindow("Lỗi bật rơ-le");
                                            }
                                            //04-04-2021
                                            Conv.delay_ms(1);
                                        }
                                    }
                                }
                                
                            }
                            else //Trong mọi tình huống lỗi tắt cấp nguồn
                            {
                                //SetPower(false);
                                // relayBoards1.SetAllState(false);
                                relayBoards1.SetOutput(142, false);
                                relayBoards1.SetOutput(143, false);
                                relayBoards1.SetOutput(144, false);

                            }

                            if (!relayOK) testOK = false;
                            if (testOK)
                            {
                                
                                if (meas_time > 0) Conv.delay_ms(meas_time);
                            }

                            
                            

                        }

                        else //Xử lý các bước đo thông thường
                        {
                            //Có yêu cầu thông báo cho người dùng:
                            if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) > 0&& Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value)!=4 && Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) != 2)
                            {
                                frmShowImage frm = new frmShowImage();
                                frm.lblTestSteep.Text = txtTestSteep.Text;
                                frm.lblTestName.Text = txtTestName.Text;
                                frm.txtMessage.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupMessage"].Value);
                                //frm.picHelp.Image = Image.FromFile(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                                //frm.ShowDialog();
                                frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value), test_steep);
                                
                            }


                            //Có yêu cầu thay đổi thang đo:
                            if (sFUNCTION != "")
                            {
                                if (testOK)
                                {
                                    testOK = DMM.SetFunction(sFUNCTION);
                                    if (testOK)
                                    {
                                        if (sRANGE != "")
                                        {
                                            testOK = DMM.SetRange(sRANGE);
                                            if (!testOK) AddLogWindow("Lỗi đặt thang đo cho DMM");
                                        }
                                    }
                                    else
                                    {
                                        AddLogWindow("Lỗi đặt chế độ đo cho DMM");
                                    }
                                }
                            }

                            

                            //Xử lý bật rơ le:
                            if (relayOK)
                            {
                                //Bật các rơ-le của bước này:
                                if (Relays[0] != "" && relayOK)
                                {
                                    for (int i = 0; i < Relays.Length; i++)
                                    {
                                        if (Relays[i] != "0")
                                        {
                                            
                                            bool isFailRes = false;
                                            for (int j = 0; j < test_steep; j++)
                                            {
                                                string resultTemp = Conv.atos(dgvTestList.Rows[j].Cells["Result"].Value);
                                                string[] relayTemp = Conv.atos(dgvTestList.Rows[j].Cells["RelayOutputs"].Value).Split(new char[] { ',' });
                                                string pcbIndexTemp = Conv.atos(dgvTestList.Rows[j].Cells["PcbIndex"].Value);
                                                if (pcbIndexTemp == pcb_index && resultTemp != "OK" && relayTemp.Contains(Relays[i]) && powerRequest)
                                                {
                                                    isFailRes = true;
                                                    
                                                }
                                            }
                                            if (!isFailRes)
                                                relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                            if (!relayOK)
                                            {
                                                AddLogWindow("Lỗi bật rơ-le");
                                            }
                                            //04-04-2021
                                            Conv.delay_ms(1);
                                        }
                                    }
                                }
                                
                            }
                            else //Trong mọi tình huống lỗi tắt cấp nguồn
                            {
                                //SetPower(false);
                                // relayBoards1.SetAllState(false);
                                relayBoards1.SetOutput(142, false);
                                relayBoards1.SetOutput(143, false);
                                relayBoards1.SetOutput(144, false);

                            }

                            if (!relayOK)
                            {
                                testOK = false;
                                
                            }
                            
                            if (testOK)
                            {
                                //Lấy kết quả đo và tính toán:
                                if (test_unit != "") //Có yêu cầu đọc từ DMM
                                {
                                    string min = Conv.atos(dgvTestList.Rows[test_steep].Cells["MinValue"].Value);
                                    string max = Conv.atos(dgvTestList.Rows[test_steep].Cells["MaxValue"].Value);
                                    string check_unit = Conv.atos(dgvTestList.Rows[test_steep].Cells["Unit"].Value);
                                    double check_min = Double.MinValue;
                                    double check_max = Double.MaxValue;
                                    double read_value = 0;
                                    double check_factor = 1;

                                    if (check_unit.IndexOf("M") > -1)
                                        check_factor = 1000000;
                                    else if (check_unit.IndexOf("K") > -1)
                                        check_factor = 1000;
                                    else if (check_unit.IndexOf("m") > -1)
                                        check_factor = 0.001;
                                    else if (check_unit.IndexOf("u") > -1)
                                        check_factor = 0.000001;

                                    if (min != "") check_min = Conv.atod(min);
                                    if (max != "") check_max = Conv.atod(max);

                                    //Chờ mạch đo ổn định:
                                    if (ms_time > 0) Conv.delay_ms(ms_time);

                                    testOK = false;
                                    meas_time /= 100;
                                    //Có yêu cầu dừng chờ ổn định:
                                    //if(check_unit.Contains("V0"))
                                    //{
                                    //    Thread.Sleep(2000);
                                    //}
                                    //
                                    List<double> listVal = new List<double>();
                                    do
                                    {
                                        dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");

                                        --meas_time;
                                        Conv.delay_ms(100);

                                        if (DMM.TextValue != "---------")
                                        {
                                            read_value = DMM.Value / check_factor;
                                            
                                                


                                            dgvTestList.Rows[test_steep].Cells["PValue"].Value = (DMM.TextValue == "0.L  ") ? "0.L" : read_value.ToString();

                                            //Không quan tâm đến dấu nếu đơn vị là điện trở:
                                            if (check_unit.IndexOf("Ω") > -1)
                                            {
                                                read_value = Math.Abs(read_value)>200?200: Math.Abs(read_value);
                                                dgvTestList.Rows[test_steep].Cells["PValue"].Value = (DMM.TextValue == "0.L  ") ? dgvTestList.Rows[test_steep].Cells["MaxValue"].Value : read_value.ToString();
                                            }

                                            
                                            testOK = (read_value >= check_min && read_value <= check_max);

                                            //Nếu OK chờ thêm 200ms nưữa đọc kết quả lần cuối
                                            if (testOK && meas_time > 1) meas_time = 1;
                                        }
                                    }
                                    while (meas_time > 0);
                                    
                                    //if(testOK&&check_unit.Contains("V0"))
                                    //{
                                    //    if (check_unit.Contains("V0"))
                                    //    {
                                    //        for (int i = 0; i < 40; i++)
                                    //        {
                                    //            read_value = DMM.Value / check_factor;
                                    //            Thread.Sleep(100);
                                    //            listVal.Add(read_value);
                                    //        }
                                    //    }
                                    //    for (int i = 0; i<listVal.Count;i++)
                                    //    {
                                    //        for(int j = 0; j< listVal.Count; j++)
                                    //        {
                                    //            if (listVal[i] - listVal[j] > Global.delta || listVal[i] - listVal[j] < -Global.delta)
                                    //            {
                                                    
                                                    
                                                    
                                    //                testOK = false;
                                    //                dgvTestList.Rows[test_steep].Cells["PValue"].Value = ((DMM.TextValue == "0.L  ") ? "0.L" : read_value.ToString()) + "~";
                                    //                break;
                                    //            }
                                    //        }    
                                    //    }    
                                    //}    
                                    //giaodong1 = false;
                                }
                            }
                            
                        }

                        ////////////////////////////////
                        //testOK = true;

                        dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = testOK ? Color.Lime : Color.Red;
                        dgvTestList.Rows[test_steep].Cells["Result"].Value = testOK ? "OK" : "NG";
                        dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");

                        //to mau cho map
                        if (pnBlockBorder1.Controls.Count > 0)
                        {
                            bool isFailed = false;
                            for(int i = 0; i< dgvTestList.Rows.Count; i++)
                            {
                                if(Conv.atos(dgvTestList.Rows[i].Cells["PcbIndex"].Value) == pcb_index)
                                {
                                    if(i != test_steep)
                                        if (Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value) != "OK" && Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value) != "")
                                        {
                                            isFailed = true;
                                            if(stepFishned)
                                            {
                                                test_steep = dgvTestList.Rows.Count - 4;//-3
                                                stepFishned = false;
                                            }    
                                            
                                            goto thoat;
                                            //pnBlockBorder1.Controls[i].BackColor = testOK ?  Color.Lime : Color.Red;
                                        }
                                }
                                    
                            }
                            if (!isFailed && Global.checkHand!=false)
                            {
                                for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
                                {
                                    if (pnBlockBorder1.Controls[i].Text == pcb_index)
                                    {
                                        pnBlockBorder1.Controls[i].BackColor = testOK ? Color.Lime : Color.Red;
                                        //btnStop_Click(null, null);
                                        //break;
                                    }

                                }
                            }
                            else
                            {
                                for (int i = 0; i < pnBlockBorder1.Controls.Count; i++)
                                {
                                    if (pnBlockBorder1.Controls[i].Text == pcb_index)
                                    {
                                        pnBlockBorder1.Controls[i].BackColor = Color.Red;
                                        
                                        //btnStop_Click(null, null);
                                        //break;
                                    }

                                }
                            }
                        }

                      
                        thoat:
                        if (relayOK && (btnMode3.BackColor != Color.DimGray || ((btnMode2.BackColor != Color.DimGray))))
                        {
                            ++test_steep;
                            if (test_steep >= dgvTestList.Rows.Count) //Hết bước kiểm
                            {
                                //Cắt hết rơ-le:
                                relayBoards1.SetAllState(false);
                                
                                testOK = true;
                                for (int i = 0; i < dgvTestList.Rows.Count; i++)
                                {
                                    if (Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value) != "OK")
                                    {
                                        testOK = false;
                                        break;
                                    }
                                }
                                ledResult.BackColor = testOK ? Global.COLOR_OK : Global.COLOR_NG;
                                ledResult.Text = testOK ? "OK" : "NG";
                                 
                                
                                btnSave_Click1(null, null);
                                //Thoát chương trình kiểm:
                                thread_started = false;
                            }
                        }
                        else //Chế độ chạy thủ công hoặc bước kiểm không đạt
                        {
                            
                            if (!testOK) MessageBox.Show("Bước kiểm không đạt", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            thread_started = false; //Thoát chương trình kiểm
                            
                        }

                    }
                
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    thread_started = false; //Thoát chương trình kiểm
                }
            
                thread_ended = true;
                if (btnReady.Enabled == false)
                    SetControlStatus(STATUS.Stop);
                else SetControlStatus(STATUS.Ready);
            }
        }

        private bool SetPower(bool state)
        {
            ////btnOUT4.BackColor = ((new_states1 & 0x800) != 0) ? Color.Red : Color.DimGray;
            //if (state && ((powerBoard1.States & 0x800) == 0))
            //    return powerBoard1.SetOutput(4, true);
            //else if (!state && ((powerBoard1.States & 0x800) != 0))
            //    return powerBoard1.SetOutput(4, false);
            return true;
        }
       

        private void btnNext_Click(object sender, EventArgs e)
        {
            int steep = Conv.atoi32(txtTestSteep.Text);
            //if (steep >= dgvTestList.Rows.Count) steep = dgvTestList.Rows.Count - 1;
            EnterTestSteep(steep);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            EnterTestSteep(Conv.atoi32(txtTestSteep.Text) - 1);
        }

        private void dgvTestList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (thread_ended && btnReady.Enabled == false)
                EnterTestSteep(e.RowIndex);
        }

        private void btnSave_Click1(object sender, EventArgs e)
        {
            if (ledResult.Text == "OK")
            {
                try
                {
                    string sBuff = "";
                    string path_name = DataLogPath + @"\PASS" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                    if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                    path_name += @"\FUN_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";

                    string path_name_image = path_name.Replace(".csv", ".jpg");
                    //Lan dau tao file thi tao luon header line:
                    if (File.Exists(path_name))
                    {
                        try
                        {
                            File.Delete(path_name);
                        }
                        catch { }
                    }
                    sBuff = lblCaption.Text + "\r\n";
                    sBuff += "Operator," + Conv.atos0(cboOperator.Text) + "\r\n";
                    sBuff += "Equipment," + "" + "\r\n";
                    sBuff += "Location,Sunhouse\r\n";
                    sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                    sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                    sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                    sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                    //        1 2         3           4           5          6          7         8
                    sBuff += "#,NAME,PCB,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                        string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                        string PcbIndex = Conv.atos0(dgvTestList.Rows[r].Cells["PcbIndex"].Value);
                        string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                        string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                        string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                        string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                        string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                        string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                        string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                        sBuff += TestOrder + "," + TestName + "," + PcbIndex + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                    }


                    File.WriteAllText(path_name, sBuff, System.Text.Encoding.UTF8);
                    //File.WriteAllText(path_name, sBuff, new UTF8Encoding(true));

                    //Lưu summary
                    string sBuffSum = "";
                    string path_nameSum = DataLogPath + @"\PASS" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                    if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_nameSum);
                    path_nameSum += @"Summary" + ".csv";
                    //Lan dau tao file thi tao luon header line:
                    if (!File.Exists(path_nameSum))
                    {
                        try
                        {
                            //        1 2         3           4           5          6          7         8
                            sBuffSum = "SerialNumber,TestTime,Elapse(sec),FirtFailItem,Result\r\n";

                            File.WriteAllText(path_nameSum, sBuffSum, System.Text.Encoding.UTF8);

                        }
                        catch { }
                    }
                    sBuffSum = txtSerialNo.Text + "," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "," + lblTestTime.Text + "," + "" + "," + "Pass" + "\r\n";
                    File.AppendAllText(path_nameSum, sBuffSum, System.Text.Encoding.UTF8);

                    ScreenShot(path_name_image);

                    AddLogWindow("Lưu log file thành công.");
                    //Kết thúc:

                    if (isTestOffline)
                    {
                        btnSave.Enabled = false;
                        btnTest.Enabled = false;
                        btnNext.Enabled = false;
                        btnStop_Click(null, null);
                        //panSettings.Enabled = true;
                        //panTest.Enabled = false;

                        btnReady.Enabled = true;
                        txtSerialNo.Text = "";
                        txtSerialNo.Focus();

                        PassCount += 1;
                        TotalCount += 1;
                    }

                }
                catch (Exception ex)
                {
                    AddLogWindow(ex.Message);
                    Global.WriteLogFile(ex.ToString());
                }
            }
            else if (ledResult.Text == "NG")
            {
                try
                {
                    string sBuff = "";
                    string path_name = DataLogPath + @"\FAIL" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                    if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                    path_name += @"\FUN_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";

                    string path_name_image = path_name.Replace(".csv", ".jpg");
                    //Lan dau tao file thi tao luon header line:
                    if (File.Exists(path_name))
                    {
                        try
                        {
                            File.Delete(path_name);
                        }
                        catch { }
                    }
                    sBuff = lblCaption.Text + "\r\n";
                    sBuff += "Operator," + Conv.atos0(cboOperator.Text) + "\r\n";
                    sBuff += "Equipment," + "" + "\r\n";
                    sBuff += "Location,Sunhouse\r\n";
                    sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                    sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                    sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                    sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                    //        1 2         3           4           5          6          7         8
                    sBuff += "#,NAME,PCB,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                        string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                        string PcbIndex = Conv.atos0(dgvTestList.Rows[r].Cells["PcbIndex"].Value);
                        string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                        string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                        string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                        string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                        string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                        string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                        string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                        sBuff += TestOrder + "," + TestName + "," + PcbIndex + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                    }


                    File.WriteAllText(path_name, sBuff, System.Text.Encoding.UTF8);
                    //File.WriteAllText(path_name, sBuff, new UTF8Encoding(true));

                    //Lưu summary
                    string sBuffSum = "";
                    string path_nameSum = DataLogPath + @"\FAIL" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                    if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_nameSum);
                    path_nameSum += @"Summary" + ".csv";
                    //Lan dau tao file thi tao luon header line:
                    if (!File.Exists(path_nameSum))
                    {
                        try
                        {
                            //        1 2         3           4           5          6          7         8
                            sBuffSum = "SerialNumber,TestTime,Elapse(sec),FirtFailItem,Result\r\n";

                            File.WriteAllText(path_nameSum, sBuffSum, System.Text.Encoding.UTF8);

                        }
                        catch { }
                    }

                    string firtFail = "";
                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        if (Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value) != "OK")
                        {
                            firtFail = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                            break;
                        }
                    }

                    sBuffSum = txtSerialNo.Text + "," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "," + lblTestTime.Text + "," + firtFail + "," + "Fail" + "\r\n";
                    File.AppendAllText(path_nameSum, sBuffSum, System.Text.Encoding.UTF8);

                    ScreenShot(path_name_image);

                    AddLogWindow("Lưu log file thành công.");
                    //Kết thúc:

                    if (isTestOffline)
                    {
                        btnSave.Enabled = false;
                        btnTest.Enabled = false;
                        btnNext.Enabled = false;
                        btnStop_Click(null, null);
                        //panSettings.Enabled = true;
                        //panTest.Enabled = false;
                        btnReady.Enabled = true;
                        txtSerialNo.Text = "";
                        txtSerialNo.Focus();

                        TotalCount++;
                    }

                }
                catch (Exception ex)
                {
                    AddLogWindow(ex.Message);
                    Global.WriteLogFile(ex.ToString());
                }
            }
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
        }
        private void ScreenShot(string pathname)
        {
            ////Bitmap bitmap = new Bitmap(this.Bounds.Width, this.Bounds.Height);
            //Bitmap bitmap = new Bitmap(panel2.Bounds.Width, panel2.Bounds.Height);
            ////Graphics g = Graphics.FromImage(bitmap);
            ////g.CopyFromScreen(100,20 , panel2.Bounds.X, panel2.Bounds.Y, panel2.Bounds.Size, CopyPixelOperation.SourceCopy);
            //frmscreenshot.pictureBox1.Image = bitmap;
            //MessageBox.Show("OK");

            Bitmap bitmap = new Bitmap(pnBlockBorder1.ClientSize.Width, pnBlockBorder1.ClientSize.Height);
            pnBlockBorder1.DrawToBitmap(bitmap, pnBlockBorder1.ClientRectangle);

            if (File.Exists(pathname))
            {
                try
                {
                    File.Delete(pathname);
                }
                catch (Exception)
                {

                }
            }
            bitmap.Save(pathname, ImageFormat.Jpeg);
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            //////////////////////////////////////////////////
            //09-04-2021
            if (dgvTestList.Rows.Count > 0 && btnStop.Text == "STOP" && !thread_started)
            {
                if (Conv.atos(dgvTestList.Rows[0].Cells["Result"].Value) != "")
                {
                    bool testOK = true;
                    for (int i = 0; i < dgvTestList.Rows.Count; i++)
                    {
                        if (Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value) != "OK")
                        {
                            testOK = false;
                            break;
                        }
                    }
                    ledResult.BackColor = testOK ? Global.COLOR_OK : Global.COLOR_NG;
                    ledResult.Text = testOK ? "OK" : "NG";

                    btnSave_Click2(null, null);
                }
                dCL6104.OFF_LOAD();

            }
            ///////////////////////////////////////////////////

            if (thread_started)
            {
                thread_started = false;
                SetControlStatus(STATUS.Stopping);
            }
            else
            {
                SetControlStatus(STATUS.Ready);
            }
        }
        private void btnSave_Click2(object sender, EventArgs e)
        {
            if (ledResult.Text == "NG")
            {
                try
                {
                    string sBuff = "";
                    string path_name = DataLogPath + @"\FAIL" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                    if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                    path_name += @"\FUN_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";

                    string path_name_image = path_name.Replace(".csv", ".jpg");
                    //Lan dau tao file thi tao luon header line:
                    if (File.Exists(path_name))
                    {
                        try
                        {
                            File.Delete(path_name);
                        }
                        catch { }
                    }
                    sBuff = lblCaption.Text + "\r\n";
                    sBuff += "Operator," + Conv.atos0(cboOperator.Text) + "\r\n";
                    sBuff += "Equipment," + "" + "\r\n";
                    sBuff += "Location,Sunhouse\r\n";
                    sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                    sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                    sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                    sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                    //        1 2         3           4           5          6          7         8
                    sBuff += "#,NAME,PCB,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                        string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                        string PcbIndex = Conv.atos0(dgvTestList.Rows[r].Cells["PcbIndex"].Value);
                        string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                        string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                        string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                        string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                        string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                        string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                        string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                        sBuff += TestOrder + "," + TestName + "," + PcbIndex + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                    }

                    //sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
                    //sBuff += TestOrder + (char)Keys.Tab + TestName + (char)Keys.Tab + Description + (char)Keys.Tab + CheckValue + (char)Keys.Tab + ReadValue + (char)Keys.Tab + BeginTest + (char)Keys.Tab + EndTest + (char)Keys.Tab + Result + "\r\n";
               
                    File.WriteAllText(path_name, sBuff, System.Text.Encoding.UTF8);
                    //File.WriteAllText(path_name, sBuff, new UTF8Encoding(true));



                    //Lưu summary
                    string sBuffSum = "";
                    string path_nameSum = DataLogPath + @"\FAIL" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                    if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_nameSum);
                    path_nameSum += @"Summary" + ".csv";
                    //Lan dau tao file thi tao luon header line:
                    if (!File.Exists(path_nameSum))
                    {
                        try
                        {
                            //        1 2         3           4           5          6          7         8
                            sBuffSum = "SerialNumber,TestTime,Elapse(sec),FirtFailItem,Result\r\n";

                            File.WriteAllText(path_nameSum, sBuffSum, System.Text.Encoding.UTF8);

                        }
                        catch { }
                    }

                    string firtFail = "";
                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {

                        if (Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value) != "OK")
                        {
                            firtFail = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                            break;
                        }

                    }

                    sBuffSum = txtSerialNo.Text + "," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "," + lblTestTime.Text + "," + firtFail + "," + "Fail" + "\r\n";

                    File.AppendAllText(path_nameSum, sBuffSum, System.Text.Encoding.UTF8);

                    ScreenShot(path_name_image);

                    AddLogWindow("Lưu log file thành công.");

                    if (isTestOffline)
                        TotalCount += 1;
                }
                catch (Exception ex)
                {
                    AddLogWindow(ex.Message);
                    Global.WriteLogFile(ex.ToString());
                }


                SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
            }
        }

        private void btnMode_Click(object sender, EventArgs e)
        {
            if (sender == btnMode1)
            {
                btnMode1.BackColor = Color.Red;
                btnMode2.BackColor = Color.DimGray;
                btnMode3.BackColor = Color.DimGray;
            }
            else if (sender == btnMode2)
            {
                btnMode1.BackColor = Color.DimGray;
                btnMode2.BackColor = Color.Red;
                btnMode3.BackColor = Color.DimGray;
            }
            else if (sender == btnMode3)
            {
                btnMode1.BackColor = Color.DimGray;
                btnMode2.BackColor = Color.DimGray;
                btnMode3.BackColor = Color.Red;
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            frmConfigPLC frmConfig = new frmConfigPLC();
            frmConfig.ShowDialog();
        }

        private void txtSerialNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnReady_Click(null, null);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            frmTestListEdit frm = new frmTestListEdit();
            frm.ShowForm(Global.BOARD_NAMES[BoardType]);

            ReloadMapdataInit();
            btnReload_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TEST_FORM fORM = new TEST_FORM();
            fORM.ShowDialog();
            
        }

        private void toolStripLabel8_Click(object sender, EventArgs e)
        {

        }
    }
}
