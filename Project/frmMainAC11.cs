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
    DateTime beginTestDT = DateTime.Now;

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

        powerBoard1.Start(ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1"));
        ReInitBoardType();

        spM931.Start(ini.IniReadValue("COMMUNICATION", "SPM3MeterPort", "COM3"));
        spM932.Start(ini.IniReadValue("COMMUNICATION", "SPM3MeterPort2", "COM4"));
        dcvMeter.Start(ini.IniReadValue("COMMUNICATION", "DCVoltageMeterPort", "COM5"), "VDC", 0.01);
        dcaMeter.Start(ini.IniReadValue("COMMUNICATION", "DCVoltageMeterPort2", "COM6"), "A", 0.001);


        DUT.Start();

        //tcpiP_Object1.Start("192.168.1.1");

        panelLeft.Visible = false;
        this.WindowState = FormWindowState.Maximized;
        btnReload_Click(null, null);
        SetControlStatus(STATUS.Ready);
        timer1s.Enabled = true;
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AppConfig.ini");

        //tcpiP_Object1.Stop();

        dcvMeter.Stop();
        powerBoard1.Stop();
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
        /////)
        
        /////////////

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
        lblTime.Text = DateTime.Now.ToString("HH:mm:ss");
        if (btnReady.Enabled == false)
        {
            TimeSpan ts = DateTime.Now - startTime;
            lblTestTime.Text = ((int)(ts.TotalSeconds / 60)).ToString("00") + ":" + ((int)(ts.TotalSeconds % 60)).ToString("00");
        }
        lbPassRate.Text = PassCount.ToString() + "/" + TotalCount.ToString();
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

            //if (!isTestOffline)
            //{
            //    AddLogWindow("Kiểm tra trạng thái của mã: " + txtSerialNo.Text + " trên MES");
            //    OutputGetSNStatus res = RestApiHelper.GetSNStatus(txtSerialNo.Text, MachineName);
            //    if (res != null)
            //    {
            //        //MessageBox.Show(res.SN + "\r\n" + res.MachineName + "\r\n" + res.Confirm + "\r\n" + res.ErrorCode);
            //        if (res.Confirm == "OK")
            //        {
            //            //MessageBox.Show("Mã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            EnterTestSteep(0);
            //        }
            //        else
            //        {

            //            MessageBox.Show("Kiểm tra trạng thái mã serial trên MES\r\nMã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Lỗi lấy trạng thái của mã Serial Number trên MES!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //}
            //else
            //    EnterTestSteep(0);
            if (!isTestOffline)
            {
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
        if(dgvTestList.Rows.Count > 0 && btnStop.Text == "STOP" && !thread_started)
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

        string rlboard = "1|DO1:LEAK|DO2:12VDC_1.5A|DO3:12VDC_1A|DO4:12VDC_SHORT|DO5:TPPE_IN|DO6:TP79_TP80|DO7:TP78_500mA|DO8:TP53_EMERGENCY|DI1:TP104_RED|DI2:TP102_GREEN|DI3:TP103_BLUE|DI4:TP57_RESET";
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
                frmConfigMainAC11 frm = new frmConfigMainAC11();
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
                //// Lưu log file
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
                }

                File.WriteAllText(path_name, sBuff, System.Text.Encoding.UTF8);
                

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

                
                sBuffSum = txtSerialNo.Text + "," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "," + lblTestTime.Text + "," + "" + "," + "Pass" +  "\r\n";

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
                
                //AddLogWindow("Lưu firmware version của mã: " + txtSerialNo.Text + " lên MES");
                //try
                //{
                //    //Update firmware version
                //    //cbi du lieu
                //    string stm_fw_version = Conv.atos(dgvTestList.Rows[(int)MAINAC_STEEPS.SOFTWARE_VERSION].Cells["ReadValue"].Value);

                //    OutputUpdateFirmwareInfo res = RestApiHelper.UpdateFirmwareInfo(txtSerialNo.Text, MachineName, "TESTTOOL", "-1", stm_fw_version);
                //    if (res != null)
                //    {
                //        if (res.Result == "OK")
                //        {
                //            AddLogWindow("Cập nhật firmware version lên MES thành công!");

                //            AddLogWindow("Lưu kết quả test của mã: " + txtSerialNo.Text + " lên MES");
                //            try
                //            {
                //                //cbi du lieu
                //                DataTable dtab = new DataTable();
                //                dtab.Columns.Add("TestName");
                //                dtab.Columns.Add("LowValue");
                //                dtab.Columns.Add("ReadValue");
                //                dtab.Columns.Add("HighValue");
                //                dtab.Columns.Add("Result");
                //                dtab.Columns.Add("TestTime");

                //                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                //                {
                //                    dtab.Rows.Add();
                //                    dtab.Rows[r]["TestName"] = Conv.atos(dgvTestList.Rows[r].Cells["TestName"].Value);
                //                    dtab.Rows[r]["LowValue"] = "";
                //                    dtab.Rows[r]["ReadValue"] = Conv.atos(dgvTestList.Rows[r].Cells["ReadValue"].Value).Replace("\"", "");
                //                    dtab.Rows[r]["HighValue"] = "";
                //                    dtab.Rows[r]["Result"] = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);
                //                    dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);

                //                }

                //                // Insert

                //                OutputInsertPassFailDetailTestCase resInsert = RestApiHelper.InsertPassFailDetailTestCase(txtSerialNo.Text, MachineName, cboOperator.Text, dtab);
                //                if (resInsert != null)
                //                {
                //                    if (resInsert.Status == "OK")
                //                    {
                //                        AddLogWindow("Ghi MES thành công!");

                //                        btnSave.Enabled = false;
                //                        btnTest.Enabled = false;
                //                        btnNext.Enabled = false;
                //                        btnStop_Click(null, null);
                //                        //panSettings.Enabled = true;
                //                        //panTest.Enabled = false;
                //                        btnReady.Enabled = true;
                //                        txtSerialNo.Text = "";
                //                        txtSerialNo.Focus();

                //                        PassCount += 1;
                //                        TotalCount += 1;
                //                    }
                //                    else
                //                    {
                //                        AddLogWindow("Ghi MES không thành công\r\nStatus: " + resInsert.Status + "\r\nError Code: " + resInsert.ErrorCode);
                //                        MessageBox.Show("Ghi MES không thành công\r\nStatus: " + resInsert.Status + "\r\nError Code: " + resInsert.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //                    }
                //                }
                //                else
                //                {
                //                    AddLogWindow("Ghi MES không thành công");
                //                    MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //                }

                //            }
                //            catch (Exception ex)
                //            {
                //                Global.WriteLogFile("[Save to MES] - " + ex.Message);
                //            }
                //        }
                //        else
                //        {
                //            AddLogWindow("Cập nhật firmware version lên MES không thành công\r\nStatus: " + res.SN + "\r\nMachineName: " + res.MachineName + "\r\nResult: " + res.Result + "\r\nError Code: " + res.ErrorCode + "\r\nUserName: " + res.UserName + "\r\nFirmware: " + res.Firmware + "\r\nFirmwareVersion: " + res.FirmwareVersion);
                //            MessageBox.Show("Cập nhật firmware version lên MES không thành công\r\nSN: " + res.SN + "\r\nMachineName: " + res.MachineName + "\r\nResult: " + res.Result + "\r\nError Code: " + res.ErrorCode + "\r\nUserName: " + res.UserName + "\r\nFirmware: " + res.Firmware + "\r\nFirmwareVersion: " + res.FirmwareVersion, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        }
                //    }
                //    else
                //    {
                //        AddLogWindow("Cập nhật firmware version lên MES không thành công");
                //        MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Global.WriteLogFile("[Update Fiwmware Version to MES] - " + ex.Message);
                //}
                
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

                if(isTestOffline)
                {
                    btnSave.Enabled = false;
                    btnTest.Enabled = false;
                    btnNext.Enabled = false;
                    btnStop_Click(null, null);
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

                            btnSave.Enabled = false;
                            btnTest.Enabled = false;
                            btnNext.Enabled = false;
                            btnStop_Click(null, null);
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
                //// Lưu log file
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
                }

                File.WriteAllText(path_name, sBuff, System.Text.Encoding.UTF8);


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

                if (isTestOffline)
                {
                    btnSave.Enabled = false;
                    btnTest.Enabled = false;
                    btnNext.Enabled = false;
                    btnStop_Click(null, null);
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
        SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
    }


    //enum MAINAC_STEEPS
    //{
    //    START_TEST = (int)0,
    //    SERIAL_NUMBER, //1
    //    SOFTWARE_VERSION,//2
    //    TONY_VERSION,//3

    //    TONY_VERSION2,
    //    TONY_VERSION3,
    //    TONY_VERSION4,
    //    TONY_VERSION5,
    //    TONY_VERSION6,
    //    TONY_VERSION7,
    //    TONY_VERSION8,
    //    TONY_VERSION9,
    //    TONY_VERSION10,
    //    TONY_VERSION11,
    //    TONY_VERSION12,
    //    TONY_VERSION13,
    //    TONY_VERSION14,
    //    TONY_VERSION15,
    //    TONY_VERSION16,
    //    TONY_VERSION17,
    //    TONY_VERSION18,
    //    TONY_VERSION19,
    //    TONY_VERSION20,
    //    TONY_VERSION21,
    //    TONY_VERSION22,
    //    TONY_VERSION23,
    //    TONY_VERSION24,
    //    TONY_VERSION25,
    //    TONY_VERSION26,
    //    TONY_VERSION27,
    //    TONY_VERSION28,
    //    TONY_VERSION29,
    //    TONY_VERSION30,
    //    TONY_VERSION31,
    //    TONY_VERSION32,
    //    TONY_VERSION33,
    //    TONY_VERSION34,
    //    TONY_VERSION35,
    //    TONY_VERSION36,
    //    TONY_VERSION37,
    //    TONY_VERSION38,
    //    TONY_VERSION39,
    //    TONY_VERSION40,
    //    TONY_VERSION41,
    //    TONY_VERSION42,
    //    TONY_VERSION43,
    //    TONY_VERSION44,
    //    TONY_VERSION45,
    //    TONY_VERSION46,
    //    TONY_VERSION47,
    //    TONY_VERSION48,
    //    TONY_VERSION49,
    //    TONY_VERSION50,

    //    TEMPRATURE_TEST,//4
    //    FLASH_TEST,//5
    //    ADC_TEST,//6
    //    RFID_UART_TEST,//7
    //    GPIO_OUTPUT,//8
    //    GPIO_INPUT,//9
    //    AC_PARAMETER,//10
    //    INPUT_CAPTURE,//11
    //    RESIDUAL_CURRENT,//12
    //    RESIDUAL_PROTECT,//12
    //    OVER_VOLTAGE,//13
    //    UNDER_VOLTAGE,//14
    //    EARTH_CONNECT,//14
    //    DC12V_OUT,//14
    //    DC12V_PROTECT,//14
    //    STOP_TEST//15
    //}

    //string[] MAINAC_STEEP_DESCRIPTION = {
    //    "Vào chế độ kiểm tra", //0
    //    "Kiểm tra số chế tạo với tem QR", //1
    //    "Phiên bản phần mềm được nạp", //2
    //    "Phiên bản mạch đo TONY", //3

    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",
    //    "Phiên bản mạch đo TONY",

    //    "Kiểm tra cảm biến nhiệt độ", //4
    //    "Kiểm tra bộ nhớ FLASH mở rộng", //5
    //    "Giá trị đo tại chân ADC", //6
    //    "Kiểm tra cổng kết nối với RFID", //7
    //    "Các đầu ra MCU_LED_RED/GREEN/BLUE/MCU_RESET_PLC", //8
    //    "Kiểm tra đầu vào MCU_EMERGENCY", //9
    //    "Kiểm tra đo lường điện áp 3 pha U±5VAC(I±0.1A)", //10
    //    "Đóng rơ-le tải đầu ra (InputCapture)", //11
    //    "Kiểm tra đo dòng dò", //12
    //    "Kiểm tra bảo vệ dòng dò", //12
    //    "Kiểm tra bảo vệ quá áp", //13
    //    "Kiểm tra bảo vệ thấp áp", //14
    //    "Kiểm tra cảnh báo mất nối đất", //14
    //    "Kiểm tra hoạt động nguồn cấp 12VDC", //14
    //    "Kiểm tra bảo vệ nguồn cấp 12VDC", //14
    //    "Thoát chế độ kiểm" //16
    //};

    enum MAINAC_STEEPS
    {
        START_TEST = (int)0,
        SERIAL_NUMBER, //1
        SOFTWARE_VERSION,//2
        TONY_VERSION,//3
        TEMPRATURE_TEST,//4
        FLASH_TEST,//5
        ADC_TEST,//6
        RFID_UART_TEST,//7
        GPIO_OUTPUT,//8
        GPIO_INPUT,//9
        AC_PARAMETER,//10
        INPUT_CAPTURE,//11
        RESIDUAL_CURRENT,//12
        RESIDUAL_PROTECT,//12
        OVER_VOLTAGE,//13
        UNDER_VOLTAGE,//14
        EARTH_CONNECT,//14
        DC12V_OUT,//14
        DC12V_PROTECT,//14
        STOP_TEST//15
    }
    string[] MAINAC_STEEP_DESCRIPTION = {
        "Vào chế độ kiểm tra", //0
        "Kiểm tra số chế tạo với tem QR", //1
        "Phiên bản phần mềm được nạp", //2
        "Phiên bản mạch đo TONY", //3
        "Kiểm tra cảm biến nhiệt độ", //4
        "Kiểm tra bộ nhớ FLASH mở rộng", //5
        "Giá trị đo tại chân ADC", //6
        "Kiểm tra cổng kết nối với RFID", //7
        "Các đầu ra MCU_LED_RED/GREEN/BLUE/MCU_RESET_PLC", //8
        "Kiểm tra đầu vào MCU_EMERGENCY", //9
        "Kiểm tra đo lường điện áp 3 pha U±5VAC(I±0.1A)", //10
        "Đóng rơ-le tải đầu ra (InputCapture)", //11                                                  
        "Kiểm tra đo dòng dò", //12
        "Kiểm tra bảo vệ dòng dò", //12
        "Kiểm tra bảo vệ quá áp", //13
        "Kiểm tra bảo vệ thấp áp", //14
        "Kiểm tra cảnh báo mất nối đất", //14
        "Kiểm tra hoạt động nguồn cấp 12VDC", //14
        "Kiểm tra bảo vệ nguồn cấp 12VDC", //14
        "Thoát chế độ kiểm" //16
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

        string[] steepNames = Enum.GetNames(typeof(MAINAC_STEEPS));

        dgvTestList.Rows.Clear();

        for (int i = 0; i < steepNames.Length; i++)
        {
            dgvTestList.Rows.Add();
            dgvTestList.Rows[i].Cells["TestOrder"].Value = i + 1;
            dgvTestList.Rows[i].Cells["TestName"].Value = steepNames[i];
            dgvTestList.Rows[i].Cells["Description"].Value = MAINAC_STEEP_DESCRIPTION[i];
        }

        txtTotalSteep.Text = dgvTestList.Rows.Count.ToString();
    }

    private bool OnOffRelayTest(int test_steep)
    {
        bool testOK = false;
        int inpValue;
        //AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + "OFF all relay. Read All Capture");
        ////Đóng tất cả rơ le đầu ra tải:
        //DUT.SetRelayOutput(0, false);
        //DUT.SetRelayOutput(1, false);
        //DUT.SetRelayOutput(2, false, 1000);

        //inpValue = DUT.ReadAllCapture();
        //if (inpValue > -1)
        //{
        //    int f11 = (inpValue >> 16) & 0xFF;
        //    int f21 = (inpValue >> 8) & 0xFF;
        //    int f31 = inpValue & 0xFF;
        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "0 - 5Hz";
        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = f11.ToString() + "|" + f21.ToString() + "|" + f31.ToString();
        //    if(f11 >= 0 && f11 <= 5 && f21 >= 0 && f21 <= 5 && f31 >= 0 && f31 <= 5)
        //    {
        //        AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + "ON all relay. Read All Capture");
        //        //Đóng tất cả rơ le đầu ra tải:
        //        DUT.SetRelayOutput(0, true);
        //        DUT.SetRelayOutput(1, true);
        //        DUT.SetRelayOutput(2, true, 1000);

        //        inpValue = DUT.ReadAllCapture();
        //        if (inpValue > -1)
        //        {
        //            int f1 = (inpValue >> 16) & 0xFF;
        //            int f2 = (inpValue >> 8) & 0xFF;
        //            int f3 = inpValue & 0xFF;
        //            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "50Hz±1";
        //            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = f1.ToString() + "|" + f2.ToString() + "|" + f3.ToString();
        //            testOK = (f1 >= 49 && f1 <= 51 && f2 >= 49 && f2 <= 51 && f3 >= 49 && f3 <= 51);
        //        }

        //        //Cắt tất cả rơ le đầu ra tải:
        //        DUT.SetRelayOutput(0, false);
        //        DUT.SetRelayOutput(1, false);
        //        DUT.SetRelayOutput(2, false);
        //    }
        //}

        //Đóng tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, true);
        DUT.SetRelayOutput(1, true);
        DUT.SetRelayOutput(2, true, 1000);

        inpValue = DUT.ReadAllCapture();
        if (inpValue > -1)
        {
            int f1 = (inpValue >> 16) & 0xFF;
            int f2 = (inpValue >> 8) & 0xFF;
            int f3 = inpValue & 0xFF;
            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "50Hz±1";
            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = f1.ToString() + "|" + f2.ToString() + "|" + f3.ToString();
            testOK = (f1 >= 49 && f1 <= 51 && f2 >= 49 && f2 <= 51 && f3 >= 49 && f3 <= 51);
        }

        //Cắt tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, false);
        DUT.SetRelayOutput(1, false);
        DUT.SetRelayOutput(2, false);

        return testOK;
    }

    private bool ResidualTest(int test_steep)
    {
        bool testOK = false;
        int inpValue;
        double LOAD_LEVEL = 0.1;
        int s_count = 0;
        //Đóng tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, true);
        DUT.SetRelayOutput(1, true);
        DUT.SetRelayOutput(2, true);
        powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải

        //Chờ xác nhận dòng điện
        while (!testOK && ++s_count < 20)
        {
            Conv.delay_ms(500);
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
            if (spM932.CommState)
            {
                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);
            }
        }

        if (testOK)
        {
            testOK = false;
            relayBoards1.SetOutput(1, true, 1000); //Đóng điện trở mô phỏng dòng dò 10k, chờ DUT xác nhận
            inpValue = DUT.ReadResidualCurrent();
            if (inpValue > -1)
            {
                double sv = spM931.Ua / 9;
                double pv = ((inpValue >> 8) & 0xFF);
                //testOK = true;
                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Đo dòng rò";
                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = ((inpValue >> 8) & 0xFF).ToString() + "|" + (inpValue & 0xFF).ToString() + "[mA]";
                testOK = true;
                //powerBoard1.SetOutput(3, true, 2000); //Ngắn mạch tạo dòng bảo vệ
                //testOK = (spM932.Ia < LOAD_LEVEL); //Nếu mất dòng tải chứng tỏ đã bảo vệ
            }
        }

        powerBoard1.SetOutput(3, false);
        relayBoards1.SetOutput(3, false); //Cắt điện trở mô phỏng dòng dò bảo vệ
        powerBoard1.SetOutput(2, false); //Cắt tải đầu ra
        //Cắt tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, false);
        DUT.SetRelayOutput(1, false);
        DUT.SetRelayOutput(2, false);

        relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra

        return testOK;
    }

    private bool ResidualProtectTest(int test_steep)
    {
        bool testOK = false;
        double LOAD_LEVEL = 0.1;
        int s_count = 0;
        //Đóng tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, true);
        DUT.SetRelayOutput(1, true);
        DUT.SetRelayOutput(2, true);
        powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải

        //Chờ xác nhận dòng điện
        while (!testOK && ++s_count < 20)
        {
            Conv.delay_ms(500);
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
            if (spM932.CommState)
            {
                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Bảo vệ cắt rơ-le";
                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);
            }
        }

        if (testOK)
        {
            testOK = false;
            relayBoards1.SetOutput(1, true, 1000); //Đóng điện trở mô phỏng dòng dò 10k, chờ DUT xác nhận
            powerBoard1.SetOutput(3, true, 2000); //Ngắn mạch tạo dòng bảo vệ
            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
            testOK = (spM932.Ia < LOAD_LEVEL); //Nếu mất dòng tải chứng tỏ đã bảo vệ
        }

        powerBoard1.SetOutput(3, false);
        relayBoards1.SetOutput(3, false); //Cắt điện trở mô phỏng dòng dò bảo vệ
        powerBoard1.SetOutput(2, false); //Cắt tải đầu ra
        //Cắt tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, false);
        DUT.SetRelayOutput(1, false);
        DUT.SetRelayOutput(2, false);

        relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra

        return testOK;
    }

    private bool TestOverVoltage(int test_steep)
    {
        bool testOK = false;
        double LOAD_LEVEL = 0.1;
        int s_count = 0;
        //Đóng tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, true);
        DUT.SetRelayOutput(1, true);
        DUT.SetRelayOutput(2, true);
        powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải

        //Chờ xác nhận dòng điện
        while (!testOK && ++s_count < 20)
        {
            Conv.delay_ms(500);
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
            if (spM932.CommState)
            {
                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);
            }
        }

        if (testOK)
        {
            frmShowChoice frm = new frmShowChoice();
            frm.lblTestSteep.Text = txtTestSteep.Text;
            frm.lblTestName.Text = txtTestName.Text;
            frm.lblMessage.Text = "Vặn biến áp vô cấp để tăng điện áp tới khi tất cả các đầu ra đều tắt\r\n";
            frm.lblMessage.Text += "DUT hiển thị thông tin '" + DUT.STATUS_OVER_VOLTAGE + "'\r\n";

            frm.btnSetNG.Enabled = true;
            //frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray; 
            frm.Show();

            while (thread_started && frm.State == 0)
            {
                Conv.delay_ms(100);
                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
                if (spM932.CommState)
                {
                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = spM932.Ua.ToString() + "|" + spM932.Ua.ToString() + "|" + spM932.Uc.ToString() + "[V]";
                }
                if (DUT.AlertState == DUT.STATUS_OVER_VOLTAGE)
                {
                    Conv.delay_ms(1000);
                    frm.State = 1;
                }
            }

            testOK = (frm.State == 1);
            frm.Close();

            frm = new frmShowChoice();
            frm.lblTestSteep.Text = txtTestSteep.Text;
            frm.lblTestName.Text = txtTestName.Text;
            frm.lblMessage.Text = "Vặn biến áp vô cấp về chế độ hoạt động bình thường để kết thúc";
            frm.btnClose.Enabled = true;
            //frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray;
            //frm.ShowDialog();
            frm.Show();
            while (thread_started && frm.State == 0)
            {
                Conv.delay_ms(100);
                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
                if (DUT.AlertState == DUT.STATUS_NORMAL_MODE)
                {
                    frm.State = 1;
                    Conv.delay_ms(1000);
                }
                if (spM931.CommState)
                {
                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM931.Ua.ToString() + "|" + spM931.Ua.ToString() + "|" + spM931.Uc.ToString() + "[V]";
                }

            }
            frm.Close();
        }
        else
        {
            AddLogWindow("Đóng tải không thành công.\r\nKhông đọc được dòng điện tải");
        }

        powerBoard1.SetOutput(2, false); //Cắt tải đầu ra, chờ có dòng tải
        return testOK;
    }

    private bool TestUnderVoltage(int test_steep)
    {
        bool testOK = false;
        double LOAD_LEVEL = 0.1;
        int s_count = 0;
        //Đóng tất cả rơ le đầu ra tải:
        DUT.SetRelayOutput(0, true);
        DUT.SetRelayOutput(1, true);
        DUT.SetRelayOutput(2, true);
        powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải

        //Chờ xác nhận dòng điện
        while (!testOK && ++s_count < 20)
        {
            Conv.delay_ms(500);
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
            if (spM932.CommState)
            {
                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);
            }
        }

        if (testOK)
        {
            frmShowChoice frm = new frmShowChoice();
            frm.lblTestSteep.Text = txtTestSteep.Text;
            frm.lblTestName.Text = txtTestName.Text;
            frm.lblMessage.Text = "Vặn biến áp vô cấp để giảm điện áp tới khi tất cả các đầu ra đều tắt\r\n";
            frm.lblMessage.Text += "DUT hiển thị thông tin '" + DUT.STATUS_UNDER_VOLTAGE + "'\r\n";

            frm.btnSetNG.Enabled = true;
            //frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray; 
            frm.Show();

            while (thread_started && frm.State == 0)
            {
                Conv.delay_ms(100);
                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
                if (spM932.CommState)
                {
                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = spM932.Ua.ToString() + "|" + spM932.Ua.ToString() + "|" + spM932.Uc.ToString() + "[V]";
                }
                if (DUT.AlertState == DUT.STATUS_UNDER_VOLTAGE)
                {
                    Conv.delay_ms(1000);
                    frm.State = 1;
                }
            }

            testOK = (frm.State == 1);
            frm.Close();

            frm = new frmShowChoice();
            frm.lblTestSteep.Text = txtTestSteep.Text;
            frm.lblTestName.Text = txtTestName.Text;
            frm.lblMessage.Text = "Vặn biến áp vô cấp về chế độ hoạt động bình thường để kết thúc";
            frm.btnClose.Enabled = true;
            //frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray;
            //frm.ShowDialog();
            frm.Show();
            while (thread_started && frm.State == 0)
            {
                Conv.delay_ms(100);
                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
                if (DUT.AlertState == DUT.STATUS_NORMAL_MODE)
                {
                    frm.State = 1;
                    Conv.delay_ms(1000);
                }
                if (spM931.CommState)
                {
                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM931.Ua.ToString() + "|" + spM931.Ua.ToString() + "|" + spM931.Uc.ToString() + "[V]";
                }

            }
            frm.Close();
        }
        else
        {
            AddLogWindow("Đóng tải không thành công.\r\nKhông đọc được dòng điện tải");
        }

        powerBoard1.SetOutput(2, false); //Cắt tải đầu ra, chờ có dòng tải
        return testOK;
    }

    private bool DC12VOutTest(int test_steep)
    {
        bool testOK = false;
        relayBoards1.SetAllState(false); //Cắt tất cả rơ le

        dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Kiểm tra không tải";
        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dcvMeter.PValue.ToString() + "V/" + dcaMeter.PValue.ToString() + "A";
        dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");

        if (dcvMeter.PValue > 11 && dcvMeter.PValue < 13)
        {
            /*
            //Nối tải 8omh:
            relayBoards1.SetOutput(2, true, 1000);
            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Đóng tải 1.5A";
            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dcvMeter.PValue.ToString() + "V/" + dcaMeter.PValue.ToString() + "A";
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
            
            testOK = (dcvMeter.PValue > 11 && dcvMeter.PValue < 13 && dcaMeter.PValue > 1.4 && dcaMeter.PValue < 1.6);
            */

            //Nối tải 12omh: (1A)
            relayBoards1.SetOutput(3, true, 1000);
            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Đóng tải 1A";
            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dcvMeter.PValue.ToString() + "V/" + dcaMeter.PValue.ToString() + "A";
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");

            testOK = (dcvMeter.PValue > 11 && dcvMeter.PValue < 13 && dcaMeter.PValue > 0.9 && dcaMeter.PValue < 1.1);
            //if (dcvMeter.PValue > 11 && dcvMeter.PValue < 13 && dcaMeter.PValue > 0.9 && dcaMeter.PValue < 1.1)
            //{
            //    //Nối tải 8omh: (2.5A)
            //    relayBoards1.SetOutput(2, true, 1000);
            //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Đóng tải 2.5A";
            //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dcvMeter.PValue.ToString() + "V/" + dcaMeter.PValue.ToString() + "A";
            //    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
                
            //    testOK = (dcvMeter.PValue < 1);
            //}         
        }
        else
            MessageBox.Show("Lỗi phát áp 12VDC");
        relayBoards1.SetAllState(false); //Cắt tất cả rơ le
        return testOK;
    }

    private bool DC12VProtectTest(int test_steep)
    {
        bool testOK = false;
        relayBoards1.SetAllState(false); //Cắt tất cả rơ le

        dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Kiểm tra không tải";
        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dcvMeter.PValue.ToString() + "V/" + dcaMeter.PValue.ToString() + "A";
        dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");

        if (dcvMeter.PValue > 11 && dcvMeter.PValue < 13)
        {
            
            //Nối tải 12omh:
            relayBoards1.SetOutput(3, true, 1000);
            //Nối thêm tải 8ohm
            relayBoards1.SetOutput(2, true, 1000);

            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Đóng tải 2.5A";
            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dcvMeter.PValue.ToString() + "V/" + dcaMeter.PValue.ToString() + "A";
            dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
        
            testOK = (dcvMeter.PValue < 1);
        }
        else
            MessageBox.Show("Lỗi phát áp 12VDC");
        relayBoards1.SetAllState(false); //Cắt tất cả rơ le
        return testOK;
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
                    beginTestDT = DateTime.Now;
                    try
                    {
                        dgvTestList.CurrentCell = dgvTestList.Rows[test_steep].Cells[0];

                        dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = Color.LightBlue;
                        dgvTestList.Rows[test_steep].Cells["BeginTest"].Value = beginTestDT.ToString("yyyy-MM-dd HH:mm:ss");
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
                        if (test_steep == (int)MAINAC_STEEPS.START_TEST) //Start Test
                        {
                            frmShowChoice frm = new frmShowChoice();
                            frm.lblTestSteep.Text = "1";
                            frm.lblTestName.Text = "BƯỚC CHUẨN BỊ";
                            frm.lblMessage.Text = "Kiểm tra các kết nối JIG với máy tính, bật tất cả các Aptomat nguồn 1 pha, 3 pha.\r\n";
                            frm.lblMessage.Text += "\r\nBấm chuột vào 'OK'";
                            frm.btnClose.Enabled = true;
                            frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray;
                            frm.ShowDialog();
                            powerBoard1.SetOutput(4, false, 3000); //Tắt nguồn
                            powerBoard1.SetOutput(4, true, 1000); //Bật nguồn
                            int s_count = 0;
                            bool in1, in2, in3;
                            while (s_count < 30)
                            {
                                //relayBoards1.ReadInputImediate();
                                in1 = relayBoards1.GetInputState(1);
                                in2 = relayBoards1.GetInputState(2);
                                in3 = relayBoards1.GetInputState(3);
                                if (in1 == true && in2 == false && in3 == false)
                                    s_count = 30;
                                else
                                {
                                    s_count += 1;
                                    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");
                                }
                                Conv.delay_ms(1000);
                            }
                            Conv.delay_ms(3000);
                            testOK = DUT.StartTest();
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.SERIAL_NUMBER) //Check Serial
                        {
                            string dutValue = DUT.ReadSerialNumber();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = txtSerialNo.Text;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (txtSerialNo.Text == dutValue);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.SOFTWARE_VERSION) //Software Version
                        {
                            string dutValue = DUT.ReadSoftwareVersion();
                            string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'"));

                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;

                            testOK = (checkValue == dutValue);
                        }
                        //else if (test_steep >= (int)MAINAC_STEEPS.TONY_VERSION && test_steep <= (int)MAINAC_STEEPS.TONY_VERSION50)
                        //{
                        //    string dutValue = DUT.ReadTonyVersion();
                        //    string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='TonyVersion'"));
                        //    if (checkValue != "") dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                        //    testOK = (dutValue == checkValue + "|" + checkValue + "|" + checkValue);
                        //}
                        else if (test_steep == (int)MAINAC_STEEPS.TONY_VERSION)
                        {
                            string dutValue = DUT.ReadTonyVersion();
                            string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='TonyVersion'"));
                            if (checkValue != "") dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (dutValue == checkValue + "|" + checkValue + "|" + checkValue);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.FLASH_TEST) //EXTERNAL FLASH check
                        {
                            testOK = DUT.FlashTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.TEMPRATURE_TEST) //Temprature
                        {
                            double dutValue = DUT.ReadTemprature();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = numTemprature.Value.ToString("0.0") + "±5";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue.ToString("0.0");
                            testOK = (Math.Abs((double)numTemprature.Value - dutValue) < 5);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.ADC_TEST) //
                        {
                            if (relayBoards1.SetOutput(7, true, 1000)) //Đóng điện TP78: 5V/100mA
                            {
                                long dutValue = -1;
                                dutValue = DUT.ReadADC();
                                if (dutValue > -1)
                                {
                                    //double test = 5.0 / 3;
                                    double test = 1.7;
                                    double volt = 3.3 * (double)dutValue / 4096;
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = volt.ToString("0.00") + "VDC";
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = test.ToString("0.0") + "VDC±1";
                                    testOK = (Math.Abs(volt - test) < 1.0);
                                }
                            }
                            relayBoards1.SetAllState(false); //Cắt tất cả rơ le
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.RFID_UART_TEST) //
                        {

                            if (relayBoards1.SetOutput(6, true, 500)) //Nối tắt TX-RX
                            {
                                testOK = DUT.RFID_UART_Test();
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                            }
                            relayBoards1.SetAllState(false); //Cắt tất cả rơ le
                        }
                        //else if (test_steep == (int)MAINAC_STEEPS.AC_PARAMETER) //
                        //{
                        //    //Đóng tất cả rơ le đầu ra tải:
                        //    DUT.SetRelayOutput(0, true);
                        //    DUT.SetRelayOutput(1, true);
                        //    DUT.SetRelayOutput(2, true);
                        //    powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải
                        //    Conv.delay_ms(2000); //Chờ xác nhận dòng điện

                        //    //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                        //    //testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);

                        //    if (DUT.ReadACParameter())
                        //    {
                        //        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = DUT.L1UValue.ToString() + "(" + DUT.L1IValue.ToString() + ")|" + DUT.L2UValue.ToString() + "(" + DUT.L2IValue.ToString() + ")|" + DUT.L3UValue.ToString() + "(" + DUT.L3IValue.ToString() + ")";
                        //        dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + spM931.Ua.ToString() + "(" + spM931.Ia.ToString() + ")|" + spM931.Ub.ToString() + "(" + spM931.Ib.ToString() + ")|" + spM931.Uc.ToString() + "(" + spM931.Ic.ToString() + ")]";
                        //        testOK = (Math.Abs(DUT.L1UValue - spM931.Ua) < 5) && (Math.Abs(DUT.L2UValue - spM931.Ub) < 5) && (Math.Abs(DUT.L3UValue - spM931.Uc) < 5)
                        //            && (Math.Abs(DUT.L1IValue - spM931.Ia) < 0.1) && (Math.Abs(DUT.L2IValue - spM931.Ib) < 0.1) && (Math.Abs(DUT.L3IValue - spM931.Ic) < 0.1);
                        //    }

                        //    powerBoard1.SetOutput(2, false); //Cắt tải đầu ra, chờ có dòng tải
                        //    DUT.SetRelayOutput(0, false);
                        //    DUT.SetRelayOutput(1, false);
                        //    DUT.SetRelayOutput(2, false);
                        //}
                        else if (test_steep == (int)MAINAC_STEEPS.AC_PARAMETER) //
                        {
                            //AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + "Read Tony version.");
                            //string dutValue = DUT.ReadTonyVersion();
                            //string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='TonyVersion'"));

                            //if (dutValue == checkValue + "|" + checkValue + "|" + checkValue)
                            //{
                            //    AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + "OFF all relay. Read AC Parameter");
                            //    DUT.SetRelayOutput(0, false);
                            //    DUT.SetRelayOutput(1, false);
                            //    DUT.SetRelayOutput(2, false);

                            //    if (DUT.ReadACParameter()) 
                            //    {
                            //        dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = DUT.L1UValue.ToString() + "(" + DUT.L1IValue.ToString() + ")|" + DUT.L2UValue.ToString() + "(" + DUT.L2IValue.ToString() + ")|" + DUT.L3UValue.ToString() + "(" + DUT.L3IValue.ToString() + ")";

                            //        if (DUT.L1UValue <= 40 && DUT.L2UValue <= 40 && DUT.L3UValue <= 40)
                            //        {
                            //            AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + "ON all relay. Read AC Parameter");

                            //            //Đóng tất cả rơ le đầu ra tải:
                            //            DUT.SetRelayOutput(0, true);
                            //            DUT.SetRelayOutput(1, true);
                            //            DUT.SetRelayOutput(2, true);
                            //            powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải
                            //            Conv.delay_ms(2000); //Chờ xác nhận dòng điện

                            //            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                            //            //testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);

                            //            if (DUT.ReadACParameter())
                            //            {
                            //                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = DUT.L1UValue.ToString() + "(" + DUT.L1IValue.ToString() + ")|" + DUT.L2UValue.ToString() + "(" + DUT.L2IValue.ToString() + ")|" + DUT.L3UValue.ToString() + "(" + DUT.L3IValue.ToString() + ")";
                            //                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + spM931.Ua.ToString() + "(" + spM931.Ia.ToString() + ")|" + spM931.Ub.ToString() + "(" + spM931.Ib.ToString() + ")|" + spM931.Uc.ToString() + "(" + spM931.Ic.ToString() + ")]";
                            //                testOK = (Math.Abs(DUT.L1UValue - spM931.Ua) < 5) && (Math.Abs(DUT.L2UValue - spM931.Ub) < 5) && (Math.Abs(DUT.L3UValue - spM931.Uc) < 5)
                            //                    && (Math.Abs(DUT.L1IValue - spM931.Ia) < 0.1) && (Math.Abs(DUT.L2IValue - spM931.Ib) < 0.1) && (Math.Abs(DUT.L3IValue - spM931.Ic) < 0.1);
                            //            }

                            //            powerBoard1.SetOutput(2, false); //Cắt tải đầu ra, chờ có dòng tải
                            //            DUT.SetRelayOutput(0, false);
                            //            DUT.SetRelayOutput(1, false);
                            //            DUT.SetRelayOutput(2, false);
                            //        }
                            //    }
                            //}

                            AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + "ON all relay. Read AC Parameter");

                            //Đóng tất cả rơ le đầu ra tải:
                            DUT.SetRelayOutput(0, true);
                            DUT.SetRelayOutput(1, true);
                            DUT.SetRelayOutput(2, true);
                            powerBoard1.SetOutput(2, true); //Đóng tải đầu ra, chờ có dòng tải
                            Conv.delay_ms(2000); //Chờ xác nhận dòng điện

                            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = spM932.Ia.ToString() + "|" + spM932.Ia.ToString() + "|" + spM932.Ic.ToString() + "[A]";
                            //testOK = (spM932.Ia > LOAD_LEVEL && spM932.Ib > LOAD_LEVEL && spM932.Ic > LOAD_LEVEL);

                            if (DUT.ReadACParameter())
                            {
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = DUT.L1UValue.ToString() + "(" + DUT.L1IValue.ToString() + ")|" + DUT.L2UValue.ToString() + "(" + DUT.L2IValue.ToString() + ")|" + DUT.L3UValue.ToString() + "(" + DUT.L3IValue.ToString() + ")";
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "[" + spM931.Ua.ToString() + "(" + spM931.Ia.ToString() + ")|" + spM931.Ub.ToString() + "(" + spM931.Ib.ToString() + ")|" + spM931.Uc.ToString() + "(" + spM931.Ic.ToString() + ")]";
                                testOK = (Math.Abs(DUT.L1UValue - spM931.Ua) < 5) && (Math.Abs(DUT.L2UValue - spM931.Ub) < 5) && (Math.Abs(DUT.L3UValue - spM931.Uc) < 5)
                                    && (Math.Abs(DUT.L1IValue - spM931.Ia) < 0.1) && (Math.Abs(DUT.L2IValue - spM931.Ib) < 0.1) && (Math.Abs(DUT.L3IValue - spM931.Ic) < 0.1);
                            }

                            powerBoard1.SetOutput(2, false); //Cắt tải đầu ra, 
                            DUT.SetRelayOutput(0, false);
                            DUT.SetRelayOutput(1, false);
                            DUT.SetRelayOutput(2, false);

                        }
                        else if (test_steep == (int)MAINAC_STEEPS.GPIO_OUTPUT) //
                        {
                            string[] check_value = { "1 0 0 0", "0 1 0 0", "0 0 1 0", "0 0 0 1" };
                            //Tắt tất cả rơ le
                            DUT.SetOutput(0);
                            relayBoards1.ReadInputImediate(); //Đọc ngay lập tức trạng thái đầu vào
                            if (!relayBoards1.GetInputState(1) && !relayBoards1.GetInputState(2) && !relayBoards1.GetInputState(3) && !relayBoards1.GetInputState(4))
                            {
                                testOK = true;
                                for (int i = 0; i < 4; i++)
                                {
                                    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = check_value[i];
                                    testOK = DUT.SetOutput(1 << i);
                                    #region old
                                    //if (relayBoards1.GetInputState(i + 1, true))
                                    //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                                    //else
                                    //{
                                    //    testOK = false;
                                    //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                    //}
                                    //dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                    //if (!testOK) break;
                                    #endregion
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = "";
                                    string read_value = "";
                                    for (int j = 0; j< 4; j++)
                                    {
                                        if (relayBoards1.GetInputState(j + 1, true))
                                            read_value += "1 ";
                                        else
                                        {

                                            read_value += "0 ";
                                        }
                                    }
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = read_value.Trim();
                                    testOK = (read_value.Trim() == check_value[i]);                                    
                                    if (!testOK) break;
                                }
                            }
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.GPIO_INPUT) //
                        {
                            int inpValue;
                            relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra
                            inpValue = DUT.ReadAllInput();
                            if (inpValue == 0)
                            {
                                testOK = true;

                                relayBoards1.SetOutput(8, true);
                                inpValue = DUT.ReadAllInput();
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value += "1 ";
                                if ((inpValue & 1) != 0)
                                {
                                    testOK = false;
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "0 ";
                                }
                                else
                                    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "1 ";
                            }
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.INPUT_CAPTURE) //
                        {
                            //powerBoard1.SetOutput(1, true, 1000); //nối OutPE -> In N   
                            testOK = OnOffRelayTest(test_steep);
                            //powerBoard1.SetOutput(1, false, 200); //ngat OutPE -> In N
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.RESIDUAL_CURRENT) //
                        {
                            testOK = ResidualTest(test_steep);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.RESIDUAL_PROTECT) //
                        {
                            testOK = ResidualProtectTest(test_steep);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.OVER_VOLTAGE) //
                        {
                            //powerBoard1.SetOutput(1, true, 1000); //nối OutPE -> In N
                            testOK = TestOverVoltage(test_steep);
                            //powerBoard1.SetOutput(1, false, 200); //ngat OutPE -> In N
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.UNDER_VOLTAGE) //
                        {
                            //powerBoard1.SetOutput(1, true, 1000); //nối OutPE -> In N
                            testOK = TestUnderVoltage(test_steep);
                            //powerBoard1.SetOutput(1, false, 200); //ngat OutPE -> In N
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.EARTH_CONNECT) //
                        {

                            int inpValue;
                            inpValue = DUT.ReadAllInput();
                            if (inpValue == 0)
                            {
                                testOK = true;
                                relayBoards1.SetOutput(5, true, 3000);
                                inpValue = DUT.ReadAllInput();
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                                testOK = ((inpValue & 8) == 0);
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = testOK ? "Pass" : "Fail";
                            }
                            relayBoards1.SetAllState(false); //Cắt tất cả rơ le đầu ra
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.DC12V_OUT) //
                        {
                            testOK = DC12VOutTest(test_steep);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.DC12V_PROTECT) //
                        {
                            testOK = DC12VProtectTest(test_steep);
                        }
                        else if (test_steep == (int)MAINAC_STEEPS.STOP_TEST) //
                        {
                            testOK = DUT.StopTest();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "[EnterTestSteep]");
                    }

                    //testOK = true;

                    //Kết luận kết quả:
                    dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = testOK ? Global.COLOR_OK : Global.COLOR_NG;
                    dgvTestList.Rows[test_steep].Cells["Result"].Value = testOK ? "OK" : "NG";
                    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - beginTestDT).TotalMilliseconds / 1000).ToString("0.0");

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

                btnStop.Text = "STOP";
                btnReload.Enabled = true;
                btnConfig.Enabled = cboOperator.Enabled = btnReady.Enabled = true;
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
