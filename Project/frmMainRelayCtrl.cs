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
    private void frmMain_Shown(object sender, EventArgs e)
    {
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");

        isTestOffline = ini.IniReadValue("API_CONNECTION", "isTestOffLine", "1") == "1" ? true : false;
        lbMeshStatus.BackColor = isTestOffline ? Color.DarkSlateGray : Color.Lime;
        MachineName = ini.IniReadValue("API_CONNECTION", "MachineName", "TEST01_PT_1");
        RestApiHelper.InitGlobalVarial();

        btnReload_Click(null, null);

        ReInitBoardType();
        //tcpiP_Object1.Start("192.168.1.1");
        powerBoard1.Start(ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1"));
        spM931.Start(ini.IniReadValue("COMMUNICATION", "SPM3MeterPort", "COM8"));
        autonicsMT41.Start(ini.IniReadValue("COMMUNICATION", "DCVoltageMeterPort", "COM9"));
        DUT.Start();

        panelLeft.Visible = false;
        timer1s.Enabled = true;
        SetControlStatus(STATUS.Ready);
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AppConfig.ini");

        //tcpiP_Object1.Stop();

        autonicsMT41.Stop();
        relayBoards1.Stop();
        spM931.Stop();
        DUT.Stop();
        Conv.delay_ms(1000);

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
        lbPassRate.Text = PassCount.ToString() + "/" + TotalCount.ToString();
        lblTime.Text = DateTime.Now.ToString("HH:mm:ss");
        if (btnReady.Enabled == false)
        {
            TimeSpan ts = DateTime.Now - startTime;
            lblTestTime.Text = ((int)(ts.TotalSeconds / 60)).ToString("00") + ":" + ((int)(ts.TotalSeconds % 60)).ToString("00");
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

    void ReInitBoardType()
    {
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");
        string rlboard = "1,1|DO1:TP149_IN1|DO2:TP150_IN2|DO3:TP151_IN3|DO4:TP152_IN4|DO5:TP153_IN5|DO6:TP154_IN6|DO7:TP155_IN7|DO8:TP156_IN8|DO9:TP157_IN9|DO10:TP158_IN10|DO11:TP159_IN11|DO12:TP160_IN12|DO13:HV1N_R150K_TP137|DO14:HV1P_R20K_TP138|DO15:HV2N_R150K_TP139|DO16:HV2P_R20K_TP140|DI1:TP32_OUT5|DI2:TP34_OUT3|DI3:TP36_OUT4|DI4:TP39_OUT8|DI5:TP41_OUT6|DI6:TP43_OUT7|DI7:TP45_OUT11|DI8:TP47_OUT9|DI9:TP49_OUT10|DI10:J25.5_OUT1|DI11:J2.1_OUT2";
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

    private void btnConfig_Click(object sender, EventArgs e)
    {
        string password = Global.PasswordInput();
        if (password.Length > 0)
        {
            if (password == Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
            {
                frmConfigRelayCtrl frm = new frmConfigRelayCtrl();
                frm.ShowDialog();
                ReInitBoardType();
                btnReload_Click(null, null);
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
                
                if(isTestOffline)
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
                    PassCount += 1;
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

                            TotalCount += 1;
                            PassCount += 1;
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
                
                if(isTestOffline)
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

                if(isTestOffline)
                TotalCount += 1;

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

                    TotalCount += 1;
                    PassCount += 1;
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

                if (isTestOffline)
                    TotalCount += 1;

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
    }


    enum MAINDC_STEEPS
    {
        START_TEST = (int)0,
        SERIAL_NUMBER,
        SOFTWARE_VERSION,
        HARDWARE_VERSION,
        DDR_TEST,
        EMMC_TEST,
        FLASH_TEST,
        GPIO_OUTPUT,
        GPIO_INPUT,
        UART_TEST,
        CAN_TEST,
        I2C_TEST,
        ETHERNET_TEST,
        USB_TEST,
        STOP_TEST
    }
    string[] MAINDC_STEEP_DESCRIPTION = {
        "Vào chế độ kiểm tra",
        "Kiểm tra số chế tạo với tem QR",
        "Phiên bản phần mềm được nạp",
        "Mã phiên bản phần cứng",
        "Kiểm tra RAM",
        "Kiểm tra thẻ nhớ ngoài",
        "Kiểm tra bộ nhớ FLASH mở rộng",
        "Kiểm tra các đầu ra",
        "Kiểm tra các đầu vào",
        "Kiểm tra các cổng kết nối UART",
        "Kiểm tra các cổng kết nối CAN",
        "Kiểm tra kết nối I2C",
        "Ping kết nối với cổng LAN thứ 2",
        "Kiểm tra cổng USB Host",
        "Thoát chế độ kiểm"
    };

    enum RELAY_STEEPS
    {
        START_TEST = (int)0,
        SERIAL_NUMBER,
        SOFTWARE_VERSION,
        //HARDWARE_VERSION,
        TEMPRATURE_TEST,
        FLASH_TEST,
        VOLTAGE_AC220V,
        VOLTAGE_DC500V,
        INSULATION_HV1,
        INSULATION_HV2,
        GPIO_OUTPUT,
        GPIO_INPUT,
        UART_RS485_TEST,
        UART_DEBUG_TEST,
        STOP_TEST
    }

    string[] RELAY_STEEP_DESCRIPTION = {
        "Vào chế độ kiểm tra",
        "Kiểm tra số chế tạo với tem QR",
        "Phiên bản phần mềm được nạp",
        //"Mã phiên bản phần cứng",
        "Kiểm tra cảm biến nhiệt độ",
        "Kiểm tra bộ nhớ FLASH mở rộng",
        "Kiểm tra đo điện áp cấp xoay chiều 3 pha",
        "Kiểm tra đo cao áp một chiều HV1 và HV2",
        "Kiểm tra cách điện cao áp HV1",
        "Kiểm tra cách điện cao áp HV2",
        "Kiểm tra các đầu ra",
        "Kiểm tra các đầu vào",
        "Kiểm tra các cổng kết nối RS485",
        "Kiểm tra các cổng kết nối UART Debug",
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

        string[] steepNames = Enum.GetNames(typeof(RELAY_STEEPS));

        dgvTestList.Rows.Clear();

        for (int i = 0; i < steepNames.Length; i++)
        {
            dgvTestList.Rows.Add();
            dgvTestList.Rows[i].Cells["TestOrder"].Value = i + 1;
            dgvTestList.Rows[i].Cells["TestName"].Value = steepNames[i];
            dgvTestList.Rows[i].Cells["Description"].Value = RELAY_STEEP_DESCRIPTION[i];
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
            try
            {
                while (thread_started)
                {
                    bool testOK = false;

                    DateTime BeginTest = DateTime.Now;
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

                        //btnTest.Enabled = false;

                        AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text);

                        //Xử lý LOGIC theo từng bước:
                        if (test_steep == (int)RELAY_STEEPS.START_TEST) //Start Test
                        {
                            frmShowChoice frm = new frmShowChoice();
                            frm.lblTestSteep.Text = "1";
                            frm.lblTestName.Text = "BƯỚC CHUẨN BỊ";
                            frm.lblMessage.Text = "Kiểm tra các kết nối JIG với máy tính, bật tất cả các Aptomat nguồn 1 pha, 3 pha.\r\n";
                            frm.lblMessage.Text += "\r\nBấm chuột vào 'OK'";
                            frm.btnClose.Enabled = true;
                            frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray;
                            frm.ShowDialog();
                            powerBoard1.SetOutput(4, true, 3000); //Bật nguồn
                            
                            AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text + "Thoát selt tesst");
                            
                            testOK = DUT.StartTest();
                        }
                        else if (test_steep == (int)RELAY_STEEPS.SERIAL_NUMBER) //Check Serial
                        {
                            string dutValue = DUT.ReadSerialNumber();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = txtSerialNo.Text;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (txtSerialNo.Text == dutValue);
                        }
                        else if (test_steep == (int)RELAY_STEEPS.SOFTWARE_VERSION) //Software Version
                        {
                            string dutValue = DUT.ReadSoftwareVersion();
                            string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'"));

                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;

                            testOK = (checkValue == dutValue);
                        }
                        //else if (test_steep == (int)RELAY_STEEPS.HARDWARE_VERSION)
                        //{
                        //    string dutValue = DUT.ReadHardwareVersion();
                        //    string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='HardwareVersion'"));
                        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                        //    testOK = (checkValue == dutValue);
                        //}
                        else if (test_steep == (int)RELAY_STEEPS.FLASH_TEST) //EXTERNAL FLASH check
                        {
                            testOK = DUT.FlashTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)RELAY_STEEPS.TEMPRATURE_TEST) //Temprature
                        {
                            int count = 0;
                            double dutValue = DUT.ReadTemprature();
                            while (dutValue == 0 && count < 30)
                            {
                                Conv.delay_ms(1000);
                                dutValue = DUT.ReadTemprature();
                                ++count;
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + count.ToString() + "]";
                            }
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = numTemprature.Value.ToString("0.0") + "±5";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue.ToString("0.0");
                            testOK = (Math.Abs((double)numTemprature.Value - dutValue) < 5);
                        }
                        else if (test_steep == (int)RELAY_STEEPS.GPIO_OUTPUT) //
                        {
                            //Tắt tất cả rơ le
                            for (int i = 0; i < 11; i++)
                                DUT.SetOutput(i + 1, false);
                            relayBoards1.ReadInputImediate(); //Đọc ngay lập tức trạng thái đầu vào
                            if (relayBoards1.GetInputState(1) && relayBoards1.GetInputState(2) && relayBoards1.GetInputState(3) && relayBoards1.GetInputState(4) && relayBoards1.GetInputState(5) && relayBoards1.GetInputState(6) && relayBoards1.GetInputState(7) && relayBoards1.GetInputState(8) && relayBoards1.GetInputState(9) && relayBoards1.GetInputState(10) && relayBoards1.GetInputState(11))
                            {
                                testOK = true;
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(1, true);
                                    if (!relayBoards1.GetInputState(10, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(2, true);
                                    Conv.delay_ms(100);
                                    if (!relayBoards1.GetInputState(11, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(3, true);
                                    if (!relayBoards1.GetInputState(2, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(4, true);
                                    if (!relayBoards1.GetInputState(3, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(5, true);
                                    if (!relayBoards1.GetInputState(1, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(6, true);
                                    if (!relayBoards1.GetInputState(5, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(7, true);
                                    if (!relayBoards1.GetInputState(6, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(8, true);
                                    if (!relayBoards1.GetInputState(4, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(9, true);
                                    if (!relayBoards1.GetInputState(8, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(10, true);
                                    if (!relayBoards1.GetInputState(9, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                                if (testOK)
                                {
                                    testOK = DUT.SetOutput(11, true);
                                    if (!relayBoards1.GetInputState(7, true))
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    else
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                }
                            }
                        }
                        else if (test_steep == (int)RELAY_STEEPS.GPIO_INPUT) //
                        {
                            int inpValue;
                            relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra
                            inpValue = DUT.ReadAllInput();
                            if (inpValue == 0)
                            {
                                testOK = true;
                                for (int i = 0; i < 9; i++)
                                {
                                    relayBoards1.SetOutput(i + 1, true);
                                    inpValue = DUT.ReadAllInput();
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                    if ((inpValue & (1 << i)) == 0)
                                    {
                                        testOK = false;
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    }
                                    else
                                        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                }
                                relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra
                                inpValue = DUT.ReadAllInput();
                            }
                        }
                        else if (test_steep == (int)RELAY_STEEPS.VOLTAGE_AC220V) //
                        {
                            if (powerBoard1.SetOutput(1, true, 1000)) //Chờ cho điện áp ổn định
                            {
                                int count = 0;
                                long dutValue = -1;
                                dutValue = DUT.ReadACVoltage();
                                while (!testOK && count < 6)
                                //while (count < 6)
                                {
                                    Conv.delay_ms(1000);
                                    dutValue = DUT.ReadACVoltage();
                                    ++count;
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + count.ToString() + "]";

                                    long Ua1 = (dutValue >> 32) & 0xFFFF;
                                    long Ub1 = (dutValue >> 16) & 0xFFFF;
                                    long Uc1 = dutValue & 0xFFFF;
                                    //if (!testOK)
                                    //{
                                    //    if (spM931.Ua > (double)160 && spM931.Ub > (double)160 && spM931.Uc > (double)160)
                                    //        testOK = (Math.Abs(Ua1 - spM931.Ua) < 5) && (Math.Abs(Ub1 - spM931.Ub) < 5) && (Math.Abs(Uc1 - spM931.Uc) < 5);
                                    //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = Ua1.ToString() + "|" + Ub1.ToString() + "|" + Uc1.ToString();
                                    //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + spM931.Ua.ToString() + "|" + spM931.Ub.ToString() + "|" + spM931.Uc.ToString() + "]±5VAC";
                                    //}
                                    if (spM931.Ua > (double)160 && spM931.Ub > (double)160 && spM931.Uc > (double)160)
                                        testOK = (Math.Abs(Ua1 - spM931.Ua) < 5) && (Math.Abs(Ub1 - spM931.Ub) < 5) && (Math.Abs(Uc1 - spM931.Uc) < 5);
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = Ua1.ToString() + "|" + Ub1.ToString() + "|" + Uc1.ToString();
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + spM931.Ua.ToString() + "|" + spM931.Ub.ToString() + "|" + spM931.Uc.ToString() + "]±5VAC";
                                    AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text + ": [" + spM931.Ua.ToString() + "|" + spM931.Ub.ToString() + "|" + spM931.Uc.ToString() + "] [" + Ua1.ToString() + "|" + Ub1.ToString() + "|" + Uc1.ToString() + "]");
                                }
                                //long Ua = (dutValue >> 32) & 0xFFFF;
                                //long Ub = (dutValue >> 16) & 0xFFFF;
                                //long Uc = dutValue & 0xFFFF;
                                //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = Ua.ToString() + "|" + Ub.ToString() + "|" + Uc.ToString();
                                //dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + spM931.Ua.ToString() + "|" + spM931.Ub.ToString() + "|" + spM931.Uc.ToString() + "]±5VAC";
                                //if(spM931.Ua > (double)160 && spM931.Ub > (double)160 && spM931.Uc > (double)160)
                                //testOK = (Math.Abs(Ua - spM931.Ua) < 5) && (Math.Abs(Ub - spM931.Ub) < 5) && (Math.Abs(Uc - spM931.Uc) < 5  );
                            }
                            //Tắt điện áp cao:
                            powerBoard1.SetOutput(1, false);
                        }
                        else if (test_steep == (int)RELAY_STEEPS.VOLTAGE_DC500V) //
                        {

                            if (powerBoard1.SetOutput(2, true) && powerBoard1.SetOutput(3, true, 1000))
                            {
                                int count = 0;
                                long dutValue = -1;
                                while (!testOK && count < 5)
                                {
                                    Conv.delay_ms(500);
                                    count++;
                                    dutValue = DUT.ReadDCVoltage();
                                    if (dutValue > -1)
                                    {
                                        long HV1 = (dutValue >> 16) & 0xFFFF;
                                        long HV2 = dutValue & 0xFFFF;
                                        testOK = (Math.Abs(HV1 - autonicsMT41.PValue) < 15) && (Math.Abs(HV2 - autonicsMT41.PValue) < 15);
                                    }
                                } 
                                if (dutValue > -1)
                                {
                                    long HV1 = (dutValue >> 16) & 0xFFFF;
                                    long HV2 = dutValue & 0xFFFF;
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = HV1.ToString() + "|" + HV2.ToString();
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = autonicsMT41.PValue.ToString() + "±10VDC";
                                    testOK = (Math.Abs(HV1 - autonicsMT41.PValue) < 10) && (Math.Abs(HV2 - autonicsMT41.PValue) < 10);
                                }
                            }
                            //Tắt điện áp cao:
                            powerBoard1.SetOutput(2, false);
                            powerBoard1.SetOutput(3, false);
                        }
                        else if (test_steep == (int)RELAY_STEEPS.INSULATION_HV1) //
                        {
                            if (powerBoard1.SetOutput(2, true, 2000)) //Cấp điện áp 500VDC
                            {
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[50±1|20±2|150±6]kΩ";
                                //int Total = 0;
                                //for (int i=0; i<5; i++)
                                //{
                                //    int temp = DUT.ReadHV1InsulationInternal();
                                //    Total += temp;
                                //}
                                //double intIns = ((double)Total / 5);
                                int[] intIns = DUT.ReadHV1InsulationInternal();
                                //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns.ToString("0.0");
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns[0].ToString() + "|" + intIns[1].ToString();
                                Conv.delay_ms(1000);
                                if (relayBoards1.SetOutput(14, true))
                                {
                                    //Total = 0;
                                    //for(int i = 0; i < 5; i++)
                                    //{
                                    //    int temp = DUT.ReadHV1LowInsulation();
                                    //    Total += temp;
                                    //}
                                    //double lowIns = ((double)Total / 5);
                                    Conv.delay_ms(1000);
                                    int lowIns = DUT.ReadHV1LowInsulation();
                                    //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns.ToString() + "|" + lowIns.ToString("0.0");
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns[0].ToString() + "|" + intIns[1].ToString() + "|" + lowIns.ToString();
                                    relayBoards1.SetOutput(14, false);
                                    Conv.delay_ms(1000);
                                    relayBoards1.SetOutput(13, true);

                                    //Total = 0;
                                    //for(int i= 0; i<5; i++)
                                    //{
                                    //    int temp = DUT.ReadHV1HighInsulation();
                                    //    Total += temp;
                                    //}
                                    //double higIns = ((double)Total / 5);
                                    Conv.delay_ms(1000);
                                    int higIns = DUT.ReadHV1HighInsulation();
                                    //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns.ToString("0.0") + "|" + lowIns.ToString("0.0") + "|" + higIns.ToString("0.0");
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns[0].ToString() + "|" + intIns[1].ToString() + "|" + lowIns.ToString() + "|" + higIns.ToString();
                                    testOK = (Math.Abs(intIns[0] - 50) <= 1) && (Math.Abs(intIns[1] - 50) <= 1) && (Math.Abs(lowIns - 20) <= 2) && (Math.Abs(higIns - 150) <= 6);
                                }

                            }
                            //Tắt điện áp cao:
                            powerBoard1.SetOutput(2, false);
                            //Tắt rơ le nối trở:
                            relayBoards1.SetOutput(13, false);
                            relayBoards1.SetOutput(14, false);
                        }
                        else if (test_steep == (int)RELAY_STEEPS.INSULATION_HV2) //
                        {
                            if (powerBoard1.SetOutput(3, true, 2000))
                            {

                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[50±1|20±2|150±6]kΩ";
                                //int Total = 0;
                                //for (int i = 0; i < 5; i++)
                                //{
                                //    int temp = DUT.ReadHV2InsulationInternal();
                                //    Total += temp;
                                //}
                                //double intIns = ((double)Total / 5);
                                int[] intIns = DUT.ReadHV2InsulationInternal();
                                //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns.ToString("0.0");
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns[0].ToString() + "|" + intIns[1].ToString();
                                Conv.delay_ms(1000);

                                if (relayBoards1.SetOutput(16, true))
                                {
                                    //Total = 0;
                                    //for (int i = 0; i < 5; i++)
                                    //{
                                    //    int temp = DUT.ReadHV2LowInsulation();
                                    //    Total += temp;
                                    //}
                                    //double lowIns = ((double)Total / 5);
                                    Conv.delay_ms(1000);
                                    int lowIns = DUT.ReadHV2LowInsulation();
                                    //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns.ToString("0.0") + "|" + lowIns.ToString("0.0");
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns[0].ToString() + "|" + intIns[1].ToString() + "|" + lowIns.ToString();
                                    relayBoards1.SetOutput(16, false);
                                    Conv.delay_ms(1000);
                                    relayBoards1.SetOutput(15, true);
                                    //Total = 0;
                                    //for (int i = 0; i < 5; i++)
                                    //{
                                    //    int temp = DUT.ReadHV2HighInsulation();
                                    //    Total += temp;
                                    //}
                                    //double higIns = ((double)Total / 5);
                                    Conv.delay_ms(1000);
                                    int higIns = DUT.ReadHV2HighInsulation();
                                    //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns.ToString("0.0") + "|" + lowIns.ToString("0.0") + "|" + higIns.ToString("0.0");
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = intIns[0].ToString() + "|" + intIns[1].ToString() + "|" + lowIns.ToString() + "|" + higIns.ToString();
                                    testOK = (Math.Abs(intIns[0] - 50) <= 1) && (Math.Abs(intIns[1] - 50) <= 1) && (Math.Abs(lowIns - 20) <= 2) && (Math.Abs(higIns - 150) <= 6);
                                }
                            }
                            //Tắt điện áp cao:
                            powerBoard1.SetOutput(3, false);
                            //Tắt rơ le nối trở:
                            relayBoards1.SetOutput(15, false);
                            relayBoards1.SetOutput(16, false);
                        }
                        else if (test_steep == (int)RELAY_STEEPS.UART_RS485_TEST) //
                        {
                            testOK = DUT.RS485Test();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)RELAY_STEEPS.UART_DEBUG_TEST) //
                        {
                            testOK = DUT.UARTDebugTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)RELAY_STEEPS.STOP_TEST) //
                        {
                            testOK = DUT.StopTest();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "[EnterTestSteep]");
                    }
                    ////////////////TEST//////////////////
                    //////////////////////////////////////
                    //testOK = true;
                    //////////////////////////////////////


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
            else if (btnReady.Enabled == true)
                SetControlStatus(STATUS.Ready);
        }
    }

    private void EnterTestSteepRU(int test_steep)
    {
        if (test_steep > -1 && test_steep < dgvTestList.Rows.Count)
        {
            while (test_steep < dgvTestList.Rows.Count)
                try
                {
                    SetControlStatus(STATUS.Start);

                    thread_ended = false;
                    thread_started = true;

                    while (thread_started)
                    {


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            thread_ended = true;
            if (btnReady.Enabled == false)
                SetControlStatus(STATUS.Stop);
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
        panelHeader.Height = panelLeft.Visible ? 384 : 240;
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
                powerBoard1.SetOutput(4, false); //Tắt nguồn
                relayBoards1.SetAllState(false); //Tắt rơ-le

                cboOperator.Enabled = btnReady.Enabled = btnConfig.Enabled = btnReload.Enabled = true;

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

    string STR_Num = "1234567890";
    private void cboOperator_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!Char.IsControl(e.KeyChar))
        {
            if (!STR_Num.Contains(e.KeyChar)) e.Handled = true;
        }
    }
}
