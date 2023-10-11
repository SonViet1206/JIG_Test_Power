using DefaultNS;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZedGraph;
using DefaultNS.Model;
using System.Linq;
using MES_INTERFACE;
using System.Collections.Generic;
using MES;

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

    int BoardType = 0;

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
        RestApiHelper.InitGlobalVarial();

        //OutputGetSNStatus res = RestApiHelper.GetSNStatus("S02HS11310000001", "TEST01_PT_1");
        //if(res != null)
        //{
        //    MessageBox.Show(res.SN + "\r\n" + res.MachineName + "\r\n" + res.Confirm + "\r\n" + res.ErrorCode);
        //}
        //else
        //{
        //    MessageBox.Show("Lỗi get trạng thái của mã SN trên mesh!");
        //}

        

        lblBuildInfo.Text = "Ver:" + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
        cboOperator.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Operator'"));
        numTemprature.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Temperature'"));

        ReloadCounter();
    }


    private int TotalCount = 0;
    private int PassCount = 0;
    private int LastCountTime = 0;
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
    string PathFwStm = "";
    string PathFwIMX = "";
    string MD5FwFromMES = "";
    string MD5FwStmFromIniFile = "";

    private void frmMain_Shown(object sender, EventArgs e)
    {
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");
        isTestOffline = ini.IniReadValue("API_CONNECTION", "isTestOffLine", "1") == "1" ? true : false;
        lbMeshStatus.BackColor = isTestOffline ? Color.DarkSlateGray : Color.Lime;
        MachineName = ini.IniReadValue("API_CONNECTION", "MachineName", "TEST01_PT_1");

        PathFwStm = ini.IniReadValue("API_CONNECTION", "PathFwStm");
        PathFwIMX = ini.IniReadValue("API_CONNECTION", "PathFwImx");
        MD5FwStmFromIniFile = ini.IniReadValue("API_CONNECTION", "MD5FwStm");

        lblPowerBoardPortName.Text = ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1");
        lblRelaysBoardPortName.Text = ini.IniReadValue("COMMUNICATION", "RelaysBoardCommPort", "COM2");


        AddLogWindow("Khởi động chương trình - MES: " + (isTestOffline ? "OFF" : "ON"));

        ReInitBoardType();

        btnReload_Click(null, null);

        DMM.Start();
        powerBoard1.Start(lblPowerBoardPortName.Text);

        timer1s.Enabled = true;

        SetControlStatus(STATUS.Ready);

        this.WindowState = FormWindowState.Maximized;


        //
        

    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        IniFile ini = new IniFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AppConfig.ini");

        DMM.Stop();
        relayBoards1.Stop();

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
            //lblTestTime.Text = (ts.TotalSeconds / 60).ToString("00") + ":" + (ts.TotalSeconds % 60).ToString("00");
            //lblTestTime.Text = Math.Truncate((ts.TotalSeconds / 60)).ToString("00") + ":" + (ts.TotalSeconds % 60).ToString("00");
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
            if(!isTestOffline)
            {
                //AddLogWindow("Kiểm tra trạng thái của mã: " + txtSerialNo.Text + " trên MES");
                //OutputGetSNStatus res = RestApiHelper.GetSNStatus(txtSerialNo.Text, MachineName);
                //if (res != null)
                //{
                //    //MessageBox.Show(res.SN + "\r\n" + res.MachineName + "\r\n" + res.Confirm + "\r\n" + res.ErrorCode);
                //    if (res.Confirm == "OK")
                //    {
                //        //Get Fw từ MES
                //        OutPutGetSNInfo resGetSNInfo = RestApiHelper.GetSNInfo(txtSerialNo.Text, MachineName, "TESTTOOL");
                //        if (resGetSNInfo != null)
                //        {
                //            Global.WriteLogFile("[Get SN info]" + "Kiểm tra thông tin mã serial trên MES\r\nMã Serial Number: " + resGetSNInfo.SN + "\r\nMachine Name: " + resGetSNInfo.MachineName + "\r\nModelCode: " + resGetSNInfo.ModelCode + "\r\nStatus: " + resGetSNInfo.Status + "\r\nError Code: " + resGetSNInfo.ErrorCode + "\r\nFirmware: " + resGetSNInfo.Firmware);
                //            if (resGetSNInfo.Status == "OK")
                //            {
                //                MD5FwFromMES = resGetSNInfo.Firmware;
                //                EnterTestSteep(0);
                //            }
                //            //MessageBox.Show("Mã Serial Number: " + res.SN + "\r\nMachine Name: " + res.MachineName + "\r\nConfirm: " + res.Confirm + "\r\nError Code: " + res.ErrorCode, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //            else
                //            {
                //                MessageBox.Show("Kiểm tra thông tin mã serial trên MES\r\nMã Serial Number: " + resGetSNInfo.SN + "\r\nMachine Name: " + resGetSNInfo.MachineName + "\r\nModelCode: " + resGetSNInfo.ModelCode + "\r\nStatus: " + resGetSNInfo.Status + "\r\nError Code: " + resGetSNInfo.ErrorCode + "\r\nFirmware: " + resGetSNInfo.Firmware, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //                return;
                //            }
                //        }
                //        else
                //        {
                //            MessageBox.Show("Lỗi lấy thông tin của mã Serial Number trên MES!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //            return;
                //        }

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
        BoardType = Conv.atoi32(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='BoardType'"), 0);
        if (BoardType > -1 && BoardType < 2)
        {
            lblCaption.Text = Global.TITLES[BoardType];
            string[] rls = Global.RELAY_BOARDS[BoardType].Split('|');
            if (rls.Length > 0)
            {
                relayBoards1.Stop();
                relayBoards1.Start(lblRelaysBoardPortName.Text, rls[0]);
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
            MessageBox.Show("Không tìm thấy thông tin BOARD mạch phù hợp");
        }
    }

    private void btnConfig_Click(object sender, EventArgs e)
    {
        string password = Global.PasswordInput();
        if (password.Length > 0)
        {
            if (password == Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
            {
                frmConfig frm = new frmConfig();
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

    #region OLD
    /*
    private void btnSave_Click1(object sender, EventArgs e)
    {
        //ledResult.Text = "OK";
        if (ledResult.Text == "OK")
        {
            //isTestOffline = false;
            try
            {
                string sBuff = "";
                string path_name = DataLogPath + @"\PASS" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
                sBuff += "Operator," + cboOperator.Text + "\r\n";
                sBuff += "Equipment,ADCMT-7351E\r\n";
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";

                sBuff += "#,NAME,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                    string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                    string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                    string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                }

                File.WriteAllText(path_name, sBuff, Encoding.UTF8);
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


                AddLogWindow("Ghi log file thành công!");
                if (isTestOffline)
                {
                    PassCount += 1;
                    TotalCount += 1;
                    SetControlStatus(STATUS.Ready);
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
                bool HasFirmware = false;
                for(int i= 0; i< dgvTestList.Rows.Count; i++)
                {
                    if (Conv.atoi32(dgvTestList.Rows[i].Cells["PopupCheck"].Value) == 2 && Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value).Contains("USB"))
                    {
                        HasFirmware = true;
                        break;
                    }
                    else if (Conv.atoi32(dgvTestList.Rows[i].Cells["PopupCheck"].Value) == 2) //Yêu cầu nạp frimware STM
                    {
                        HasFirmware = true;
                        break;
                    }    
                }

                if (HasFirmware)
                {
                    AddLogWindow("Lưu firmware version của mã: " + txtSerialNo.Text + " lên MES");
                    try
                    {

                        //Update firmware version
                        //cbi du lieu
                        OutputUpdateFirmwareInfo res = RestApiHelper.UpdateFirmwareInfo(txtSerialNo.Text, MachineName, "TESTTOOL", MD5FwFromMES, MD5FwFromMES);
                        if (res != null)
                        {
                            if (res.Result == "OK")
                            {
                                AddLogWindow("Cập nhật firmware version lên MES thành công!");

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
                                        dtab.Rows[r]["TestName"] = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                                        dtab.Rows[r]["LowValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MinValue"].Value);
                                        dtab.Rows[r]["ReadValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["PValue"].Value);
                                        dtab.Rows[r]["HighValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                                        dtab.Rows[r]["Result"] = Conv.atos0(dgvTestList.Rows[r].Cells["Result"].Value);
                                        dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                                    }

                                    // Insert

                                    OutputInsertPassFailDetailTestCase resInsert = RestApiHelper.InsertPassFailDetailTestCase(txtSerialNo.Text, MachineName, cboOperator.Text, dtab);
                                    if (resInsert != null)
                                    {
                                        if (resInsert.Status == "OK")
                                        {
                                            PassCount += 1;
                                            TotalCount += 1;
                                            AddLogWindow("Ghi MES thành công!");
                                            SetControlStatus(STATUS.Ready);
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
                            else
                            {
                                AddLogWindow("Cập nhật firmware version lên MES không thành công\r\nStatus: " + res.SN + "\r\nMachineName: " + res.MachineName + "\r\nResult: " + res.Result + "\r\nError Code: " + res.ErrorCode + "\r\nUserName: " + res.UserName + "\r\nFirmware: " + res.Firmware + "\r\nFirmwareVersion: " + res.FirmwareVersion);
                                MessageBox.Show("Cập nhật firmware version lên MES không thành công\r\nSN: " + res.SN + "\r\nMachineName: " + res.MachineName + "\r\nResult: " + res.Result + "\r\nError Code: " + res.ErrorCode + "\r\nUserName: " + res.UserName + "\r\nFirmware: " + res.Firmware + "\r\nFirmwareVersion: " + res.FirmwareVersion, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            AddLogWindow("Cập nhật firmware version lên MES không thành công");
                            MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Global.WriteLogFile("[Update Fiwmware Version to MES] - " + ex.Message);
                    }
                }
                else
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
                            dtab.Rows[r]["TestName"] = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                            dtab.Rows[r]["LowValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MinValue"].Value);
                            dtab.Rows[r]["ReadValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["PValue"].Value);
                            dtab.Rows[r]["HighValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                            dtab.Rows[r]["Result"] = Conv.atos0(dgvTestList.Rows[r].Cells["Result"].Value);
                            dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                        }

                        // Insert

                        OutputInsertPassFailDetailTestCase resInsert = RestApiHelper.InsertPassFailDetailTestCase(txtSerialNo.Text, MachineName, cboOperator.Text, dtab);
                        if (resInsert != null)
                        {
                            if (resInsert.Status == "OK")
                            {
                                PassCount += 1;
                                TotalCount += 1;
                                AddLogWindow("Ghi MES thành công!");
                                SetControlStatus(STATUS.Ready);
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
            
        }
        else if (ledResult.Text == "NG")
        {
            //////////isTestOffline = false;
            try
            {
                string sBuff = "";
                string path_name = DataLogPath + @"\FAIL" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
                sBuff += "Operator," + cboOperator.Text + "\r\n";
                sBuff += "Equipment,ADCMT-7351E\r\n";
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";

                sBuff += "#,NAME,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                    string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                    string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                    string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                }

                File.WriteAllText(path_name, sBuff, Encoding.UTF8);
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



                AddLogWindow("Ghi log file thành công!");
                if (isTestOffline)
                {
                    TotalCount += 1;
                    SetControlStatus(STATUS.Ready);
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
                        dtab.Rows[r]["TestName"] = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                        dtab.Rows[r]["LowValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MinValue"].Value);
                        dtab.Rows[r]["ReadValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["PValue"].Value);
                        dtab.Rows[r]["HighValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                        dtab.Rows[r]["Result"] = Conv.atos0(dgvTestList.Rows[r].Cells["Result"].Value);
                        dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value); 
                    }

                    // Insert
                    OutputInsertDetailTestCase res = RestApiHelper.InsertDetailTestCase(txtSerialNo.Text, MachineName, dtab);
                    if (res != null)
                    {
                        if (res.Confirm == "OK")
                        {
                            TotalCount += 1;
                            AddLogWindow("Ghi MES thành công!");
                            SetControlStatus(STATUS.Ready);
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
                path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
                sBuff += "Operator," + cboOperator.Text + "\r\n";
                sBuff += "Equipment,ADCMT-7351E\r\n";
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";

                sBuff += "#,NAME,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                    string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                    string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                    string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                }

                File.WriteAllText(path_name, sBuff, Encoding.UTF8);



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
                        dtab.Rows[r]["TestName"] = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                        dtab.Rows[r]["LowValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MinValue"].Value);
                        dtab.Rows[r]["ReadValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["PValue"].Value);
                        dtab.Rows[r]["HighValue"] = Conv.atos0(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                        dtab.Rows[r]["Result"] = Conv.atos0(dgvTestList.Rows[r].Cells["Result"].Value);
                        dtab.Rows[r]["TestTime"] = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    }

                    // Insert
                    OutputInsertDetailTestCase res = RestApiHelper.InsertDetailTestCase(txtSerialNo.Text, MachineName, dtab);
                    if (res != null)
                    {
                        AddLogWindow("Ghi MES: Status: " + res.Confirm + "\r\nError Code: " + res.ErrorCode);
                        if (res.Confirm == "OK")
                        {
                            TotalCount += 1;
                            AddLogWindow("Ghi MES thành công!");
                            SetControlStatus(STATUS.Ready);
                        }
                        else
                        {
                            //AddLogWindow("Ghi MES không thành công\r\nStatus: " + res.Confirm + "\r\nError Code: " + res.ErrorCode);
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


    #region NEW
    
    private void btnSave_Click1(object sender, EventArgs e)
    {
        //ledResult.Text = "OK";
        if (ledResult.Text == "OK")
        {
            //isTestOffline = false;
            try
            {
                string sBuff = "";
                string path_name = DataLogPath + @"\PASS" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
                sBuff += "Operator," + cboOperator.Text + "\r\n";
                sBuff += "Equipment,ADCMT-7351E\r\n";
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";

                sBuff += "#,NAME,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                    string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                    string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                    string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                }

                File.WriteAllText(path_name, sBuff, Encoding.UTF8);
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


                AddLogWindow("Ghi log file thành công!");
                if (isTestOffline)
                {
                    PassCount += 1;
                    TotalCount += 1;
                    SetControlStatus(STATUS.Ready);
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
                        itemInfor.MinValue = Conv.atos0(dgvTestList.Rows[i].Cells["MinValue"].Value);
                        itemInfor.Value = Conv.atos0(dgvTestList.Rows[i].Cells["PValue"].Value);
                        itemInfor.MaxValue = Conv.atos0(dgvTestList.Rows[i].Cells["MaxValue"].Value);
                        itemInfor.TestResult = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value);
                        itemInfor.TestTime = Conv.atos(dgvTestList.Rows[i].Cells["BeginTest"].Value);

                        lstItems.Add(itemInfor);
                    }

                    string res = mesInterface.UpdateTestListToServer(txtSerialNo.Text, "OK", lstItems);
                    if (res == "OK")
                    {
                        AddLogWindow("Ghi MES thành công!");

                        PassCount += 1;
                        TotalCount += 1;
                        SetControlStatus(STATUS.Ready);
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
            //////////isTestOffline = false;
            try
            {
                string sBuff = "";
                string path_name = DataLogPath + @"\FAIL" + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
                if (!Directory.Exists(path_name)) Directory.CreateDirectory(path_name);
                path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
                sBuff += "Operator," + cboOperator.Text + "\r\n";
                sBuff += "Equipment,ADCMT-7351E\r\n";
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";

                sBuff += "#,NAME,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                    string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                    string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                    string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                }

                File.WriteAllText(path_name, sBuff, Encoding.UTF8);
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



                AddLogWindow("Ghi log file thành công!");
                if (isTestOffline)
                {
                    TotalCount += 1;
                    SetControlStatus(STATUS.Ready);
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
                        itemInfor.MinValue = Conv.atos0(dgvTestList.Rows[i].Cells["MinValue"].Value);
                        itemInfor.Value = Conv.atos0(dgvTestList.Rows[i].Cells["PValue"].Value);
                        itemInfor.MaxValue = Conv.atos0(dgvTestList.Rows[i].Cells["MaxValue"].Value);
                        itemInfor.TestResult = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value);
                        itemInfor.TestTime = Conv.atos(dgvTestList.Rows[i].Cells["BeginTest"].Value);

                        lstItems.Add(itemInfor);
                    }

                    string res = mesInterface.UpdateTestListToServer(txtSerialNo.Text, "NG", lstItems);
                    if (res == "OK")
                    {
                        AddLogWindow("Ghi MES thành công!");

                        TotalCount += 1;
                        SetControlStatus(STATUS.Ready);

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
                path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
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
                sBuff += "Operator," + cboOperator.Text + "\r\n";
                sBuff += "Equipment,ADCMT-7351E\r\n";
                sBuff += "Location,VinSmart\r\n";
                sBuff += "Temperature," + numTemprature.Value.ToString("0.0") + "\r\n";
                sBuff += "Serial Number," + txtSerialNo.Text + "\r\n";
                sBuff += "Test Time," + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                sBuff += "Test Result," + ((ledResult.Text == "OK") ? "OK" : "NG") + "\r\n";

                sBuff += "#,NAME,MIN,MAX,VALUE,UNIT,START,ELAPSE(s),RESULT\r\n";

                for (int r = 0; r < dgvTestList.Rows.Count; r++)
                {
                    string TestOrder = Conv.atos(dgvTestList.Rows[r].Cells["TestOrder"].Value);
                    string TestName = Conv.atos0(dgvTestList.Rows[r].Cells["TestName"].Value);
                    string MinValue = Conv.atos(dgvTestList.Rows[r].Cells["MinValue"].Value);
                    string MaxValue = Conv.atos(dgvTestList.Rows[r].Cells["MaxValue"].Value);
                    string PValue = Conv.atos(dgvTestList.Rows[r].Cells["PValue"].Value);
                    string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                    string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                    string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                    string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                    sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
                }

                File.WriteAllText(path_name, sBuff, Encoding.UTF8);



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
                        itemInfor.MinValue = Conv.atos0(dgvTestList.Rows[i].Cells["MinValue"].Value);
                        itemInfor.Value = Conv.atos0(dgvTestList.Rows[i].Cells["PValue"].Value);
                        itemInfor.MaxValue = Conv.atos0(dgvTestList.Rows[i].Cells["MaxValue"].Value);
                        itemInfor.TestResult = Conv.atos(dgvTestList.Rows[i].Cells["Result"].Value);
                        itemInfor.TestTime = Conv.atos(dgvTestList.Rows[i].Cells["BeginTest"].Value);

                        lstItems.Add(itemInfor);
                    }

                    string res = mesInterface.UpdateTestListToServer(txtSerialNo.Text, "NG", lstItems);
                    if (res == "OK")
                    {
                        TotalCount += 1;
                        AddLogWindow("Ghi MES thành công!");
                        SetControlStatus(STATUS.Ready);
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
    
    #endregion


    private void btnReload_Click(object sender, EventArgs e)
    {
        string strSQL;
        DataTable dtab;

        //Đọc lại thông tin cấu hình:
        DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
        if (!Directory.Exists(DataLogPath)) DataLogPath = Application.StartupPath + @"\DataLogs";
        if(BoardType > -1 && BoardType < 7)
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

                }
            }
            txtTotalSteep.Text = dgvTestList.Rows.Count.ToString();
        }
        //
        
    }

    private bool SetPower(bool state)
    {
        //btnOUT4.BackColor = ((new_states1 & 0x800) != 0) ? Color.Red : Color.DimGray;
        if (state && ((powerBoard1.States & 0x800) == 0))
            return powerBoard1.SetOutput(4, true);
        else if (!state && ((powerBoard1.States & 0x800) != 0))
            return powerBoard1.SetOutput(4, false);
        return true;
    }

    public static string MD5(string path)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(path))
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToUpper();
            }
        }
    }

    string[] LastRelays = new string[0];
    bool CheckStepLastFwSTM = false;
    private void EnterTestSteep(int test_steep)
    {
        if (test_steep > -1 && test_steep < dgvTestList.Rows.Count)
        {
            //while (test_steep < dgvTestList.Rows.Count)
            powerBoard1.SetOutput(1, true);
            try
            {
                SetControlStatus(STATUS.Start);

                thread_ended = false;
                thread_started = true;

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

                    AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text);

                    //Xử lý tắt rơ le:
                    if (relayOK)
                    {
                        //Tắt các rơ-le theo thứ tự của bước trước:
                        if (LastRelays.Length > 0)
                        {
                            if (LastRelays[0] != "" && relayOK)
                            {
                                for (int i = LastRelays.Length-1; i >=0; i--)
                                {
                                    if (LastRelays[i] != "0")
                                    {
                                        relayOK = relayBoards1.SetOutput(Conv.atoi32(LastRelays[i]), false);
                                        if (!relayOK)
                                        {
                                            AddLogWindow("Lỗi tắt rơ-le", true);
                                        }
                                    }
                                }
                            }
                        }
                        //Tắt các rơ-le của bước trước:
                        relayOK = relayBoards1.SetAllState(false);
                        if (!relayOK) AddLogWindow("Lỗi tắt rơ-le all", true);
                    }
                    testOK = relayOK;

                    // lấy trạng thái rơ le của bước trước
                    LastRelays = Relays;


                    //Nếu bước này không có yêu cầu nguồn thì tắt
                    if (!powerRequest) SetPower(false);

                    if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 2 && Conv.atos(dgvTestList.Rows[test_steep].Cells["TestName"].Value).Contains("USB")) //Yêu cầu nạp frimware IMX
                    {
                        if (!isTestOffline)
                        {
                            //string PathFileStm = "";
                            string PathFileImx = "";
                            //Scan folfer chứa file fw
                            //if (PathFwStm != "")
                            //{
                            //    DirectoryInfo sourDirFwStm = new DirectoryInfo(PathFwStm);
                            //    FileInfo[] logFiles = new FileInfo[0];
                            //    logFiles = sourDirFwStm.GetFiles("*.sap", System.IO.SearchOption.TopDirectoryOnly).OrderByDescending(aa => aa.CreationTime).ToArray();
                            //    if (logFiles.Length > 0)
                            //    {
                            //        PathFileStm = logFiles[0].FullName;
                            //    }
                            //}
                            if (PathFwIMX != "")
                            {
                                DirectoryInfo sourDirFwImx = new DirectoryInfo(PathFwIMX);
                                FileInfo[] logFiles = new FileInfo[0];
                                logFiles = sourDirFwImx.GetFiles("*.bz2", System.IO.SearchOption.TopDirectoryOnly).OrderByDescending(aa => aa.CreationTime).ToArray();
                                if (logFiles.Length > 0)
                                {
                                    PathFileImx = logFiles[0].FullName;
                                }
                            }
                            
                            //Check ma MD5
                            //string _MD5serialStm = PathFwStm != "" ? MD5(PathFwStm) : "";
                            //string _MD5serialImx = PathFwIMX != "" ? MD5(PathFwIMX) : "";

                            //string _MD5serialStm = PathFileStm != "" ? MD5(PathFileStm) : "";
                            string _MD5serialImx = PathFileImx != "" ? MD5(PathFileImx) : "";

                            //Global.WriteLogFile("[Caculate MD5]\r\n" + "Path=" + PathFwIMX + "\r\nMD5FwImx=" + _MD5serialImx);
                            Global.WriteLogFile("[Caculate MD5]\r\n" + "Path=" + PathFileImx + "\r\nMD5FwImx=" + _MD5serialImx);

                            //Neu ma MD5stm khac voi md5 lay tu mes
                            if (_MD5serialImx != "" && _MD5serialImx != MD5FwFromMES)
                            {
                                MessageBox.Show("Mã MD5 firmware IMX không giống với mã MD5 từ MES");
                                testOK = false;
                                goto EndCurSteep1;
                            }

                            //if (_MD5serialImx != "")
                            //{
                            //    string res = mesInterface.UpdateFirmwareInfo(txtSerialNo.Text, _MD5serialImx, "");
                            //    if(res != "OK")
                            //    {
                            //        MessageBox.Show("Mã MD5 firmware IMX không giống với mã MD5 từ MES");
                            //        testOK = false;
                            //        goto EndCurSteep1;
                            //    }
                            //}
                        }
                        
                        //Xử lý bật rơ le:
                        if (relayOK)
                        {
                            //Tắt nguồn:
                            SetPower(false);
                            //Bật các rơ-le của bước này:
                            if (Relays[0] != "" && relayOK)
                            {
                                for (int i = 0; i < Relays.Length; i++)
                                {
                                    if (Relays[i] != "0")
                                    {
                                        relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                        if (!relayOK)
                                        {
                                            AddLogWindow("Lỗi bật rơ-le", true);
                                        }
                                    }
                                }
                            }
                            //Bật nguồn:
                            Conv.delay_ms(2000);
                            SetPower(true);
                        }
                        else //Trong mọi tình huống lỗi tắt cấp nguồn
                        {
                            SetPower(false);
                        }

                        if (!relayOK) testOK = false;

                        if (testOK)
                        {
                            frmFlashFirmware frm = new frmFlashFirmware();
                            frm.txtSerialNo.Text = txtSerialNo.Text;
                            frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                            testOK = frm.state;
                            /*   23-02-2021 -- Linh */
                            SetPower(false);
                            Conv.delay_ms(2000);
                        }
                    EndCurSteep1:
                        Conv.delay_ms(10);

                    }
                    else if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 2) //Yêu cầu nạp frimware STM
                    {
                        if (!isTestOffline)
                        {
                            string PathFileStm = "";
                            string PathFileImx = "";
                            //Scan folfer chứa file fw
                            if (PathFwStm != "")
                            {
                                DirectoryInfo sourDirFwStm = new DirectoryInfo(PathFwStm);
                                FileInfo[] logFiles = new FileInfo[0];
                                logFiles = sourDirFwStm.GetFiles("*.sap", System.IO.SearchOption.TopDirectoryOnly).OrderByDescending(aa => aa.CreationTime).ToArray();
                                if (logFiles.Length > 0)
                                {
                                    PathFileStm = logFiles[0].FullName;
                                }
                            }
                            if (PathFwIMX != "")
                            {
                                DirectoryInfo sourDirFwImx = new DirectoryInfo(PathFwIMX);
                                FileInfo[] logFiles = new FileInfo[0];
                                logFiles = sourDirFwImx.GetFiles("*.bz2", System.IO.SearchOption.TopDirectoryOnly).OrderByDescending(aa => aa.CreationTime).ToArray();
                                if (logFiles.Length > 0)
                                {
                                    PathFileImx = logFiles[0].FullName;
                                }
                            }

                            //Check ma MD5
                            //string _MD5serialStm = PathFwStm != "" ? MD5(PathFwStm) : "";
                            //string _MD5serialImx = PathFwIMX != "" ? MD5(PathFwIMX) : "";

                            string _MD5serialStm = PathFileStm != "" ? MD5(PathFileStm) : "";
                            string _MD5serialImx = PathFileImx != "" ? MD5(PathFileImx) : "";

                            //Global.WriteLogFile("[Caculate MD5]\r\n" + "Path=" + PathFwStm + "\r\nMD5FwStm=" + _MD5serialStm);
                            Global.WriteLogFile("[Caculate MD5]\r\n" + "Path=" + PathFileStm + "\r\nMD5FwStm=" + _MD5serialStm);
                            //Neu ma MD5stm khac voi md5 lay tu mes
                            if (_MD5serialStm != "" && _MD5serialStm != MD5FwFromMES && _MD5serialImx == "")
                            {
                                MessageBox.Show("Mã MD5 firmware STM không đúng với mã MD5 từ MES");
                                testOK = false;
                                goto EndCurSteep2;
                            }
                            //if (_MD5serialStm != ""  && _MD5serialImx == "")
                            //{
                            //    string res = mesInterface.UpdateFirmwareInfo(txtSerialNo.Text, _MD5serialStm,"");
                            //    if(res != "OK")
                            //    {
                            //        MessageBox.Show("Mã MD5 firmware STM không đúng với mã MD5 từ MES");
                            //        testOK = false;
                            //        goto EndCurSteep2;
                            //    }
                            //}
                            else if (_MD5serialStm != "" && _MD5serialImx != "")
                            {
                                //nhap maunual md5
                                //string _md5_input = Global.PasswordInput("", false, 2);
                                string _md5_input = MD5FwStmFromIniFile;
                                if (_md5_input.Length <= 0 || _md5_input != _MD5serialStm)
                                {
                                    MessageBox.Show("Mã MD5 firmware STM không đúng");
                                    testOK = false;
                                    goto EndCurSteep2;
                                }

                            }
                        }
                        
                        //Xử lý bật rơ le:
                        if (relayOK)
                        {
                            //Tắt nguồn:
                            SetPower(false);
                            //Bật các rơ-le của bước này:
                            if (Relays[0] != "" && relayOK)
                            {
                                for (int i = 0; i < Relays.Length; i++)
                                {
                                    if (Relays[i] != "0")
                                    {
                                        relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                        if (!relayOK)
                                        {
                                            AddLogWindow("Lỗi bật rơ-le", true);
                                        }
                                    }
                                }
                            }
                            //Bật nguồn:
                            Conv.delay_ms(2000);
                            SetPower(true);
                        }
                        else //Trong mọi tình huống lỗi tắt cấp nguồn
                        {
                            SetPower(false);
                        }

                        if (!relayOK) testOK = false;

                        if (testOK)
                        {
                            frmFlashFirmware frm = new frmFlashFirmware();
                            frm.txtSerialNo.Text = txtSerialNo.Text;
                            frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                            testOK = frm.state;
                            /*   23-02-2021 -- Linh */
                            SetPower(false);
                            Conv.delay_ms(2000);
                        }
                    EndCurSteep2: 
                        Conv.delay_ms(10);

                    }                   
                    else //Xử lý các bước đo thông thường
                    {
                        //Có yêu cầu thông báo cho người dùng:
                        if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) > 0)
                        {
                            frmShowImage frm = new frmShowImage();
                            frm.lblTestSteep.Text = txtTestSteep.Text;
                            frm.lblTestName.Text = txtTestName.Text;
                            frm.txtMessage.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupMessage"].Value);
                            //frm.picHelp.Image = Image.FromFile(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                            //frm.ShowDialog();
                            frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
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
                                        if (!testOK) AddLogWindow("Lỗi đặt thang đo cho DMM", true);
                                    }
                                }
                                else
                                {
                                    AddLogWindow("Lỗi đặt chế độ đo cho DMM", true);
                                }
                            }
                        }

                        bool isDmmACrequest = false;
                        bool isDmmDCrequest = false;

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
                                        if (Conv.atoi32(Relays[i]) <= 32) isDmmACrequest = true;
                                        if (Conv.atoi32(Relays[i]) <= 32) isDmmDCrequest = true;
                                        relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                        if (!relayOK)
                                        {
                                            AddLogWindow("Lỗi bật rơ-le", true);
                                        }
                                        //04-04-2021
                                        Conv.delay_ms(300);
                                    }
                                }
                            }
                            //Nếu có yêu cầu nguồn thì bật:
                            if (powerRequest && isDmmDCrequest)
                            {
                                powerBoard1.SetOutput(3, false);
                                powerBoard1.SetOutput(3, true);
                                SetPower(true);
                            }
                            else
                            {
                                if (isDmmDCrequest) powerBoard1.SetOutput(2,true);
                                else if (isDmmACrequest) powerBoard1.SetOutput(3,true);
                                if (powerRequest) SetPower(true);
                            }
                        }
                        else //Trong mọi tình huống lỗi tắt cấp nguồn
                        {
                            SetPower(false);
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
                                meas_time /= 200;
                                //Có yêu cầu dừng chờ ổn định:
                                do
                                {
                                    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");

                                    --meas_time;
                                    Conv.delay_ms(200);

                                    if (DMM.TextValue != "---------")
                                    {
                                        read_value = DMM.Value / check_factor;

                                        dgvTestList.Rows[test_steep].Cells["PValue"].Value = (DMM.TextValue == "0.L  ") ? "0.L" : read_value.ToString();

                                        //Không quan tâm đến dấu nếu đơn vị là điện trở:
                                        if (check_unit.IndexOf("Ω") > -1)
                                        {
                                            read_value = Math.Abs(read_value);
                                            dgvTestList.Rows[test_steep].Cells["PValue"].Value = (DMM.TextValue == "0.L  ") ? "0.L" : read_value.ToString();
                                        }
                                        testOK = (read_value >= check_min && read_value <= check_max);
                                        //Nếu OK chờ thêm 200ms nưữa đọc kết quả lần cuối
                                        //if (testOK && meas_time > 1) meas_time = 1;
                                    }
                                }
                                while (meas_time > 0);
                            }
                        }
                    }

                    ////////////////////////////////
                    //testOK = true;

                    dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = testOK ? Color.Lime : Color.Red;
                    dgvTestList.Rows[test_steep].Cells["Result"].Value = testOK ? "OK" : "NG";
                    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");
                    ////relayOK = true;
                    ////testOK = true;
                    //Tính toán bước tiếp theo:
                    if (relayOK && (btnMode3.BackColor != Color.DimGray || ((btnMode2.BackColor != Color.DimGray) && testOK)))
                    {
                        ++test_steep;
                        if (test_steep >= dgvTestList.Rows.Count) //Hết bước kiểm
                        {
                            //Cắt hết rơ-le:
                            relayBoards1.SetAllState(false);
                            //Tắt nguồn:
                            SetPower(false);
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

                            //if (chkAutoSave.Checked)
                            //{
                            //    btnSave_Click(null, null);
                            //}
                            //else
                            //{
                            //    MessageBox.Show("Đã hoàn thành bước kiểm cuối cùng");
                            //}
                            btnSave_Click1(null, null);
                            //Thoát chương trình kiểm:
                            thread_started = false;
                        }
                    }
                    else //Chế độ chạy thủ công hoặc bước kiểm không đạt
                    {
                        if (!testOK) MessageBox.Show("Bước kiểm không đạt");
                        thread_started = false; //Thoát chương trình kiểm
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            powerBoard1.SetOutput(1, false);
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

    private void btnEdit_Click(object sender, EventArgs e)
    {
        frmTestListEdit frm = new frmTestListEdit();
        frm.ShowForm(Global.BOARD_NAMES[BoardType]);
        btnReload_Click(null, null);
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
        EnterTestSteep(Conv.atoi32(txtTestSteep.Text) - 1);
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        int steep = Conv.atoi32(txtTestSteep.Text);
        if (steep >= dgvTestList.Rows.Count) steep = dgvTestList.Rows.Count - 1;
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
        btnEdit.Enabled = false;
        btnConfig.Enabled = false;

        picTesting.Visible = false;
        btnSave.Enabled = false;

        switch (_status)
        {
            case STATUS.Ready:
                SetPower(false); //Tắt nguồn
                relayBoards1.SetAllState(false); //Tắt rơ-le

                btnStop.Text = "STOP";
                btnReload.Enabled = btnEdit.Enabled = btnConfig.Enabled = true;
                cboOperator.Enabled = btnReady.Enabled = true;
                txtSerialNo.Enabled = true;
                txtSerialNo.Text = "";
                txtSerialNo.Focus();

                CheckStepLastFwSTM = false;
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

    string STR_Num = "1234567890";
    private void cboOperator_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!Char.IsControl(e.KeyChar))
        {
            if (!STR_Num.Contains(e.KeyChar)) e.Handled = true;
        }
    }
}
