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

    private int CountTimeBetweenTestXmcAndIMX = 90000;

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
    double OfsetCalib = 0.2;
    private void frmMain_Shown(object sender, EventArgs e)
    {
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");

        isTestOffline = ini.IniReadValue("API_CONNECTION", "isTestOffLine", "1") == "1" ? true : false;
        lbMeshStatus.BackColor = isTestOffline ? Color.DarkSlateGray : Color.Lime;
        MachineName = ini.IniReadValue("API_CONNECTION", "MachineName", "TEST01_PT_1");

        OfsetCalib = Conv.atod(ini.IniReadValue("COMMON", "OfsetCalib", "0.2"));
        //MessageBox.Show(OfsetCalib.ToString());

        RestApiHelper.InitGlobalVarial();

        btnReload_Click(null, null);

        powerBoard1.Start(ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1"));
        measuringBoard1.Start(ini.IniReadValue("COMMUNICATION", "MeasuringBoardCommPort", "COM4"));
        ReInitBoardType(); //COM2
        evalator1.Start(ini.IniReadValue("COMMUNICATION", "EvalatorBoardCommPort", "COM3"));
        DUT.Start(ini.IniReadValue("COMMUNICATION", "DebugCommPort", "COM5"));

        CountTimeBetweenTestXmcAndIMX = Conv.atoi32(ini.IniReadValue("COMMUNICATION", "WaitTime", "90000"));

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
        measuringBoard1.Stop();
        evalator1.Stop();
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

        string rlboard = "1|DO1:TEMP1_IN - TEMP_IN_COM|DO2:TEMP2_IN - TEMP_IN_COM|DO3:RES 1000|DO4:RES 1155|DO5:RES 1232|DO6:RES 1308|DO7:PP_ADC_INPUT - GND|DO8:PP_ADC_INPUT - 5V";
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

    private void btnConfig_Click(object sender, EventArgs e)
    {
        string password = Global.PasswordInput();
        if (password.Length > 0)
        {
            if (password == Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
            {
                frmConfigPLC frm = new frmConfigPLC();
                frm.ShowDialog();
                //ReInitBoardType();
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
        if(ledResult.Text == "OK")
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
        else if(ledResult.Text == "NG")
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

                    TotalCount++;
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
                            TotalCount++;
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
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
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

                    TotalCount++;
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
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + TotalCount + "|" + PassCount + "|" + DateTime.Now.Day + "' WHERE ItemName='Password3'");
        }
    }

    enum PLC_STEEPS
    {
        EXT_MCU_RST = (int)0,
        START_TEST_XMC,
        IMX_UART_TEST,
        CONTROL_PILOT,
        CONTROL_PILOT_ON,
        CONTROL_PILOT_OFF,
        CALIBRATION_ADC,
        CALIBRATION_ADC_E,
        CALIBRATION_ADC_A,
        TRIGGER_INTERRUP,
        PILOT_PWM_INTERRUPT,
        PROXIMITY_TEST,
        XMC_FW_VERSION,
        STOP_TEST_XMC,

        //CHECK_PRECONDITION,
        START_TEST_IMX,
        SERIAL_NUMBER,
        SOFTWARE_VERSION,
        HARDWARE_VERSION,
        DDR_TEST,
        EMMC_TEST,
        //GPIO_OUTPUT_TEST,
        //GPIO_INPUT_TEST,
        UART4_TEST,
        UART5_TEST,
        CAN_TEST,
        I2C_TEST,
        ETHERNET_TEST,
        //TEMP1_TEST,
        //TEMP2_TEST,
        PLC_IC_INFO,
        HLC_SLAC_TEST,
        STOP_TEST
    }
    string[] PLC_STEEP_DESCRIPTION = {
        "Kiểm tra tín hiệu 'RESET' mạch PLC",
        "Vào chế độ kiểm tra chip XMC",
        "Kiểm tra truyền thông với IMX",
        "Vào bước kiểm tra phát xung PWM",
        "Kiểm tra phát xung PWM điều khiển PILOT",
        "Kiểm tra tắt xung PWM điều khiển PILOT",
        "Vào bước kiểm tra giá trị đo ADC",
        "Kiểm tra giá trị đo ADC mode E",
        "Kiểm tra giá trị đo ADC mode A",
        "Kiểm tra chuyển trạng thái tín hiệu Trigger",
        "Kiểm tra chuyển trạng thái tín hiệu Pilot",
        "Kiểm tra Proximity Sensor",
        "Kiểm tra phiên bản phần mềm XMC",
        "Kết thúc kiểm tra XMC",

        //"Kiểm tra điều kiện thực hiện test IMX",
        "Vào chế độ kiểm tra IMX",
        "Kiểm tra số chế tạo với tem QR",
        "Phiên bản phần mềm được nạp",
        "Mã phiên bản phần cứng",
        "Kiểm tra RAM",
        "Kiểm tra thẻ nhớ ngoài",
        //"Kiểm tra tín hiệu đầu ra",
        //"Kiểm tra tín hiệu đầu vào",
        "Kiểm tra cổng kết nối UART4",
        "Kiểm tra cổng kết nối UART5",
        "Kiểm tra các cổng kết nối CAN",
        "Kiểm tra kết nối I2C",
        "Kiểm tra các cổng kết nối ETHERNET",
        //"Kiểm tra Temperature Sensor 1",
        //"Kiểm tra Temperature Sensor 2",
        "Kiểm tra cổng thông tin PLC IC",
        "Kiểm tra truyền thông HLC SLAC",
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

        string[] steepNames = Enum.GetNames(typeof (PLC_STEEPS));
        
        dgvTestList.Rows.Clear();

        for (int i = 0; i < steepNames.Length; i++)
        {
            dgvTestList.Rows.Add();
            dgvTestList.Rows[i].Cells["TestOrder"].Value = i + 1;
            dgvTestList.Rows[i].Cells["TestName"].Value = steepNames[i];
            dgvTestList.Rows[i].Cells["Description"].Value = PLC_STEEP_DESCRIPTION[i];
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
            int retryStartIMX = 5;
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

                        //btnTest.Enabled = false;

                        AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text);

                        Application.DoEvents();
                        //Xử lý LOGIC theo từng bước:
                        
                        if (test_steep == (int)PLC_STEEPS.EXT_MCU_RST) //0
                        {
                            int s_count = 0;
                            powerBoard1.SetOutput(4, false, 1000); //Tắt nguồn
                            frmShowChoice frm = new frmShowChoice();
                            frm.lblTestSteep.Text = "1";
                            frm.lblTestName.Text = "BƯỚC CHUẨN BỊ KIỂM TRA TÍN HIỆU RESET PLC";
                            frm.lblMessage.Text = "Kiểm tra các kết nối với JIG, Chú ý các điều kiện an toàn.\r\n";
                            frm.lblMessage.Text += "\r\nBấm chuột vào 'OK' hoặc gõ phím 'Enter'";
                            frm.btnClose.Enabled = true;
                            frm.btnSetOK.BackColor = frm.btnSetNG.BackColor = Color.DimGray;
                            frm.ShowDialog();
                            DUT.ClearLog();
                            powerBoard1.SetOutput(4, true, 1000); //Bật nguồn
                            Conv.delay_ms(1000);
                            //if (DUT.BootingOK())
                            //{
                            //    powerBoard1.SetOutput(2, true, 1000); //Chập chân reset
                            //    DUT.ClearLog();
                            //    powerBoard1.SetOutput(2, false, 1000); //Nhả chân reset
                            //    testOK = DUT.BootingOK();
                            //}
                            ////Vì bỏ cổng Debug nên thay thế điều kiện kiểm tra khởi động bằng đoạn này:
                            if (DUT.CAN_StartTest()) //Dùng lệnh vào test mode để khẳng định khởi động OK
                            {
                                powerBoard1.SetOutput(2, true, 1000); //Chập chân reset
                                testOK = !DUT.CAN_StopTest(); //Dùng lệnh thoát test mode để kiểm tra mạch bị giữ reset
                                powerBoard1.SetOutput(2, false, 1000); //Nhả chân reset
                            }

                            //Chờ thời gian cho thiết bị sẵn sàng:
                            while (++s_count < 20)
                            {
                                dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");
                                Conv.delay_ms(1000);
                            }
                        }
                        else if (test_steep == (int)PLC_STEEPS.START_TEST_XMC) //0
                        {
                            testOK = DUT.CAN_StartTest();
                        }
                        else if (test_steep == (int)PLC_STEEPS.IMX_UART_TEST) //=1
                        {
                            //testOK = DUT.XMC_UartTestIMX();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Skip";
                            testOK = true;
                        }
                        else if (test_steep == (int)PLC_STEEPS.CONTROL_PILOT) //=2
                        {
                            powerBoard1.SetOutput(3, false); //Cắt DUT với Evalator
                            testOK = DUT.XMC_ControlPilotEnter();
                        }
                        else if (test_steep == (int)PLC_STEEPS.CONTROL_PILOT_ON) //=2
                        {
                            powerBoard1.SetOutput(3, false); //Cắt DUT với Evalator

                            DUT.XMC_ControlPilotOn();
                            Conv.delay_ms(3000);
                            //if (DUT.XMC_ControlPilotOn())
                            {
                                double f = measuringBoard1.Frequency;
                                double v = measuringBoard1.PeakVotage;
                                double d = measuringBoard1.Duty;
                                double v_min = measuringBoard1.PeakVotageMin;
                                //Đọc giá trị PMW: nếu sai số duty nhỏ 0.5% và tần số là 1% thì bài test PASS
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Vp=12V±1|F=1000Hz±20|Duty=5%±0.5";
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = v.ToString("0.0") + "|" + v_min.ToString("0.0") + "|" + f.ToString("0.0") + "|" + d.ToString("0.0");
                                
                                testOK = (f > 980 && f < 1020 && v > 11 && v < 13 && d > 4.5 && d < 5.5 && v_min > -13 && v_min < -11);
                            
                            }
                        }
                        else if (test_steep == (int)PLC_STEEPS.CONTROL_PILOT_OFF) //=2
                        {
                            powerBoard1.SetOutput(3, false); //Cắt DUT với Evalator
                            
                            DUT.XMC_ControlPilotOff();
                            Conv.delay_ms(2000);
                            //if (DUT.XMC_ControlPilotOff())
                            {
                                double f = measuringBoard1.Frequency;
                                double v = measuringBoard1.PeakVotage;
                                double d = measuringBoard1.Duty;

                                //Đọc giá trị PMW: nếu sai số duty nhỏ 0.5% và tần số là 1% thì bài test PASS
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Vp>10V|F=0Hz|Duty=100%";
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = v.ToString("0.0") + "|" + f.ToString("0.0") + "|" + d.ToString("0.0");
                                //nếu phát hiện trạng thái control pilot ở mức thấp thì bài test FAIL
                                testOK = (f == 0 && v > 10 && d == 100);
                            }
                        }
                        else if (test_steep == (int)PLC_STEEPS.CALIBRATION_ADC) //
                        {
                            //powerBoard1.SetOutput(3, true); //Nối DUT với Evalator
                            DUT.XMC_CalibrationADCEnter();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Skip";
                            testOK = true;
                        }
                        else if (test_steep == (int)PLC_STEEPS.CALIBRATION_ADC_E) //
                        {
                            int retValue;
                            
                            evalator1.SetMode("E", 1000);
                            Conv.delay_ms(5000);
                            Global.WriteLogFile("[PLC STEEP CALIBRATION_ADC_E] - " + ((int)((measuringBoard1.PeakVotage + OfsetCalib) * 100)).ToString());
                            retValue = DUT.XMC_CalibrationADCStateE((int)((measuringBoard1.PeakVotage + OfsetCalib) * 100));
                            testOK = (retValue > 0xFFFF);
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = "E:" + (retValue & 0xFFFF).ToString() + "|" + (retValue >> 16).ToString();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                        }
                        else if (test_steep == (int)PLC_STEEPS.CALIBRATION_ADC_A) //
                        {
                            long retValue;
                            string modeF;
                            int temp;
                            evalator1.SetMode("A", 1000);
                            Conv.delay_ms(9000);
                            Global.WriteLogFile("[PLC STEEP CALIBRATION_ADC_A] - " + ((int)((measuringBoard1.PeakVotage + OfsetCalib) * 100)).ToString());
                            retValue = DUT.XMC_CalibrationADCStateA((int)((measuringBoard1.PeakVotage + OfsetCalib) * 100));
                            temp = (int)(retValue & 0xFFFF);
                            //testOK = (((retValue >> 24) & 0xFF) > 0) && (((retValue >> 56) & 0xFF) > 0);
                            testOK = (temp > 240 && temp < 320) && (((retValue >> 56) & 0xFF) > 0);
                            //testOK = (temp > 235 && temp < 355) && (((retValue >> 56) & 0xFF) > 0);
                            modeF = " F:" + (retValue & 0xFFFF).ToString() + "|" + ((retValue>>16) & 0xFF).ToString() + "|" + ((retValue >> 24) & 0xFF).ToString();
                            retValue >>= 32;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = "A:" + (retValue & 0xFFFF).ToString() + "|" + ((retValue >> 16) & 0xFF).ToString() + "|" + ((retValue >> 24) & 0xFF).ToString() + modeF;
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                        }
                        else if (test_steep == (int)PLC_STEEPS.TRIGGER_INTERRUP) //
                        {
                            long retValue = 0;

                            powerBoard1.SetOutput(3, true); //Nối DUT với Evalator
                            DUT.XMC_TriggerInterruptEnter(); //Vào bài kiểm Trigger
                            evalator1.SetMode("A", 150); //Set EV mode A
                            //evalator1.SetMode("D", 150); //Set EV mode A
                            //Conv.delay_ms(2000);
                            DUT.XMC_TriggerInterruptA2B_On(300); //Báo cho DUT bắt đầu
                            for (int i = 0; i < 10; i++)
                            {
                                evalator1.SetMode("B", 150); //Set EV mode B
                                evalator1.SetMode("A", 150); //Set EV mode A
                            }
                            Conv.delay_ms(1000);
                            DUT.XMC_TriggerInterruptA2B_Off(1000); //Báo cho DUT kết thúc
                            Conv.delay_ms(2000);
                            //powerBoard1.SetOutput(3, true);
                            evalator1.SetMode("D", 300); //Set EV mode D
                            DUT.XMC_TriggerInterruptD2E_On(300); //Báo cho DUT bắt đầu
                            for (int i = 0; i < 10; i++)
                            {
                               evalator1.SetMode("E", 150); //Set EV mode E
                               evalator1.SetMode("D", 150); //Set EV mode D
                            }
                            Conv.delay_ms(1000);
                            retValue = DUT.XMC_TriggerInterruptD2E_Off(1000); //Báo cho DUT kết thúc chờ nhận kết quả

                            int count1 = (int)((retValue >> 24) & 0xFF);
                            int count2 = (int)((retValue >> 16) & 0xFF);
                            int count3 = (int)((retValue >> 8) & 0xFF);
                            int count4 = (int)(retValue & 0xFF);

                            testOK = (((retValue>>32) & 0xFF) == 0x0F);
                            //testOK = (Math.Round((double)count1 - 100) <= 1 && Math.Round((double)count2 - 100) <= 1 && Math.Round((double)count3 - 100) <= 1 && Math.Round((double)count4 - 100) <=1);
                            //testOK = (int)((retValue >> 32) & 0xFF) == 15;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = ((retValue >> 24) & 0xFF).ToString() + "|" + ((retValue >> 16) & 0xFF).ToString() + "|" + ((retValue >> 8) & 0xFF).ToString() + "|" + (retValue & 0xFF).ToString() + "|" + ((retValue >> 32) & 0xFF).ToString();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                        }
                        else if (test_steep == (int)PLC_STEEPS.PILOT_PWM_INTERRUPT) //
                        {

                            bool retValue = DUT.XMC_PilotPWMInterrupt();
                            //dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue.ToString();
                            //testOK = (retValue > 0);

                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue ? "Pass" : "Fail";
                            testOK = (retValue);
                        }

                        else if (test_steep == (int)PLC_STEEPS.PROXIMITY_TEST) //
                        {
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "4095";
                            relayBoards1.SetAllState(false);
                            //dong relay noi j2.17 - 1500 - AGND
                            //yeu cau test proximity
                            DUT.XMC_ProximitySensor_Req();

                            relayBoards1.SetOutput(7, true);
                            relayBoards1.SetOutput(8, true);
                            Conv.delay_ms(3000);
                            
                            //doc ket qua
                            int retValue1; 
                            bool result =  DUT.XMC_ProximitySensor_Read(out retValue1);
                            //int retValue2;
                            //DUT.XMC_ProximitySensor_Read(out retValue2);
                            //DUT.XMC_ProximitySensor_Read(out retValue1);
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue1;
                            ////dong relay noi j2.17 - 330 - 5V
                            //relayBoards1.SetOutput(7, false);
                            
                            ////yeu cau test proximity
                            //DUT.XMC_ProximitySensor_Req();
                            ////doc ket qua
                            //int retValue2;
                            //DUT.XMC_ProximitySensor_Read(out retValue2);

                            //ngat relay
                            relayBoards1.SetAllState(false);

                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue1.ToString() + "|" + (result ? "1" : "0");
                            //testOK = (Math.Abs(retValue1 - 4095) < 409);
                            testOK = (result);
                        }
                        else if (test_steep == (int)PLC_STEEPS.XMC_FW_VERSION) //
                        {
                            string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'"));
                            if (checkValue.Split('|').Length > 1)
                            {
                                checkValue = checkValue.Split('|')[0];
                            }
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                            string retvalue = DUT.XMC_Read_SoftwareVersion();
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retvalue;
                            testOK = (retvalue == checkValue);
                        }
                        else if (test_steep == (int)PLC_STEEPS.STOP_TEST_XMC) //
                        {
                            DUT.CAN_StopTest();
                            DUT.CAN_StopTest();
                            DUT.CAN_StopTest();
                            testOK = true;
                            powerBoard1.SetOutput(4, false);
                            Conv.delay_ms(1500);
                            powerBoard1.SetOutput(4, true);
                            Conv.delay_ms(CountTimeBetweenTestXmcAndIMX);
                        }

                        //
                        else if (test_steep == (int)PLC_STEEPS.START_TEST_IMX) //Start Test
                        {
                            
                            testOK = DUT.TCP_StartTest();
                            if (!testOK && --retryStartIMX >= 0) goto StartAgainIfStartIMXFail;
                            //while(!testOK && thread_started && ++s_count<3)
                            //{
                            //    Conv.delay_ms(500);
                            //    testOK = DUT.TCP_StartTest();
                            //}    
                        }
                        else if (test_steep == (int)PLC_STEEPS.SERIAL_NUMBER) //Check Serial
                        {
                            string dutValue = DUT.TCP_ReadSerialNumber();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = txtSerialNo.Text;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (txtSerialNo.Text == dutValue);
                        }
                        else if (test_steep == (int)PLC_STEEPS.SOFTWARE_VERSION) //Software Version
                        {
                            string dutValue = DUT.TCP_ReadSoftwareVersion();
                            string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'"));
                            if (checkValue.Split('|').Length > 1)
                            {
                                checkValue = checkValue.Split('|')[1];
                            }
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (checkValue == dutValue);
                        }
                        else if (test_steep == (int)PLC_STEEPS.HARDWARE_VERSION)
                        {
                            string dutValue = DUT.TCP_ReadHardwareVersion();
                            string checkValue = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='HardwareVersion'"));
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = checkValue;
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = dutValue;
                            testOK = (checkValue == dutValue);
                        }
                        else if (test_steep == (int)PLC_STEEPS.DDR_TEST) //
                        {
                            string retValue = DUT.TCP_DDRTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        else if (test_steep == (int)PLC_STEEPS.EMMC_TEST) //
                        {
                            string retValue = DUT.TCP_EMMCTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        //else if (test_steep == (int)PLC_STEEPS.FLASH_TEST) //EXTERNAL FLASH check
                        //{
                        //    string retValue = DUT.TCP_FlashTest();
                        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                        //    testOK = (retValue.ToUpper() == "PASS");
                        //}
                        //else if (test_steep == (int)PLC_STEEPS.UART_TEST) //
                        //{
                        //    string retValue = DUT.TCP_UART_Test();
                        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                        //    testOK = (retValue.ToUpper() == "PASS");
                        //}
                        else if (test_steep == (int)PLC_STEEPS.UART4_TEST) //
                        {
                            string retValue = DUT.TCP_UART4_Test();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        else if (test_steep == (int)PLC_STEEPS.UART5_TEST) //
                        {
                            powerBoard1.SetOutput(3,true);
                            string retValue = DUT.TCP_UART5_Test();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        else if (test_steep == (int)PLC_STEEPS.CAN_TEST) //
                        {
                            string retValue = DUT.TCP_CANTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        else if (test_steep == (int)PLC_STEEPS.I2C_TEST) //
                        {
                            string retValue = DUT.TCP_I2CTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        else if (test_steep == (int)PLC_STEEPS.ETHERNET_TEST) //
                        {
                            string retValue = DUT.TCP_EthernetTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue.ToUpper() == "PASS");
                        }
                        //else if (test_steep == (int)PLC_STEEPS.TEMP1_TEST) //
                        //{
                        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "62±6|116±11|142±14|166±16";
                            
                        //    relayBoards1.SetAllState(false);
                        //    //dong relay noi TP93 - ADC com
                        //    relayBoards1.SetOutput(1,true);
                        //    //dong tro 1000
                        //    relayBoards1.SetOutput(3, true,1000);
                        //    //doc ket qua
                        //    string retValue = DUT.TCP_TempTest("temp1");
                        //    double retValueInt1 = Conv.atod(retValue);
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                        //    //dong tro 1155
                        //    relayBoards1.SetOutput(3, false);
                        //    relayBoards1.SetOutput(4, true,1000);
                        //    //doc ket qua
                        //    retValue =  DUT.TCP_TempTest("temp1");
                        //    double retValueInt2 = Conv.atod(retValue);
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "|" + retValue;
                        //    //dong tro 1232
                        //    relayBoards1.SetOutput(4, false);
                        //    relayBoards1.SetOutput(5, true,1000);
                        //    //doc ket qua
                        //    retValue = DUT.TCP_TempTest("temp1");
                        //    double retValueInt3 = Conv.atod(retValue);
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "|" + retValue;
                        //    //dong tro 1308
                        //    relayBoards1.SetOutput(5, false);
                        //    relayBoards1.SetOutput(6, true,1000);
                        //    //doc ket qua
                        //    retValue = DUT.TCP_TempTest("temp1");
                        //    double retValueInt4 = Conv.atod(retValue);
                        //    //ngat TP93 - ADC com
                        //    relayBoards1.SetOutput(1, false);
                        //    relayBoards1.SetOutput(6, false);

                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "|" + retValue;
                        //    testOK = (Math.Abs(retValueInt1 - 62) < 6 && Math.Abs(retValueInt2 - 116) < 11 && Math.Abs(retValueInt3 - 142) < 14 && Math.Abs(retValueInt4 - 166) < 16);
                        //}
                        //else if (test_steep == (int)PLC_STEEPS.TEMP2_TEST) //
                        //{
                        //    dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "62±6|116±11|142±14|166±16";
                        //    relayBoards1.SetAllState(false);
                        //    //dong relay noi TP93 - ADC com
                        //    relayBoards1.SetOutput(2, true);
                        //    //dong tro 1000
                        //    relayBoards1.SetOutput(3, true, 1000);
                        //    //doc ket qua
                        //    string retValue = DUT.TCP_TempTest("temp2");
                        //    double retValueInt1 = Conv.atod(retValue);
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                        //    //dong tro 1155
                        //    relayBoards1.SetOutput(3, false);
                        //    relayBoards1.SetOutput(4, true, 1000);
                        //    //doc ket qua
                        //    retValue = DUT.TCP_TempTest("temp2");
                        //    double retValueInt2 = Conv.atod(retValue);
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "|" + retValue;
                        //    //dong tro 1232
                        //    relayBoards1.SetOutput(4, false);
                        //    relayBoards1.SetOutput(5, true, 1000);
                        //    //doc ket qua
                        //    retValue = DUT.TCP_TempTest("temp2");
                        //    double retValueInt3 = Conv.atod(retValue);
                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "|" + retValue;
                        //    //dong tro 1308
                        //    relayBoards1.SetOutput(5, false);
                        //    relayBoards1.SetOutput(6, true, 1000);
                        //    //doc ket qua
                        //    retValue = DUT.TCP_TempTest("temp2");
                        //    double retValueInt4 = Conv.atod(retValue);
                        //    //ngat TP93 - ADC com
                        //    relayBoards1.SetOutput(2, false);
                        //    relayBoards1.SetOutput(6, false);

                        //    dgvTestList.Rows[test_steep].Cells["ReadValue"].Value += "|" + retValue;
                        //    testOK = (Math.Abs(retValueInt1 - 62) < 6 && Math.Abs(retValueInt2 - 116) < 11 && Math.Abs(retValueInt3 - 142) < 14 && Math.Abs(retValueInt4 - 166) < 16);
                        //}
                        
                        else if (test_steep == (int)PLC_STEEPS.PLC_IC_INFO) //
                        {
                            string retValue = DUT.TCP_PLCICInfoTest();
                            dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Pass/Fail";
                            dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                            testOK = (retValue != "");
                            //dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "Skip";
                            //testOK = true;
                        }
                        else if (test_steep == (int)PLC_STEEPS.HLC_SLAC_TEST) //
                        {
                            //noi dut voi EV
                            powerBoard1.SetOutput(3, true);
                            // PEV
                            //kiem tra Ethernet trên board EV
                            AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text + ": Kiểm tra trạng thái ethernet trên EV board");
                            if (evalator1.CheckEthernetStatus(100))
                            {
                                evalator1.SentSLAC();
                                AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text + ": Kiểm tra HLC từ DUT");
                                //doc ket qua
                                string retValue = DUT.TCP_HLCSlacTest();
                                dgvTestList.Rows[test_steep].Cells["CheckValue"].Value = "PASS/FAIL";
                                dgvTestList.Rows[test_steep].Cells["ReadValue"].Value = retValue;
                                testOK = (retValue.ToUpper() == "PASS");
                            }
                            
                        }

                        else if (test_steep == (int)PLC_STEEPS.STOP_TEST) //
                        {
                            
                            testOK = DUT.TCP_StopTest();
                            powerBoard1.SetOutput(3,false);
                        }
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
                powerBoard1.SetOutput(3, false);
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

    private void evalator1_Load(object sender, EventArgs e)
    {

    }
}
