using DefaultNS;
using DefaultNS.Model;
using MES;
using MES_INTERFACE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZedGraph;

public partial class frmMain : Form
{
    private TimeSpan lastCPU_TS;
    private System.DateTime lastCPU_Time = DateTime.Now;

    bool thread_ended = true;
    bool thread_started = false;
    Thread threadCOMM;

    private bool dragging = false;
    private Point dragCursorPoint;
    private Point dragFormPoint;

    DateTime startTime = DateTime.Now;

    string DataLogPath;

    private int TotalCount = 0;
    private int PassCount = 0;
    private int LastCountTime = 0;

    //api
    ClsMesInterface mesInterface = new ClsMesInterface();


    enum STATUS
    {
        Ready, // = 0;
        Start,
        Stopping,
        Stop
    }

    public frmMain()
    {
        InitializeComponent();


    }

    private void frmMain_Load(object sender, EventArgs e)
    {
        AddLogWindow("Khởi động chương trình");

        lblBuildInfo.Text = "Ver:" + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
        cboOperator.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Operator'"));
        numTemprature.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Temperature'"));

        ReloadCounter();
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


    bool isTestOffline = false;
    string MachineName = "";
    string MD5Code = "";
    private void frmMain_Shown(object sender, EventArgs e)
    {
        //MessageBox.Show(MD5(@"D:\WORK\PMTT\2.CUSTOMER\10.VIN\FINAL-Mesh\04042021\FW\IO_EXT_V0.0.01_fct.sap").ToUpper());


        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");
        isTestOffline = ini.IniReadValue("API_CONNECTION", "isTestOffLine", "1") == "1" ? true : false;
        lbMeshStatus.BackColor = isTestOffline ? Color.DarkSlateGray : Color.Lime;
        MachineName = ini.IniReadValue("API_CONNECTION", "MachineName", "TEST01_PT_1");

        MD5Code = ini.IniReadValue("API_CONNECTION", "Md5Code", "");

        RestApiHelper.InitGlobalVarial();



        btnReload_Click(null, null);

        powerBoard1.Start(ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1"));
        ReInitBoardType(); //COM2
        DUT.Start(ini.IniReadValue("COMMUNICATION", "DebugCommPort", "COM3"), ini.IniReadValue("COMMUNICATION", "DebugCommPort2", "COM4"));

        panelLeft.Visible = false;
        timer1s.Enabled = true;
        SetControlStatus(STATUS.Ready);
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AppConfig.ini");

        powerBoard1.Stop();
        relayBoards1.Stop();
        DUT.Stop();

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
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");

        string rlboard = "1,2";
        rlboard += "|DO1:LED_SW1_IN|DO2:LED_SW2_IN|DO3:SPD_DET_IN|DO4:DOOR_SW_IN|DO5:CTRL_DET_IN|DO6:GPIO_RESERVED_IN1|DO7:GPIO_RESERVED_IN2|DO8:GPIO_RESERVED_IN3";
        rlboard += "|DO9:STM485_B1_B2|DO10:STM485_A1_A2|DO11:UART7_TX_RX|DO12:UART8_TX_RX|DO13:UART6_TX_RX|DO14:UART2_RS485_B3_Z3|DO15:UART2_RS485_A3_Y3|DO16:UART3_TX_RX";
        rlboard += "|DO17:GPIO_RESERVED_IN4|DO18:MCU_RFID_INT|DO19:CAN_L1_L2|DO20:CAN_H1_H2|DO21:CAN_STM_L|DO22:CAN_STM_H|DO23:PLC1_3V3_IN|DO24:PLC2_3V3_IN";
        rlboard += "|DI1:STM_LED_A_SW1|DI2:STM_LED_A_SW2|DI3:STM_GPIO_RESERVED5|DI4:STM_GPIO_RESERVED6|DI5:STM_GPIO_RESERVED7|DI6:STM_GPIO_RESERVED8|DI7:MCU_RFID_EN|DI8:STM_GPIO_RELAY1(2)";
        string[] rls = rlboard.Split('|'); // Global.RELAY_BOARDS[BoardType].Split('|');

        relayBoards1.Stop();
        relayBoards1.Start(ini.IniReadValue("COMMUNICATION", "RelaysBoardCommPort", "COM2"), rls[0]);

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

    private string ExecuteTextCommand(SerialPort port, string cmd)
    {
        try
        {
            if (port.IsOpen)
            {
                string rx_buff = "";
                int ms_timeout = 500; //Frame timeout

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                port.ReadExisting(); //Lam sach bo dem nhan
                port.Write(Conv.stringToBytes(cmd + "\n"), 0, cmd.Length + 1);
                while (stopWatch.ElapsedMilliseconds < ms_timeout) // && string.IsNullOrEmpty(buffReceiver))
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                    if (port.BytesToRead > 0)
                    {
                        rx_buff += port.ReadExisting();
                        //Khi co byte duoc nhan thi reset frame timeout, cho cho byt cuoi cung kết thuc
                        stopWatch.Restart();
                        ms_timeout = 20;
                    }
                }
                if (rx_buff.Length > 0) rx_buff = rx_buff.Replace(cmd + "\n", "").Replace(cmd.Replace("?", ""), "").Trim();
                return rx_buff;
            }
        }
        catch (Exception)
        {


        }
        return "";
    }

    string GetText(string sData, string key)
    {
        int ind = sData.IndexOf(key + "=");
        if (ind > -1)
        {
            string sBuff = sData.Substring(ind + key.Length + 1);
            ind = sBuff.IndexOf("\r");
            if (ind > 0)
            {
                return sBuff.Substring(0, ind);
            }
        }
        return "";
    }

    private void timer1s_Tick(object sender, EventArgs e)
    {
        //timer1s.Enabled = false;

        lbPassRate.Text = PassCount.ToString() + "/" + TotalCount.ToString();
        lblTime.Text = DateTime.Now.ToString("HH:mm:ss");
        if (btnReady.Enabled == false)
        {
            TimeSpan ts = DateTime.Now - startTime;
            lblTestTime.Text = ((int)(ts.TotalSeconds / 60)).ToString("00") + ":" + ((int)(ts.TotalSeconds % 60)).ToString("00");
        }

        try
        {
            string sBuff = null;

            //Thong ke tai nguyen da su dung:
            slbThreadCount.Text = System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString();

            TimeSpan currCPU_TS = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime - lastCPU_TS;
            lastCPU_TS = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
            TimeSpan currTotal_TS = DateTime.Now - lastCPU_Time;
            lastCPU_Time = DateTime.Now;
            slbCPUUsage.Text = ((currCPU_TS.Ticks / System.Environment.ProcessorCount) * 100 / currTotal_TS.Ticks).ToString("0.0") + "%";

            //sBuff = (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1048576).ToString("0.0");
            //sBuff = (System.Diagnostics.Process.GetCurrentProcess().PagedMemorySize64 / 1048576).ToString("0.0");
            sBuff = (System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1048576).ToString("0.0");
            //sBuff = (GC.GetTotalMemory(false) / 1048576).ToString("0.0");
            slbRAMUsage.Text = sBuff + "MB";
        }
        catch (Exception ex)
        {
        }

        //zgChart.AxisChange();
        //zgChart.Invalidate();
        //zgChart.Refresh();
    }

    private void btnReady_Click(object sender, EventArgs e)
    {
        if (txtSerialNo.Text != "")
        {
            AddLogWindow("Bắt đầu kiểm");
            AddLogWindow("Operator: " + cboOperator.Text);
            AddLogWindow("Serial Number: " + txtSerialNo.Text);
            btnReload_Click(null, null);
            DUT.ClearAllLog();
            startTime = DateTime.Now;

            if (!isTestOffline)
            {
                //AddLogWindow("Kiểm tra trạng thái của mã: " + txtSerialNo.Text + " trên MES");
                //OutputGetSNStatus res = RestApiHelper.GetSNStatus(txtSerialNo.Text, MachineName);
                //if (res != null)
                //{
                //    //MessageBox.Show(res.SN + "\r\n" + res.MachineName + "\r\n" + res.Confirm + "\r\n" + res.ErrorCode);
                //    if (res.Confirm == "OK")
                //    {
                //        //MessageBox.Show("Mã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        EnterTestSteep(0);
                //    }
                //    else
                //    {

                //        MessageBox.Show("Kiểm tra trạng thái mã serial trên MES\r\nMã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        return;
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("Lỗi lấy trạng thái của mã Serial Number trên MES!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}

                AddLogWindow("Kiểm tra trạng thái của mã: " + txtSerialNo.Text + " trên MES");
                string res = mesInterface.CheckSNStatus(txtSerialNo.Text);
                if (res == "OK")
                {
                    //MessageBox.Show("Mã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    EnterTestSteep(0);
                }
                else
                {

                    MessageBox.Show("Trạng thái của mã: " + txtSerialNo.Text + " không hợp lệ!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            else
                EnterTestSteep(0);
        }
        else
        {
            MessageBox.Show("Chưa nhập số chế tạo");
        }
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

        }
        /////////////////////////////////////

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

    private void btnDelete_Click(object sender, EventArgs e)
    {

    }



    private void btnMinimize_Click(object sender, EventArgs e)
    {
        this.WindowState = FormWindowState.Minimized;
    }

    private void btnMaximize_Click(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Maximized)
            this.WindowState = FormWindowState.Normal;
        else
            this.WindowState = FormWindowState.Maximized;
    }

    private void btnclose_Click(object sender, EventArgs e)
    {
        if (thread_ended)
            this.Close();
    }

    private void btnConfig_Click(object sender, EventArgs e)
    {
        string password = Global.PasswordInput();
        if (password.Length > 0)
        {
            if (password == Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
            {
                frmConfigMainDC frm = new frmConfigMainDC();
                frm.ShowDialog();
                ReInitBoardType();
                btnReload_Click(null, null);
                ReloadCounter();
            }
            else MessageBox.Show("Sai mật khẩu.", "LỖI THAO TÁC", MessageBoxButtons.OK);
        }
    }


    private void panelTop_MouseDown(object sender, MouseEventArgs e)
    {
        dragging = true;
        dragCursorPoint = Cursor.Position;
        dragFormPoint = this.Location;
    }

    private void panelTop_MouseMove(object sender, MouseEventArgs e)
    {
        if (dragging)
        {
            Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            this.Location = Point.Add(dragFormPoint, new Size(dif));
        }
    }

    private void panelTop_MouseUp(object sender, MouseEventArgs e)
    {
        dragging = false;
    }

    int sample_count = 0;
    private void SaveDailyToCSV()
    {
        try
        {
            string sBuff = "";
            string path_name = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            path_name += @"\DataLogs\" + DateTime.Now.ToString("yyyy") + @"\" + DateTime.Now.ToString("MM") + @"\" + DateTime.Now.ToString("dd") + @"\";
            if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
            path_name += @"\PV_" + DateTime.Now.ToString("HH") + ".csv";
            //Lan dau tao file thi tao luon header line:
            if (!File.Exists(path_name))
            {
                sample_count = 0;
                sBuff = "#, DATE TIME, PROCESS VALUES\r\n";
                File.WriteAllText(path_name, sBuff);
            }
            ++sample_count;
            sBuff = sample_count.ToString() + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // + "," + ledOut1.Value + "\r\n";
            File.AppendAllText(path_name, sBuff);
        }
        catch (Exception ex)
        {
            Debug.Write(ex.ToString());
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            string sBuff = "";
            string path_name = DataLogPath + @"\";
            if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
            path_name += @"\FUN_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
            sBuff += "Location,VinSmart\r\n";
            sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
            sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
            sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
            sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
            //        1 2         3           4           5          6          7         8
            sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

            for (int r = 0; r < dgvTestList.Rows.Count; r++)
            {
                string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
                //sBuff += TestOrder + (char)Keys.Tab + TestName + (char)Keys.Tab + Description + (char)Keys.Tab + CheckValue + (char)Keys.Tab + ReadValue + (char)Keys.Tab + BeginTest + (char)Keys.Tab + EndTest + (char)Keys.Tab + Result + "\r\n";
            }

            File.WriteAllText(path_name, sBuff, System.Text.Encoding.UTF8);
            //File.WriteAllText(path_name, sBuff, new UTF8Encoding(true));

            AddLogWindow("Lưu log file thành công.");
            //Kết thúc:
            btnSave.Enabled = false;
            btnTest.Enabled = false;
            btnNext.Enabled = false;
            btnStop_Click(null, null);
            //panSettings.Enabled = true;
            //panTest.Enabled = false;
            txtSerialNo.Text = "";
            txtSerialNo.Focus();
        }
        catch (Exception ex)
        {
            AddLogWindow(ex.Message);
            Global.WriteLogFile(ex.ToString());
        }


    }

    #region OLD
    /*
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
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                //        1 2         3           4           5          6          7         8
                sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                    string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                    string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
                    //sBuff += TestOrder + (char)Keys.Tab + TestName + (char)Keys.Tab + Description + (char)Keys.Tab + CheckValue + (char)Keys.Tab + ReadValue + (char)Keys.Tab + BeginTest + (char)Keys.Tab + EndTest + (char)Keys.Tab + Result + "\r\n";
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

            /// Save API
            if (!isTestOffline)
            {
                AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                try
                {
                    //cbi du lieu
                    DataTable dtab = new DataTable();
                    dtab.Columns.Add("TestName");
                    dtab.Columns.Add("LowValue");
                    dtab.Columns.Add("ReadValue");
                    dtab.Columns.Add("HighValue");
                    dtab.Columns.Add("Result");
                    dtab.Columns.Add("TestTime");

                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        dtab.Rows.Add();
                        dtab.Rows[r]["TestName"] = Conv.atos(dgvTestList.Rows[r].Cells["TestName"].Value);
                        dtab.Rows[r]["LowValue"] = Conv.atos("");
                        dtab.Rows[r]["ReadValue"] = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                        dtab.Rows[r]["HighValue"] = Conv.atos("");
                        dtab.Rows[r]["Result"] = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);
                        dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);

                    }

                    // Insert

                    OutputInsertPassFailDetailTestCase resInsert = RestApiHelper.InsertPassFailDetailTestCase(txtSerialNo.Text, MachineName, cboOperator.Text, dtab);
                    if (resInsert != null)
                    {
                        if (resInsert.Status == "OK")
                        {
                            AddLogWindow("Ghi MES thành công!");

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
                        else
                        {
                            AddLogWindow("Ghi MES không thành công\r\nStatus: " + resInsert.Status + "\r\nError Code: " + resInsert.ErrorCode);
                            MessageBox.Show("Ghi MES không thành công\r\nStatus: " + resInsert.Status + "\r\nError Code: " + resInsert.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        AddLogWindow("Ghi MES không thành công");
                        MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                catch (Exception ex)
                {
                    Global.WriteLogFile("[Save to MES] - " + ex.Message);
                }
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
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                //        1 2         3           4           5          6          7         8
                sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                    string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                    string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
                    //sBuff += TestOrder + (char)Keys.Tab + TestName + (char)Keys.Tab + Description + (char)Keys.Tab + CheckValue + (char)Keys.Tab + ReadValue + (char)Keys.Tab + BeginTest + (char)Keys.Tab + EndTest + (char)Keys.Tab + Result + "\r\n";
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

                    TotalCount += 1;
                }

            }
            catch (Exception ex)
            {
                AddLogWindow(ex.Message);
                Global.WriteLogFile(ex.ToString());
            }

            /// Save API
            if (!isTestOffline)
            {
                AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                try
                {
                    //cbi du lieu
                    DataTable dtab = new DataTable();
                    dtab.Columns.Add("TestName");
                    dtab.Columns.Add("LowValue");
                    dtab.Columns.Add("ReadValue");
                    dtab.Columns.Add("HighValue");
                    dtab.Columns.Add("Result");
                    dtab.Columns.Add("TestTime");

                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        dtab.Rows.Add();
                        dtab.Rows[r]["TestName"] = Conv.atos(dgvTestList.Rows[r].Cells["TestName"].Value);
                        dtab.Rows[r]["LowValue"] = Conv.atos("");
                        dtab.Rows[r]["ReadValue"] = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                        dtab.Rows[r]["HighValue"] = Conv.atos("");
                        dtab.Rows[r]["Result"] = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);
                        dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    }

                    // Insert
                    OutputInsertDetailTestCase res = RestApiHelper.InsertDetailTestCase(txtSerialNo.Text, MachineName, dtab);
                    if (res != null)
                    {
                        if (res.Confirm == "OK")
                        {
                            AddLogWindow("Ghi MES thành công!");
                            btnSave.Enabled = false;
                            btnTest.Enabled = false;
                            btnNext.Enabled = false;
                            btnStop_Click(null, null);
                            //panSettings.Enabled = true;
                            //panTest.Enabled = false;

                            btnReady.Enabled = true;
                            txtSerialNo.Text = "";
                            txtSerialNo.Focus();

                            TotalCount += 1;
                        }
                        else
                        {
                            AddLogWindow("Ghi MES không thành công\r\nStatus: " + res.Confirm + "\r\nError Code: " + res.ErrorCode);
                            MessageBox.Show("Ghi MES không thành công\r\nStatus: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        AddLogWindow("Ghi MES không thành công");
                        MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                catch (Exception ex)
                {
                    Global.WriteLogFile("[Save to MES] - " + ex.Message);
                }
            }

        }
        SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
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
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                //        1 2         3           4           5          6          7         8
                sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                    string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                    string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
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



                AddLogWindow("Lưu log file thành công.");
                //Kết thúc:

                //panSettings.Enabled = true;
                //panTest.Enabled = false;

                //if (isTestOffline)
                //{
                //    btnSave.Enabled = false;
                //    btnTest.Enabled = false;
                //    btnNext.Enabled = false;
                //    btnStop_Click(null, null);
                //    btnReady.Enabled = true;
                //    txtSerialNo.Text = "";
                //    txtSerialNo.Focus();
                //}
                if (isTestOffline)
                    TotalCount += 1;

            }
            catch (Exception ex)
            {
                AddLogWindow(ex.Message);
                Global.WriteLogFile(ex.ToString());
            }

            /// Save API
            if (!isTestOffline)
            {
                AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                try
                {
                    //cbi du lieu
                    DataTable dtab = new DataTable();
                    dtab.Columns.Add("TestName");
                    dtab.Columns.Add("LowValue");
                    dtab.Columns.Add("ReadValue");
                    dtab.Columns.Add("HighValue");
                    dtab.Columns.Add("Result");
                    dtab.Columns.Add("TestTime");

                    for (int r = 0; r < dgvTestList.Rows.Count; r++)
                    {
                        dtab.Rows.Add();
                        dtab.Rows[r]["TestName"] = Conv.atos(dgvTestList.Rows[r].Cells["TestName"].Value);
                        dtab.Rows[r]["LowValue"] = "";
                        dtab.Rows[r]["ReadValue"] = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value).Replace("\"", "");
                        dtab.Rows[r]["HighValue"] = "";
                        dtab.Rows[r]["Result"] = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);
                        dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);


                    }

                    // Insert
                    OutputInsertDetailTestCase res = RestApiHelper.InsertDetailTestCase(txtSerialNo.Text, MachineName, dtab);
                    if (res != null)
                    {
                        if (res.Confirm == "OK")
                        {
                            AddLogWindow("Ghi MES thành công!");

                            TotalCount += 1;
                        }
                        else
                        {
                            AddLogWindow("Ghi MES không thành công\r\nStatus: " + res.Confirm + "\r\nError Code: " + res.ErrorCode);
                            MessageBox.Show("Ghi MES không thành công\r\nStatus: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        AddLogWindow("Ghi MES không thành công");
                        MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                catch (Exception ex)
                {
                    Global.WriteLogFile("[Save to MES] - " + ex.Message);
                }
            }

        }
        SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
    }
    */
    #endregion

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
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                //        1 2         3           4           5          6          7         8
                sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                    string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                    string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
                    //sBuff += TestOrder + (char)Keys.Tab + TestName + (char)Keys.Tab + Description + (char)Keys.Tab + CheckValue + (char)Keys.Tab + ReadValue + (char)Keys.Tab + BeginTest + (char)Keys.Tab + EndTest + (char)Keys.Tab + Result + "\r\n";
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

            /// Save API
            if (!isTestOffline)
            {
                AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                try
                {
                    List<clsTestItemInfor> lstItems = new List<clsTestItemInfor>();

                    for (int i = 0; i < dgvTestList.Rows.Count; i++)
                    {
                        clsTestItemInfor itemInfor = new clsTestItemInfor();
                        itemInfor.TestItemName = Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value);
                        itemInfor.MinValue = "";
                        itemInfor.Value = Conv.atos(dgvTestList.Rows[i].Cells["ReadValue"].Value).Replace("\"", "");
                        itemInfor.MaxValue = "";
                        itemInfor.TestResult = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value);
                        itemInfor.TestTime = Conv.atos(dgvTestList.Rows[i].Cells["BeginTest"].Value);

                        lstItems.Add(itemInfor);
                    }

                    string res = mesInterface.UpdateTestListToServer(txtSerialNo.Text, "OK", lstItems);
                    if (res == "OK")
                    {
                        AddLogWindow("Ghi MES thành công!");

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
                    else
                    {
                        AddLogWindow("Ghi MES không thành công!");
                        MessageBox.Show("Ghi MES không thành công!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    Global.WriteLogFile(ex.Message);
                }
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
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                //        1 2         3           4           5          6          7         8
                sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                    string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                    string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
                    //sBuff += TestOrder + (char)Keys.Tab + TestName + (char)Keys.Tab + Description + (char)Keys.Tab + CheckValue + (char)Keys.Tab + ReadValue + (char)Keys.Tab + BeginTest + (char)Keys.Tab + EndTest + (char)Keys.Tab + Result + "\r\n";
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

                    TotalCount += 1;
                }

            }
            catch (Exception ex)
            {
                AddLogWindow(ex.Message);
                Global.WriteLogFile(ex.ToString());
            }

            /// Save API
            if (!isTestOffline)
            {
                AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                try
                {
                    List<clsTestItemInfor> lstItems = new List<clsTestItemInfor>();

                    for (int i = 0; i < dgvTestList.Rows.Count; i++)
                    {
                        clsTestItemInfor itemInfor = new clsTestItemInfor();
                        itemInfor.TestItemName = Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value);
                        itemInfor.MinValue = "";
                        itemInfor.Value = Conv.atos(dgvTestList.Rows[i].Cells["ReadValue"].Value).Replace("\"", "");
                        itemInfor.MaxValue = "";
                        itemInfor.TestResult = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value);
                        itemInfor.TestTime = Conv.atos(dgvTestList.Rows[i].Cells["BeginTest"].Value);

                        lstItems.Add(itemInfor);
                    }

                    string res = mesInterface.UpdateTestListToServer(txtSerialNo.Text, "NG", lstItems);
                    if (res == "OK")
                    {
                        AddLogWindow("Ghi MES thành công!");

                        btnSave.Enabled = false;
                        btnTest.Enabled = false;
                        btnNext.Enabled = false;
                        btnStop_Click(null, null);
                        //panSettings.Enabled = true;
                        //panTest.Enabled = false;
                        btnReady.Enabled = true;
                        txtSerialNo.Text = "";
                        txtSerialNo.Focus();

                        TotalCount += 1;
                    }
                    else
                    {
                        AddLogWindow("Ghi MES không thành công!");
                        MessageBox.Show("Ghi MES không thành công!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    Global.WriteLogFile(ex.Message);
                }
            }

        }
        SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
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
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";
                //        1 2         3           4           5          6          7         8
                sBuff += "#,TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string Description = Conv.atos0(dgvTestList.Rows[r].Cells["Description"].Value);
                    string CheckValue = Conv.atos(dgvTestList.Rows[r].Cells["CheckValue"].Value);
                    string ReadValue = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string Elapse = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + Description + "," + CheckValue + "," + ReadValue + "," + BeginTest + "," + Elapse + "," + Result + "\r\n";
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



                AddLogWindow("Lưu log file thành công.");
                //Kết thúc:

                //panSettings.Enabled = true;
                //panTest.Enabled = false;

                //if (isTestOffline)
                //{
                //    btnSave.Enabled = false;
                //    btnTest.Enabled = false;
                //    btnNext.Enabled = false;
                //    btnStop_Click(null, null);
                //    btnReady.Enabled = true;
                //    txtSerialNo.Text = "";
                //    txtSerialNo.Focus();
                //}
                if (isTestOffline)
                    TotalCount += 1;

            }
            catch (Exception ex)
            {
                AddLogWindow(ex.Message);
                Global.WriteLogFile(ex.ToString());
            }

            /// Save API
            if (!isTestOffline)
            {
                AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                try
                {
                    List<clsTestItemInfor> lstItems = new List<clsTestItemInfor>();

                    for (int i = 0; i < dgvTestList.Rows.Count; i++)
                    {
                        clsTestItemInfor itemInfor = new clsTestItemInfor();
                        itemInfor.TestItemName = Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value);
                        itemInfor.MinValue = "";
                        itemInfor.Value = Conv.atos(dgvTestList.Rows[i].Cells["ReadValue"].Value).Replace("\"", "");
                        itemInfor.MaxValue = "";
                        itemInfor.TestResult = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value);
                        itemInfor.TestTime = Conv.atos(dgvTestList.Rows[i].Cells["BeginTest"].Value);

                        lstItems.Add(itemInfor);
                    }
                    string res = mesInterface.UpdateTestListToServer(txtSerialNo.Text, "NG", lstItems);
                    if (res == "OK")
                    {
                        AddLogWindow("Ghi MES thành công!");
                        TotalCount += 1;
                    }
                    else
                    {
                        AddLogWindow("Ghi MES không thành công!");
                        MessageBox.Show("Ghi MES không thành công!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    Global.WriteLogFile("[Save to MES] - " + ex.Message);
                }
            }

        }
        SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
    }

    enum MAINDC_STEEPS
    {
        STM_START_TEST = (int)0,
        //STM_SERIAL_NUMBER,
        STM_SOFTWARE_VERSION,
        //STM_HARDWARE_VERSION,
        STM_FLASH_TEST,
        STM_RS485_TEST,
        STM_DIGITAL_INPUT,
        STM_DIGITAL_OUTPUT,
        STM_STOP_TEST,

        START_TEST_IMX,
        SERIAL_NUMBER,
        SOFTWARE_VERSION,
        HARDWARE_VERSION,
        DDR_TEST,
        EMMC_TEST,
        FLASH_TEST,
        MCU_RFID_EN,
        PLC1_3V3_IN,
        PLC2_3V3_IN,
        MCU_RFID_INT,
        UART2_TEST,
        UART3_TEST,
        UART5_TEST,
        UART6_TEST,
        UART7_TEST,
        UART8_TEST,
        CAN0_TEST,
        CAN1_TEST,
        I2C_TEST,
        ETHERNET_TEST,
        USB_TEST,
        STOP_TEST
    }
    string[] MAINDC_STEEP_DESCRIPTION = {
        "Vào chế độ kiểm tra chip STM",
        //"Kiểm tra số chế tạo in trên QR",
        "Kiểm tra phiên bản phần mềm",
        //"Kiểm tra phiên bản phần cứng",
        "Kiểm tra bộ nhớ FLASH",
        "Kiểm tra cổng RS485",
        "Kiểm tra các đầu vào số",
        "Kiểm tra các đầu ra số",
        "Kết thúc kiểm tra STM",
        "Vào chế độ kiểm tra IMX",
        "Kiểm tra số chế tạo với tem QR",
        "Phiên bản phần mềm được nạp",
        "Mã phiên bản phần cứng",
        "Kiểm tra RAM",
        "Kiểm tra thẻ nhớ ngoài",
        "Kiểm tra bộ nhớ FLASH mở rộng",
        "Kiểm tra đầu ra MCU_RFID_EN",
        "Kiểm tra đầu vào PLC1_3V3_IN",
        "Kiểm tra đầu vào PLC2_3V3_IN",
        "Kiểm tra đầu vào MCU_RFID_INT",
        "UART2 RS485 kết nối với FAN",
        "UART3 kết nối với PLC1",
        "UART5 kết nối với chip SMT",
        "UART6 kết nối với PLC2",
        "UART7 kết nối với RFID",
        "UART8 kết nối với HMI",
        "Kiểm tra các cổng kết nối CAN0",
        "Kiểm tra các cổng kết nối CAN1",
        "Kiểm tra kết nối I2C",
        "Kiểm tra kết nối cổng LAN thứ 2",
        "Kiểm tra cổng USB Host",
        "Thoát chế độ kiểm"
    };

    private void btnReload_Click(object sender, EventArgs e)
    {
        //string strSQL;
        //DataTable dtab;

        //Đọc lại thông tin cấu hình:
        DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
        if (!Directory.Exists(DataLogPath)) DataLogPath = Application.StartupPath + @"\DataLogs";

        //BoardType = Conv.atoi32(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='BoardType'"));
        //DeltaT = Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaT'"));
        //DeltaU = Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaU'"));
        //DeltaI = Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaI'")) / 1000;
        //DeltaP = Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaP'"));
        //LoadPower = Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='LoadPower'"));
        //DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
        //if (!Directory.Exists(DataLogPath)) DataLogPath = Application.StartupPath + @"\DataLogs";

        string[] steepNames = Enum.GetNames(typeof(MAINDC_STEEPS));

        dgvTestList.Rows.Clear();

        for (int i = 0; i < steepNames.Length; i++)
        {
            dgvTestList.Rows.Add();
            dgvTestList.Rows[i].Cells["TestOrder"].Value = i + 1;
            dgvTestList.Rows[i].Cells["TestName"].Value = steepNames[i];
            dgvTestList.Rows[i].Cells["Description"].Value = MAINDC_STEEP_DESCRIPTION[i];
        }

        txtTotalSteep.Text = dgvTestList.Rows.Count.ToString();
    }

    private void EnterTestSteep(int test_steep)
    {
        if (test_steep > -1 && test_steep < dgvTestList.Rows.Count)
        {
            SetControlStatus(STATUS.Start);
            thread_ended = false;
            thread_started = true;
            int tryStartTestImx = 8;
            try
            {
                while (thread_started)
                {
                    DateTime BeginTest = DateTime.Now;
                StartAgainIfStartIMXFail:
                    bool testOK = false;
                    
                    try
                    {
                        dgvTestList.CurrentCell = dgvTestList.Rows[test_steep].Cells[0];

                        dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = Color.LightBlue;
                        dgvTestList.Rows[test_steep].Cells["BeginTest"].Value = BeginTest.ToString("yyyy-MM-dd HH:mm:ss");
                        //Xoa ket qua cu:
                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = "";
                        dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "";
                        dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = "";
                        dgvTestList.Rows[test_steep].Cells["Result"].Value = "";

                        txtTestSteep.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestOrder"].Value);
                        txtTestName.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestName"].Value);

                        ledResult.Text = "---";
                        ledResult.BackColor = Global.COLOR_OFF;

                        relayBoards1.SetAllState(false);  //Tắt tất cả rơ-le

                        AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text);

                        Application.DoEvents();
                        //Xử lý LOGIC theo từng bước:
                        if (test_steep == (int)MAINDC_STEEPS.STM_START_TEST) //0
                        {
                            int s_count = 0;
                            powerBoard1.SetOutput(4, false, 1000); //Tắt nguồn
                            frmShowChoice frm = new frmShowChoice();
                            frm.lblTestSteep.Text = "1";
                            frm.lblTestName.Text = "BƯỚC CHUẨN BỊ";
                            frm.lblMessage.Text = "Kiểm tra các kết nối với JIG, Chú ý các điều kiện an toàn.\r\n";
                            frm.lblMessage.Text += "\r\nBấm chuột vào 'OK'";
                            frm.btnClose.Enabled = true;
                            frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray;
                            frm.ShowDialog();
                            powerBoard1.SetOutput(4, true); //Bật nguồn
                            //Chờ thời gian khoảng 20 giây cho thist bị sẵn sàng:
                            while (++s_count < 20)
                            {
                                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");
                                Conv.delay_ms(1000);
                            }
                            testOK = DUT.CAN_StartTest();
                        }
                        /*
                        else if (test_steep == (int)MAINDC_STEEPS.STM_SERIAL_NUMBER) //Check Serial
                        {
                            string dutValue = DUT.CAN_ReadSerialNumber();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = txtSerialNo.Text;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (txtSerialNo.Text == dutValue);
                        }
                        */
                        else if (test_steep == (int)MAINDC_STEEPS.STM_SOFTWARE_VERSION) //Software Version
                        {
                            string dutValue = DUT.CAN_ReadSoftwareVersion();
                            string[] checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'")).Split('|');

                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue[0];
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;

                            testOK = (checkValue[0] == dutValue);
                        }
                        //else if (test_steep == (int)MAINDC_STEEPS.STM_HARDWARE_VERSION) //=2
                        //{
                        //    string dutValue = DUT.CAN_ReadHardwareVersion();
                        //    string[] checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='HardwareVersion'")).Split('|');

                        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue[0];
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;

                        //    testOK = (checkValue[0] == dutValue);
                        //}
                        else if (test_steep == (int)MAINDC_STEEPS.STM_FLASH_TEST) //
                        {
                            testOK = DUT.CAN_FlashTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.STM_RS485_TEST) //
                        {
                            relayBoards1.SetOutput(9, true);  //Đóng RS485_A
                            relayBoards1.SetOutput(10, true); //Đóng RS485_A
                            testOK = DUT.CAN_RS485Test();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                            if (testOK) relayBoards1.SetAllState(false);  //Tắt tất cả rơ-le
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.STM_DIGITAL_OUTPUT) //
                        {
                            //Tắt tất cả đầu ra:
                            for (int i = 0; i < 8; i++) DUT.SetOutput(i, false);
                            relayBoards1.ReadInputImediate(); //Đọc ngay lập tức trạng thái đầu vào
                            //if (!relayBoards1.GetInputState(1) && !relayBoards1.GetInputState(2) && !relayBoards1.GetInputState(3) && !relayBoards1.GetInputState(4) && !relayBoards1.GetInputState(5) && !relayBoards1.GetInputState(6))
                            if (!relayBoards1.GetInputState(1) && !relayBoards1.GetInputState(2) && !relayBoards1.GetInputState(3) && !relayBoards1.GetInputState(4) && !relayBoards1.GetInputState(5) && !relayBoards1.GetInputState(6) && !relayBoards1.GetInputState(7) && !relayBoards1.GetInputState(8)) //Bổ xung DI8 cho 2 Output của DUT
                            {
                                testOK = true;
                                for (int i = 0; i < 8; i++)
                                {
                                    DUT.SetOutput(i, true);
                                    Conv.delay_ms(500);
                                    if (relayBoards1.GetInputState(i + 1, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                /*
                                ///////////////////////////////////////////////////////////////////////////
                                ///////////////////////////////////////////////////////////////////////////
                                //Bổ sung 2 Output STM_GPIO_RELAY1 và STM_GPIO_RELAY2 chỉ với 1 đầu vào DI8:
                                DUT.SetOutput(6, true); //Bật đầu ra STM_GPIO_RELAY1
                                Conv.delay_ms(500);
                                if (relayBoards1.GetInputState(8, true))
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                else
                                {
                                    testOK = false;
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                }
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";

                                DUT.SetOutput(6, false); //Tắt đầu ra STM_GPIO_RELAY1
                                if (!relayBoards1.GetInputState(8, true)) //Khẳng định đầu ra STM_GPIO_RELAY1 đã tắt
                                {
                                    DUT.SetOutput(7, true); //Bật đầu ra STM_GPIO_RELAY2
                                    Conv.delay_ms(500);
                                    if (relayBoards1.GetInputState(8, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }    
                                else
                                    testOK = false; //Đầu ra STM_GPIO_RELAY1 có vấn đề
                                ////////////////////////////////////////////////////////////////////////////
                                ////////////////////////////////////////////////////////////////////////////
                                */
                            }
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.STM_DIGITAL_INPUT) //
                        {
                            if (DUT.ReadAllInput() == 0)
                            {
                                int inpValue;
                                testOK = true;
                                for (int i = 0; i < 6; i++)
                                {
                                    relayBoards1.SetOutput(i + 1, true);
                                    inpValue = DUT.ReadAllInput();
                                    if ((inpValue & (1 << i)) == 0)
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    else
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra
                            }
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.STM_STOP_TEST) //
                        {
                            testOK = DUT.CAN_StopTest();
                        }

                        //
                        else if (test_steep == (int)MAINDC_STEEPS.START_TEST_IMX) //Start Test
                        {
                            Conv.delay_ms(1000);
                            testOK = DUT.TCP_StartTest();
                            //if (!testOK) testOK = DUT.TCP_StartTest();
                            //if (!testOK) testOK = DUT.TCP_StartTest();
                            if (!testOK && --tryStartTestImx >0  && btnStop.Text == "PAUSE") goto StartAgainIfStartIMXFail;
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.SERIAL_NUMBER) //Check Serial
                        {
                            string dutValue = DUT.TCP_ReadSerialNumber();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = txtSerialNo.Text;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (txtSerialNo.Text == dutValue);
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.SOFTWARE_VERSION) //Software Version
                        {
                            string dutValue = DUT.TCP_ReadSoftwareVersion();
                            string[] checkValue = (Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'")) + "|").Split('|');
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue[1];
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (checkValue[1] == dutValue);
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.HARDWARE_VERSION)
                        {
                            string dutValue = DUT.TCP_ReadHardwareVersion();
                            string[] checkValue = (Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='HardwareVersion'")) + "|").Split('|');
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue[1];
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (checkValue[1] == dutValue);
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.FLASH_TEST) //EXTERNAL FLASH check
                        {
                            testOK = DUT.TCP_FlashTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.DDR_TEST) //
                        {
                            testOK = DUT.TCP_DDRTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.EMMC_TEST) //
                        {
                            testOK = DUT.TCP_EMMCTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.I2C_TEST) //
                        {
                            testOK = DUT.TCP_I2CTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.MCU_RFID_EN) //
                        {
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "Skip";
                            testOK = true;
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.PLC1_3V3_IN) //
                        {
                            if (DUT.TCP_GetGPIO_Input(0) == false)
                            {
                                relayBoards1.SetOutput(23, true);
                                testOK = DUT.TCP_GetGPIO_Input(0);
                            }
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += testOK ? "Pass" : "Fail";
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "Pass/Fail";

                        }
                        else if (test_steep == (int)MAINDC_STEEPS.PLC2_3V3_IN) //
                        {
                            if (DUT.TCP_GetGPIO_Input(1) == false)
                            {
                                relayBoards1.SetOutput(24, true);
                                testOK = DUT.TCP_GetGPIO_Input(1);
                            }
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += testOK ? "Pass" : "Fail";
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "Pass/Fail";

                        }
                        else if (test_steep == (int)MAINDC_STEEPS.MCU_RFID_INT) //
                        {
                            if (DUT.TCP_GetGPIO_Input(2) == false)
                            {
                                relayBoards1.SetOutput(18, true);
                                testOK = DUT.TCP_GetGPIO_Input(2);
                            }
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += testOK ? "Pass" : "Fail";
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "Pass/Fail";

                        }
                        else if (test_steep == (int)MAINDC_STEEPS.UART2_TEST) //
                        {
                            relayBoards1.SetOutput(14, true); //Nối B-Z
                            relayBoards1.SetOutput(15, true); //Nối A-Y
                            string retValue = DUT.TCP_UART_Test(2);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.UART3_TEST) //
                        {
                            relayBoards1.SetOutput(16, true); //Nối TX-RX
                            string retValue = DUT.TCP_UART_Test(3);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.UART5_TEST) //
                        {
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Skip";
                            testOK = true;
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.UART6_TEST) //
                        {
                            relayBoards1.SetOutput(13, true); //Nối TX-RX
                            string retValue = DUT.TCP_UART_Test(6);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.UART7_TEST) //
                        {
                            relayBoards1.SetOutput(11, true); //Nối TX-RX
                            string retValue = DUT.TCP_UART_Test(7);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.UART8_TEST) //
                        {
                            relayBoards1.SetOutput(12, true); //Nối TX-RX
                            string retValue = DUT.TCP_UART_Test(8);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.CAN0_TEST) //
                        {
                            //relayBoards1.SetOutput(12, true); //Nối TX-RX
                            string retValue = DUT.TCP_CAN_Test(0);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.CAN1_TEST) //
                        {
                            //relayBoards1.SetOutput(12, true); //Nối TX-RX
                            string retValue = DUT.TCP_CAN_Test(1);
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.ETHERNET_TEST) //
                        {
                            //relayBoards1.SetOutput(12, true); //Nối TX-RX
                            string retValue = DUT.TCP_Ethernet_Test();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.USB_TEST) //
                        {
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Skip";
                            testOK = true;
                        }
                        else if (test_steep == (int)MAINDC_STEEPS.STOP_TEST) //
                        {
                            testOK = DUT.TCP_StopTest();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "[EnterTestSteep]");
                    }

                    //Kết luận kết quả:
                    dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = testOK ? Global.COLOR_OK : Global.COLOR_NG;
                    dgvTestList.Rows[test_steep].Cells["Result"].Value = testOK ? "OK" : "NG";
                    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");

                    //Tính toán bước tiếp theo:
                    if (btnMode3.BackColor != Color.DimGray || ((btnMode2.BackColor != Color.DimGray) && testOK))
                    {
                        ++test_steep;
                        if (test_steep >= dgvTestList.Rows.Count) //Hết bước kiểm
                        {
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
                            //Tu dong luu khi buoc kiem OK
                            //if (testOK)
                            //{
                            //    btnSave_Click(null, null);
                            //}
                            //else
                            //{
                            //    MessageBox.Show("Đã hoàn thành bước kiểm cuối cùng, kết quả NG");
                            //}
                            btnSave_Click1(null, null);
                            thread_started = false; //Thoát chương trình kiểm
                        }
                    }
                    else //Chế độ chạy thủ công hoặc bước kiểm không đạt
                    {
                        if (!testOK)
                        {
                            MessageBox.Show("Bước kiểm không đạt");
                        }
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

    private void dgvTestList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (thread_ended && btnReady.Enabled == false)
            EnterTestSteep(e.RowIndex);
    }

    private void btnHome_Click(object sender, EventArgs e)
    {
        panelLeft.Visible = !panelLeft.Visible;
        //panelHeader.Height = panelLeft.Visible ? 400 : 300;
    }

    private void txtSerialNo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            btnReady_Click(null, null);
        }
    }

    private void DMM_DoubleClick(object sender, EventArgs e)
    {
        if (panelHeader.Height == 156)
        {
            panelHeader.Height = 236;
        }
        else
        {
            panelHeader.Height = 156;
        }
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
        EnterTestSteep(Conv.atoi32(txtTestSteep.Text) - 1);
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        int steep = Conv.atoi32(txtTestSteep.Text);
        //if (steep >= dgvTestList.Rows.Count) steep = dgvTestList.Rows.Count - 1;
        EnterTestSteep(steep);
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

    private void AddLogWindow(string mess)
    {
        //lstLogWindow.Items.Insert(0, "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "] " + mess);
        if (lstLogWindow.Items.Count > 10000) lstLogWindow.Items.RemoveAt(0);
        lstLogWindow.Items.Add("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "] " + mess);
        lstLogWindow.TopIndex = lstLogWindow.Items.Count - 1;
        lstLogWindow.SelectedIndex = lstLogWindow.Items.Count - 1;
        Global.WriteLogFile(mess);
    }

    private void AddLogWindow(string mess, bool show_popup)
    {
        AddLogWindow(mess);
        if (show_popup) MessageBox.Show(mess);
    }

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        lstLogWindow.Items.Clear();
    }

    private void chkAutoSave_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void panel3_Paint(object sender, PaintEventArgs e)
    {

    }

    private void SetControlStatus(STATUS _status)
    {
        cboOperator.Enabled = false;
        txtSerialNo.Enabled = false;
        btnReady.Enabled = false;
        btnStop.Enabled = false;
        btnTest.Enabled = false;
        btnNext.Enabled = false;

        btnReload.Enabled = false;

        picTesting.Visible = false;
        btnSave.Enabled = false;
        btnConfig.Enabled = false;

        switch (_status)
        {
            case STATUS.Ready:
                powerBoard1.SetOutput(1, false);
                powerBoard1.SetOutput(4, false); //Tắt nguồn
                
                cboOperator.Enabled = btnReady.Enabled = btnConfig.Enabled = btnReload.Enabled = true;
                btnStop.Text = "STOP";

                txtSerialNo.Enabled = true;
                txtSerialNo.Text = "";
                txtSerialNo.Focus();
                break;
            case STATUS.Start:
                powerBoard1.SetOutput(1, true);
                btnStop.Text = "PAUSE";
                picTesting.Visible = btnStop.Enabled = true;
                break;
            case STATUS.Stopping:
                btnStop.Text = "...";
                break;
            case STATUS.Stop:
                btnStop.Text = "STOP";
                btnSave.Enabled = btnStop.Enabled = btnTest.Enabled = btnNext.Enabled = true;
                break;
            default:
                break;
        }


    }

    private void DUT_Load(object sender, EventArgs e)
    {

    }

    private void powerBoard1_Load(object sender, EventArgs e)
    {

    }

    private void tcpiP_Object1_Load(object sender, EventArgs e)
    {

    }

    string STR_Num = "1234567890";
    private void cboOperator_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!Char.IsControl(e.KeyChar))
        {
            if (!STR_Num.Contains(e.KeyChar)) e.Handled = true;
        }
    }

    public static string MD5(string path)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(path))
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
            }
        }
    }
}
