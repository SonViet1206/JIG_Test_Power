using DefaultNS;
using DefaultNS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Xml;
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
    }

    bool isTestOffline = false;
    string MachineName = "";
    private void frmMain_Shown(object sender, EventArgs e)
    {
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");

        isTestOffline = ini.IniReadValue("API_CONNECTION", "isTestOffLine", "1") == "1" ? true : false;
        //lbMeshStatus.BackColor = isTestOffline ? Color.DarkSlateGray : Color.Lime;
        MachineName = ini.IniReadValue("API_CONNECTION", "MachineName", "TEST01_PT_1");
        RestApiHelper.InitGlobalVarial();

        btnReload_Click(null, null);

        powerBoard1.Start(ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1"));
        //ReInitBoardType(); //COM2
        DUT.Start(ini.IniReadValue("COMMUNICATION", "DebugCommPort", "COM5"));

        panelLeft.Visible = false;
        timer1s.Enabled = true;
        SetControlStatus(STATUS.Ready);
        btnHome_Click(null, null);
        btnHome_Click(null, null);
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AppConfig.ini");

        powerBoard1.Stop();
        //relayBoards1.Stop();
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

    //void ReInitBoardType()
    //{
    //    IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");

    //    string rlboard = "1|DO1:EXT_MCU_RST|DO2:UART5_TXD_RX";
    //    string[] rls = rlboard.Split('|'); // Global.RELAY_BOARDS[BoardType].Split('|');

    //    relayBoards1.Stop();
    //    relayBoards1.Start(ini.IniReadValue("COMMUNICATION", "RelaysBoardCommPort", "COM2"), rls[0]);

    //    for (int i = 1; i < rls.Length; i++)
    //    {
    //        string[] r = rls[i].Split(':');
    //        if (r.Length > 1 && r[0][0] == 'D')
    //        {
    //            if (r[0][1] == 'O')
    //                relayBoards1.SetDOCaption(Conv.atoi32(r[0].Replace("DO", "")), r[1]);
    //            else if (r[0][1] == 'I')
    //                relayBoards1.SetDICaption(Conv.atoi32(r[0].Replace("DI", "")), r[1]);
    //        }
    //    }

    //}

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
            startTime = DateTime.Now;

            if (!isTestOffline)
            {
                AddLogWindow("Kiểm tra trạng thái của mã: " + txtSerialNo.Text + " trên MES");
                OutputGetSNStatus res = RestApiHelper.GetSNStatus(txtSerialNo.Text, MachineName);
                if (res != null)
                {
                    //MessageBox.Show(res.SN + "\r\n" + res.MachineName + "\r\n" + res.Confirm + "\r\n" + res.ErrorCode);
                    if (res.Confirm == "OK")
                    {
                        //MessageBox.Show("Mã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        EnterTestSteep(0);
                    }
                    else
                    {

                        MessageBox.Show("Kiểm tra trạng thái mã serial trên MES\r\nMã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Lỗi lấy trạng thái của mã Serial Number trên MES!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                EnterTestSteep(0);
            }
        }
        else
        {
            MessageBox.Show("Chưa nhập số chế tạo");
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
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
        //string password = Global.PasswordInput();
        //if (password.Length > 0)
        //{
        //    if (password == Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
        //    {
        //        frmConfigSelfTest frm = new frmConfigSelfTest();
        //        frm.ShowDialog();
        //        //ReInitBoardType();
        //        btnReload_Click(null, null);
        //    }
        //    else MessageBox.Show("Sai mật khẩu.", "LỖI THAO TÁC", MessageBoxButtons.OK);
        //}

        frmConfigSelfTest frm = new frmConfigSelfTest();
        frm.ShowDialog();
        //ReInitBoardType();
        btnReload_Click(null, null);
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

    }

    private void SaveCSVLog()
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

                    OutputInsertPassFailDetailTestCase res = RestApiHelper.InsertPassFailDetailTestCase(txtSerialNo.Text, MachineName, cboOperator.Text, dtab);
                    if (res != null)
                    {
                        if (res.Status == "OK")
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
                        }
                        else
                        {
                            AddLogWindow("Ghi MES không thành công\r\nStatus: " + res.Status + "\r\nError Code: " + res.ErrorCode);
                            MessageBox.Show("Ghi MES không thành công\r\nStatus: " + res.Status + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private void SaveXMLLogFile()
    {
        string sBuff = "";
        string path_name = DataLogPath + @"\";
        if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
        path_name += @"\FUN_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".xml";
        //Lan dau tao file thi tao luon header line:
        if (File.Exists(path_name))
        {
            try
            {
                File.Delete(path_name);
            }
            catch { }
        }

        XmlTextWriter writer = new XmlTextWriter(path_name, System.Text.Encoding.UTF8);

        //writer.WriteComment("sample XML fragment"); //<!--sample XML fragment-->
        //writer.WriteStartDocument(true);
        writer.Formatting = System.Xml.Formatting.Indented;
        writer.Indentation = 2;
        try
        {
            //<SHOP_FLOR_DATA 
            //xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
            //xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
            //LINE_DESCR="G3" 
            //MODCOD="" 
            //FINALMATERIAL="CHG00005674" 
            //ORDERTYPE="EV" 
            //PRODUCTIONORDER="" 
            //SAPSEQ="0" 
            //SAPCODE="" 
            //SERIALNUMBER="CHG00005674-001632" 
            //MSG_DT="2021-04-28T09:43:15.0000000" 
            //STATUS="TP_60"

            writer.WriteStartElement("SHOP_FLOR_DATA");
            
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
            writer.WriteAttributeString("LINE_DESCR", "SelfTest");
            writer.WriteAttributeString("MODCOD", "");
            writer.WriteAttributeString("FINALMATERIAL", "");
            writer.WriteAttributeString("ORDERTYPE", "EV");
            writer.WriteAttributeString("PRODUCTIONORDER", "");
            writer.WriteAttributeString("SAPSEQ", "0");
            writer.WriteAttributeString("SAPCODE", "");
            writer.WriteAttributeString("SERIALNUMBER", txtSerialNo.Text);
            writer.WriteAttributeString("MSG_DT", Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-ddTHH:mm:ss.0000000"));
            writer.WriteAttributeString("STATUS", "");



            sBuff = lblCaption.Text + "\r\n";
            sBuff += "Operator," + Conv.atos(cboOperator.Text) + "\r\n";
            sBuff += "Equipment," + "" + "\r\n";
            sBuff += "Location,VinSmart\r\n";
            sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
            sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
            sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
            sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";




            writer.WriteStartElement("PROCESSING_CHECK_SECTION");

            sBuff = "";
            for (int i = 2; i < dgvTestList.Rows.Count; i++)
            {
                string test_name = Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value).Trim();
                string description = Conv.atos(dgvTestList.Rows[i].Cells["Description"].Value).Trim();
                string check_value = Conv.atos(dgvTestList.Rows[i].Cells["CheckValue"].Value).Trim();
                string read_value = Conv.atos(dgvTestList.Rows[i].Cells["ReadValue"].Value).Trim();
                string result = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value).Trim();

                //writer.WriteAttributeString("KEY", key);
                //if (val != "") writer.WriteAttributeString("VALUE", val);
                //TEST NAME,DESCRIPTION,CHECK VALUE,READ VALUE,START TEST,ELAPSE(sec),RESULT
                if (result != "")
                {
                    writer.WriteStartElement("OP_VAL");

                    writer.WriteAttributeString("RESULT", (result == "OK" ? "OK" : "NOK"));
                    writer.WriteAttributeString("TEST_NAME", test_name);
                    writer.WriteAttributeString("DESCRIPTION", description);
                    writer.WriteAttributeString("CHECK_VALUE", check_value);
                    writer.WriteAttributeString("READ_VALUE", read_value);

                    writer.WriteEndElement();
                }
                else
                {
                    if (sBuff != "") writer.WriteEndElement();
                    sBuff = test_name;
                    //writer.WriteStartElement(sBuff);
                    //<CHECKED_DATA OP_RESULT="OK" OPCODE="019446" OPERATOR="" OPTYPE="BUYOFF" OPSTARTDATE="2021-04-28T09:40:59" OPDATE="2021-04-28T09:43:15" WORKPLACE="">
                    writer.WriteStartElement("CHECKED_DATA");
                    writer.WriteAttributeString("OP_RESULT", "");
                    writer.WriteAttributeString("OPCODE", "Sample OP for " + sBuff + " check");
                    writer.WriteAttributeString("OPERATOR", cboOperator.Text);
                    writer.WriteAttributeString("OPTYPE", "BUYOFF");
                    writer.WriteAttributeString("OPSTARTDATE", Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-ddTHH:mm:ss"));
                    writer.WriteAttributeString("OPDATE", Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-ddTHH:mm:ss"));
                    writer.WriteAttributeString("WORKPLACE", "");

                }
            }
            if (sBuff != "") writer.WriteEndElement();
            
            //writer.WriteEndElement(); //CHECKED_DATA
            writer.WriteEndElement(); //PROCESSING_CHECK_SECTION
            writer.WriteEndElement(); //SHOP_FLOR_DATA

            // Write the XML to file and close the writer.
            writer.Flush();
            //MessageBox.Show("XML File created ! ");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    private void btnSave_Click1(object sender, EventArgs e)
    {
        SaveCSVLog();
        SaveXMLLogFile();

    }


    enum PLC_STEEPS
    {
        START_SELF_TEST = (int)0,
        EXECUTE_TEST
    }
    string[] PLC_STEEP_DESCRIPTION = {
        "Vào chế độ tự kiểm tra",
        "Thực hiện quy trình tự kiểm tra"
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

        string[] steepNames = Enum.GetNames(typeof (PLC_STEEPS));
        
        dgvTestList.Rows.Clear();

        for (int i = 0; i < steepNames.Length; i++)
        {
            dgvTestList.Rows.Add();
            dgvTestList.Rows[i].Cells["TestOrder"].Value = i + 1;
            dgvTestList.Rows[i].Cells["TestName"].Value = steepNames[i];
            dgvTestList.Rows[i].Cells["Description"].Value = PLC_STEEP_DESCRIPTION[i];
        }
        
        //txtTotalSteep.Text = dgvTestList.Rows.Count.ToString();
    }

    bool IsJObject(string svalue, ref JObject json_out)
    {
        json_out = null;
        try
        {
            json_out = JObject.Parse(svalue); //Dữ liệu trả lại có cấu trúc json
            return true;
        }
        catch {}
        return false;
    }

    bool IsJArray(string svalue, ref JArray json_out)
    {
        json_out = null;
        try
        {
            json_out = JArray.Parse(svalue); //Dữ liệu trả lại có cấu trúc json
            return true;
        }
        catch { }
        return false;
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
                    //StartAgainIfStartIMXFail:
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
                        //txtTestName.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestName"].Value);

                        ledResult.Text = "---";
                        ledResult.BackColor = Global.COLOR_OFF;

                        //btnTest.Enabled = false;

                        //AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text);

                        Application.DoEvents();
                        //Xử lý LOGIC theo từng bước:
                        
                        if (test_steep == (int)PLC_STEEPS.START_SELF_TEST) //Start Test
                        {
                            int s_count = 0;
                            powerBoard1.SetOutput(4, true); //Bật nguồn

                            frmShowChoice frm = new frmShowChoice();
                            frm.lblTestSteep.Text = txtTestSteep.Text;
                            frm.lblTestName.Text = "Chuẩn bị vào SELF TEST";
                            frm.lblMessage.Text = "Kiểm tra lại thông tin,\r\n";
                            frm.lblMessage.Text += "Kiểm tra và đặt lại địa chỉ của bộ iMeter\r\n";
                            frm.lblMessage.Text += "Kiểm tra cắm thẻ nhớ\r\n";
                            frm.lblMessage.Text += "Quan sát trên màn hình LCD của thiết bị, chờ khởi động hoàn tất\r\n";
                            frm.btnClose.Visible = frm.btnClose.Enabled = true;
                            frm.btnSetOK.Visible = frm.btnSetNG.Visible = false;

                            //frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray; 
                            frm.ShowDialog();

                            //Chờ thiết bị khởi động:
                            while (thread_started && ++s_count < 2)
                            {
                                Conv.delay_ms(500);
                                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");
                            }
                            
                            testOK = DUT.TCP_StartTest();

                            //if (!testOK) goto StartAgainIfStartIMXFail;
                            while (!testOK && thread_started && ++s_count < 2)
                            {
                                Conv.delay_ms(500);
                                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");
                                testOK = DUT.TCP_StartTest();
                            }
                        }
                        else if (test_steep == (int)PLC_STEEPS.EXECUTE_TEST) //Check Serial
                        {
                            //Xóa các dòng kết quả:
                            while (dgvTestList.Rows.Count > 2) dgvTestList.Rows.RemoveAt(dgvTestList.Rows.Count - 1);

                            int s_count = 0;

                            while (++s_count < 3)
                            {
                                Conv.delay_ms(1000);
                                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");
                            }
                            
                            string dutValue = DUT.TCP_GetDiag();
                            //dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = txtSerialNo.Text;
                            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            //testOK = (txtSerialNo.Text == dutValue);

                            //DEBUG:
                            //dutValue = File.ReadAllText(@"D:\self_test.log");
                            //dutValue = File.ReadAllText(@"D:\FILE_20210601_144310_diag123_imxNG.log");

                            ////Xử lý kết quả:
                            int steep_count = 2;
                            JObject root_json = null;
                            
                            if (IsJObject(dutValue, ref root_json))
                            {
                                int BoardType = (txtSerialNo.Text.IndexOf(Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='BoardType'"))) > -1 ? 1 : 0);
                                arUtil.INIHelper cfg = new arUtil.INIHelper(Application.StartupPath + "\\VersionCfg.ini");
                                arUtil.INIHelper des = new arUtil.INIHelper(Application.StartupPath + "\\Descriptions.ini");

                                string cfg_str = "";
                                
                                foreach (var root_object in root_json)
                                {
                                    JObject child_json = null;
                                    string root_key = root_object.Key.ToUpper();
                                    string root_value = root_object.Value.ToString();
                                    JArray root_array = null;

                                    dgvTestList.Rows.Add();
                                    dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["TestName"].Value = root_key;

                                    //20210601: nếu đối tượng là mảng chỉ lấy phần tử đầu tiên:
                                    if (IsJArray(root_value, ref root_array)) root_value = root_array[0].ToString();
                                    
                                    if (IsJObject(root_value, ref child_json))
                                    {
                                        foreach (var child_object in child_json)
                                        {
                                            JArray child_array = null;
                                            string child_key = child_object.Key;
                                            string child_value = child_object.Value.ToString();
                                            child_value = child_value.Replace("\r", "");
                                            child_value = child_value.Replace("\n", "");
                                            dgvTestList.Rows.Add();
                                            dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["TestOrder"].Value = (++steep_count).ToString();
                                            dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["TestName"].Value = "      " + child_key;
                                            dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["Description"].Value = des.get_Data(root_key, child_key, "");
                                            if (IsJArray(child_value, ref child_array))
                                            {
                                                string sBuff = "";
                                                bool isOk = true;
                                                int fan_count = 0;
                                                foreach (var item in child_array.Children())
                                                {
                                                    if (((child_key != "fanStatus" && child_key != "meterConnectStatus") || BoardType != 0) 
                                                        || (child_key == "fanStatus" && BoardType == 0 && fan_count < 4)
                                                        || (child_key == "meterConnectStatus" && BoardType == 0 && fan_count < 1)
                                                        ) 
                                                    {
                                                        sBuff += item.ToString() + " ";
                                                        if (item.ToString() != "OK") isOk = false;
                                                    }
                                                    ++fan_count;
                                                }
                                                dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["ReadValue"].Value = sBuff.Trim();
                                                dgvTestList.Rows[dgvTestList.Rows.Count - 1].DefaultCellStyle.BackColor = isOk ? Global.COLOR_OK : Global.COLOR_NG;
                                                dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["Result"].Value = isOk ? "OK" : "NG";
                                            }   
                                            else
                                            {
                                                bool steepOK = false;
                                                dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["ReadValue"].Value = child_value;
                                                cfg_str = cfg.get_Data(root_key, child_key, "NULL");
                                                if (cfg_str != "NULL") //Có dữ liệu cấu hình
                                                {
                                                    if (cfg_str == "" || cfg_str.ToUpper() == "SKIP")
                                                    {
                                                        //File config có trường này nhưng thông tin rỗng  thì bỏ qua không check
                                                        dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["CheckValue"].Value = "SKIP";
                                                        steepOK = true;
                                                    }
                                                    else
                                                    {
                                                        dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["CheckValue"].Value = cfg_str;
                                                        steepOK = (cfg_str == child_value);
                                                    }
                                                }    
                                                else
                                                {
                                                    steepOK = (child_value == "OK");
                                                }
                                                ////Kết luận:
                                                dgvTestList.Rows[dgvTestList.Rows.Count - 1].DefaultCellStyle.BackColor = steepOK ? Global.COLOR_OK : Global.COLOR_NG;
                                                dgvTestList.Rows[dgvTestList.Rows.Count - 1].Cells["Result"].Value = steepOK ? "OK" : "NG";
                                            }    
                                            
                                        }
                                    }
                                }
                                //Có ít nhất 1 dòng, thêm bước ghép trạm sạc:
                                if (steep_count > 2)
                                {
                                    //Hoàn thành bước kiểm này:
                                    dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = Global.COLOR_OK; // : Global.COLOR_NG;
                                    dgvTestList.Rows[test_steep].Cells["Result"].Value = "OK"; // : "NG";
                                    
                                    dgvTestList.Rows.Add();
                                    test_steep = dgvTestList.Rows.Count - 1;
                                    dgvTestList.Rows[test_steep].Cells["TestOrder"].Value = (++steep_count).ToString();
                                    dgvTestList.Rows[test_steep].Cells["TestName"].Value = "UPDATE_MES";
                                    dgvTestList.Rows[test_steep].Cells["Description"].Value = "Kiểm tra thông tin ghép QR code";
                                    //Mở cửa sổ chờ ghép trạm thành công:
                                    frmMesSelfTest frm = new frmMesSelfTest();
                                    frm.txtSerialNo.Text = txtSerialNo.Text;
                                    frm.ShowForm(Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='MesCOMMPort'")));
                                    testOK = frm.state;
                                }
                            }
                                
                        }
                        
                        //testOK = DUT.TCP_StopTest();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "[EnterTestSteep]");
                    }
                    Conv.delay_ms(1000);
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
                            ////Tu dong luu khi buoc kiem OK
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
        if (thread_ended && btnReady.Enabled == false && e.RowIndex < 2) 
            EnterTestSteep(e.RowIndex);
    }

    private void btnHome_Click(object sender, EventArgs e)
    {
        panelLeft.Visible = !panelLeft.Visible;
        //panelHeader.Height = panelLeft.Visible ? 384 : 240;

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

    private void tcpiP_Object1_Load(object sender, EventArgs e)
    {

    }

    private void DUT_Load_1(object sender, EventArgs e)
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

    private void button1_Click(object sender, EventArgs e)
    {
        XmlTextReader textReader = new XmlTextReader("D:\\VF_Sample.xml");
        textReader.Read();
        textBox1.Text = "";
        // If the node has value  
        while (textReader.Read())
        {
            // Move to fist element  
            textReader.MoveToElement();
            try
            {
                textBox1.Text += "\r\n===================\r\n";
                // Read this element's properties and display them on console  
                textBox1.Text += "\r\nName:" + textReader.Name;
                textBox1.Text += "\r\nBase URI:" + textReader.BaseURI;
                textBox1.Text += "\r\nLocal Name:" + textReader.LocalName;
                textBox1.Text += "\r\nAttribute Count:" + textReader.AttributeCount.ToString();
                textBox1.Text += "\r\nDepth:" + textReader.Depth.ToString();
                textBox1.Text += "\r\nLine Number:" + textReader.LineNumber.ToString();
                textBox1.Text += "\r\nNode Type:" + textReader.NodeType.ToString();
                textBox1.Text += "\r\nAttribute Count:" + textReader.Value.ToString();
            }
            catch (Exception)
            {

                
            }
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        XmlTextWriter writer = new XmlTextWriter("D:\\product.xml", System.Text.Encoding.UTF8);

        //writer.WriteComment("sample XML fragment"); //<!--sample XML fragment-->
        //writer.WriteStartDocument(true);
        writer.Formatting = System.Xml.Formatting.Indented;
        writer.Indentation = 2;
        try
        {
            //<SHOP_FLOR_DATA xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" LINE_DESCR="G3" MODCOD="" FINALMATERIAL="CHG00005674" ORDERTYPE="EV" PRODUCTIONORDER="" SAPSEQ="0" SAPCODE="" SERIALNUMBER="CHG00005674-001632" MSG_DT="2021-04-28T09:43:15.0000000" STATUS="TP_60">
            writer.WriteStartElement("SHOP_FLOR_DATA");
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
            writer.WriteAttributeString("LINE_DESCR", "G3");
            writer.WriteAttributeString("MODCOD", "");
            writer.WriteAttributeString("FINALMATERIAL", "CHG00005674");

            
            writer.WriteStartElement("PROCESSING_CHECK_SECTION");

            //<CHECKED_DATA OP_RESULT="OK" OPCODE="019446" OPERATOR="" OPTYPE="BUYOFF" OPSTARTDATE="2021-04-28T09:40:59" OPDATE="2021-04-28T09:43:15" WORKPLACE="">
            writer.WriteStartElement("CHECKED_DATA");
            writer.WriteAttributeString("OP_RESULT", "OK");
            writer.WriteAttributeString("OPCODE", "019446");
            writer.WriteAttributeString("OPERATOR", "");
            writer.WriteAttributeString("OPTYPE", "BUYOFF");
            writer.WriteAttributeString("OPSTARTDATE", "2021-04-28T09:40:59");
            writer.WriteAttributeString("OPDATE", "2021-04-28T09:43:15");
            writer.WriteAttributeString("WORKPLACE", "");

            //<OP_VAL KEY="CP State A2 was detected." VALUE="OK" />
            //<OP_VAL KEY="CP State B was detected." VALUE="OK" />
            
            writer.WriteStartElement("OP_VAL");
            writer.WriteAttributeString("KEY", "CP State A2 was detected.");
            writer.WriteAttributeString("VALUE", "OK");
            writer.WriteEndElement();

            writer.WriteStartElement("OP_VAL");
            writer.WriteAttributeString("KEY", "CP State B was detected.");
            writer.WriteAttributeString("VALUE", "OK");
            writer.WriteEndElement();

            
            writer.WriteEndElement(); //CHECKED_DATA
            writer.WriteEndElement(); //PROCESSING_CHECK_SECTION
            writer.WriteEndElement(); //SHOP_FLOR_DATA

            // Write the XML to file and close the writer.
            writer.Flush();
            MessageBox.Show("XML File created ! ");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    private void createNode(string pID, string pName, string pPrice, XmlTextWriter writer)
    {
        writer.WriteStartElement("PROCESSING_CHECK_SECTION");
        writer.WriteStartElement("Product_id");
        writer.WriteString(pID);
        writer.WriteEndElement();
        writer.WriteStartElement("Product_name");
        writer.WriteString(pName);
        writer.WriteEndElement();
        writer.WriteStartElement("Product_price");
        writer.WriteString(pPrice);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private void button3_Click(object sender, EventArgs e)
    {
        XmlTextReader textReader = new XmlTextReader("D:\\product.xml");
        textReader.Read();
        textBox1.Text = "";
        // If the node has value  
        while (textReader.Read())
        {
            // Move to fist element  
            textReader.MoveToElement();
            try
            {
                textBox1.Text += "\r\n===================\r\n";
                // Read this element's properties and display them on console  
                textBox1.Text += "\r\nName:" + textReader.Name;
                textBox1.Text += "\r\nBase URI:" + textReader.BaseURI;
                textBox1.Text += "\r\nLocal Name:" + textReader.LocalName;
                textBox1.Text += "\r\nAttribute Count:" + textReader.AttributeCount.ToString();
                textBox1.Text += "\r\nDepth:" + textReader.Depth.ToString();
                textBox1.Text += "\r\nLine Number:" + textReader.LineNumber.ToString();
                textBox1.Text += "\r\nNode Type:" + textReader.NodeType.ToString();
                textBox1.Text += "\r\nAttribute Count:" + textReader.Value.ToString();
            }
            catch (Exception)
            {


            }
        }
    }
}
