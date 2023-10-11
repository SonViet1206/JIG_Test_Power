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

    string BOARD_NAME = "BACK_BOARD";

    public frmMain()
    {
        InitializeComponent();


    }

    private void frmMain_Load(object sender, EventArgs e)
    {
        AddLogWindow("Khởi động chương trình");
        
        //numProductHeight.Value = Conv.atoi32(ini.IniReadValue("COMMON", "ProductHeight"), 500);
        //numTestSpeed.Value = Conv.atoi32(ini.IniReadValue("COMMON", "TestSpeed"), (int)numTestSpeed.Minimum, (int)numTestSpeed.Maximum);
        //numNormalSpeed.Value = Conv.atoi32(ini.IniReadValue("COMMON", "NormalSpeed"), (int)numNormalSpeed.Minimum, (int)numNormalSpeed.Maximum);

        lblBuildInfo.Text = "Ver:" + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
        cboOperator.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Operator'"));
        numTemprature.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Temperature'"));
    }

    private void frmMain_Shown(object sender, EventArgs e)
    {
        IniFile ini = new IniFile(Application.StartupPath + "\\AppConfig.ini");
        lblPowerBoard.Text = ini.IniReadValue("COMMUNICATION", "PowerBoardCommPort", "COM1");
        lblRelaysBoard.Text = ini.IniReadValue("COMMUNICATION", "RelaysBoardCommPort", "COM2");

        btnReload_Click(null, null);

        DMM.Start();

        powerBoard1.Start(lblPowerBoard.Text); 
        
        relayBoards1.Start(lblRelaysBoard.Text, "4,1,2"); //Sử dụng port lớn nhất
        
        relayBoards1.SetDOCaption(1, "TP4_GND");
        relayBoards1.SetDOCaption(2, "TP1_24V_IN");
        relayBoards1.SetDOCaption(3, "TP3_5V");
        relayBoards1.SetDOCaption(4, "TP11_24V_SUPPLY");
        relayBoards1.SetDOCaption(5, "TP10_24V_IN_HV");
        relayBoards1.SetDOCaption(6, "TP8_24V_IN_DC1");
        relayBoards1.SetDOCaption(7, "TP9_24V_IN_DC2");
        relayBoards1.SetDOCaption(8, "TP25_24V_LED");

        relayBoards1.SetDOCaption(9, "TP12_3V3_RELAY1");
        relayBoards1.SetDOCaption(10, "TP13_3V3_RELAY2");
        relayBoards1.SetDOCaption(11, "TP21_3V3_LED1");
        relayBoards1.SetDOCaption(12, "TP22_3V3_LED2");
        relayBoards1.SetDOCaption(13, "TP23_3V3_LED3");
        relayBoards1.SetDOCaption(14, "TP24_3V3_LED4");

        relayBoards1.SetDOCaption(25, "TP19_24V_OUT_RELAY1");
        relayBoards1.SetDOCaption(26, "TP20_24V_OUT_RELAY2");
        relayBoards1.SetDOCaption(27, "TP15_24V_OUT_LED1");
        relayBoards1.SetDOCaption(28, "TP16_24V_OUT_LED2");
        relayBoards1.SetDOCaption(29, "TP17_24V_OUT_LED3");
        relayBoards1.SetDOCaption(30, "TP18_24V_OUT_LED4");

        autonicsMT41.Start("COM3"); //Sử dụng port lớn nhất
        
        timer1s.Enabled = true;
        txtSerialNo.Focus();
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
        SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + cboOperator.Text + "' WHERE ItemName='Operator'");
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
        lblTime.Text = DateTime.Now.ToString("HH:mm:ss");

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
            EnterTestSteep(0);
        }
        else
        {
            MessageBox.Show("Chưa nhập số chế tạo");
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        btnStop.Enabled = false; 
        if (thread_started)
        {
            thread_started = false;
        }
        else
        {
            panSettings.Enabled = true;
            btnReload.Enabled = true;
            btnEdit.Enabled = true;
        }
        AddLogWindow("Kết thúc kiểm, kết quả: " + ledResult.Text);
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
        this.Close();
    }

    private void btnConfig_Click(object sender, EventArgs e)
    {
        frmConfig frm = new frmConfig();
        frm.ShowDialog();
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
            path_name += @"\RU_TEST_" + txtSerialNo.Text + "_" + Conv.atodt(dgvTestList.Rows[0].Cells["BeginTest"].Value).ToString("yyyyMMddHHmmss") + ".csv";
            //Lan dau tao file thi tao luon header line:
            if (File.Exists(path_name))
            {
                try 
                {
                    File.Delete(path_name);
                }
                catch {}
            }
            sBuff = "GREEN STATION R-U TEST REPORT\r\n";
            sBuff += "Operator," + cboOperator.Text + "\r\n";
            sBuff += "Equipment,ADCMT-7351E\r\n";
            sBuff += "Location,Synopex Vina 2\r\n";
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
                string PValue = Conv.atod(dgvTestList.Rows[r].Cells["PValue"].Value).ToString("0.00");
                string Unit = Conv.atos(dgvTestList.Rows[r].Cells["Unit"].Value);
                string BeginTest = Conv.atos(dgvTestList.Rows[r].Cells["BeginTest"].Value);
                string ElapseTime = Conv.atos(dgvTestList.Rows[r].Cells["ElapseTime"].Value);
                string Result = Conv.atos(dgvTestList.Rows[r].Cells["Result"].Value);

                sBuff += TestOrder + "," + TestName + "," + MinValue + "," + MaxValue + "," + PValue + "," + Unit + "," + BeginTest + "," + ElapseTime + "," + Result + "\r\n";
            }

            File.WriteAllText(path_name, sBuff, Encoding.UTF8);
            //File.WriteAllText(path_name, sBuff, new UTF8Encoding(true));
            AddLogWindow("Ghi log file thành công!");
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

    private void btnReload_Click(object sender, EventArgs e)
    {
        string strSQL = "SELECT * FROM tbl_test_list WHERE jig_name='" + BOARD_NAME + "' AND test_enable = 1 ORDER BY test_order";
        DataTable dtab = SQLite.ExecuteDataTable(strSQL);
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
        DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
        if (!Directory.Exists(DataLogPath)) DataLogPath = Application.StartupPath + @"\DataLogs";

    }

    private bool SetPower(bool state)
    {
        return powerBoard1.SetOutput(4, state);
    }

    private void EnterTestSteep(int test_steep)
    {
        if (test_steep > -1 && test_steep < dgvTestList.Rows.Count)
        {
            //while (test_steep < dgvTestList.Rows.Count)
            try
            {
                
                panSettings.Enabled = false;
                btnReload.Enabled = false;
                btnEdit.Enabled = false;
                btnStop.Enabled = true;
                

                btnNext.Enabled = false;
                btnTest.Enabled = false;

                thread_ended = false;
                thread_started = true;

                picTesting.Visible = true;
                while (thread_started)
                {
                    bool testOK = true;
                    bool relayOK = true;
                    string F = Conv.atos(dgvTestList.Rows[test_steep].Cells["DMMFunction"].Value);
                    string R = Conv.atos(dgvTestList.Rows[test_steep].Cells["DMMRange"].Value);
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
                    //Nếu bước này không có yêu cầu nguồn thì tắt
                    if (!powerRequest) SetPower(false);

                    dgvTestList.CurrentCell = dgvTestList.Rows[test_steep].Cells[0];

                    dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = Color.LightBlue;
                    dgvTestList.Rows[test_steep].Cells["BeginTest"].Value = BeginTest.ToString("yyyy-MM-dd HH:mm:ss");

                    txtTestSteep.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestOrder"].Value);
                    txtTestName.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["TestName"].Value);
                    
                    ledResult.Text = "---";
                    ledResult.BackColor = Global.COLOR_OFF;

                    btnTest.Enabled = false;
                    btnSave.Enabled = false;

                    AddLogWindow("Bước kiểm: " + txtTestSteep.Text + " - " + txtTestName.Text);
                    //Có yêu cầu thông báo cho người dùng:
                    if (Conv.atoi32(dgvTestList.Rows[test_steep].Cells["PopupCheck"].Value) == 1)
                    {
                        frmShowImage frm = new frmShowImage();
                        frm.lblTestSteep.Text = txtTestSteep.Text;
                        frm.lblTestName.Text = txtTestName.Text;
                        frm.txtMessage.Text = Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupMessage"].Value);
                        //frm.picHelp.Image = Image.FromFile(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                        //frm.ShowDialog();
                        frm.ShowForm(Conv.atos(dgvTestList.Rows[test_steep].Cells["PopupImage"].Value));
                    }

                    //Xử lý tắt rơ le:
                    if (relayOK)
                    {
                        //Tắt các rơ-le của bước trước:
                        relayOK = relayBoards1.SetAllState(false);
                        if (!relayOK) AddLogWindow("Lỗi tắt rơ-le 1", true);
                    }
                    testOK = relayOK;
                    //Có yêu cầu thay đổi thang đo:
                    if (F != "")
                    {
                        if (testOK)
                        {
                            testOK = DMM.SetFunction(F);
                            if (testOK)
                            {
                                if (R != "")
                                {
                                    testOK = DMM.SetRange(R);
                                    if (!testOK) AddLogWindow("Lỗi đặt thang đo cho DMM", true); 
                                }
                            }
                            else
                            {
                                AddLogWindow("Lỗi đặt chế độ đo cho DMM", true);
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
                                    relayOK = relayBoards1.SetOutput(Conv.atoi32(Relays[i]), true);
                                    if (!relayOK)
                                    {
                                        AddLogWindow("Lỗi bật rơ-le", true);
                                    }
                                }
                            }
                        }
                        //Nếu có yêu cầu nguồn thì bật:
                        if (powerRequest) SetPower(true);
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
                                    if (check_unit.IndexOf("Ω") > -1) read_value = Math.Abs(read_value);
                                    testOK = (read_value >= check_min && read_value <= check_max);
                                    //Nếu OK chờ thêm 200ms nưữa đọc kết quả lần cuối
                                    if (testOK && meas_time > 1) meas_time = 1;
                                }
                            }
                            while (meas_time > 0);
                        }
                    }

                    dgvTestList.Rows[test_steep].DefaultCellStyle.BackColor = testOK ? Color.Lime : Color.Red;
                    dgvTestList.Rows[test_steep].Cells["Result"].Value = testOK ? "OK" : "NG";
                    dgvTestList.Rows[test_steep].Cells["ElapseTime"].Value = ((DateTime.Now - BeginTest).TotalMilliseconds / 1000).ToString("0.0");

                    btnSave.Enabled = true;

                    //Tính toán bước tiếp theo:
                    if (relayOK && (btnMode3.BackColor != Color.DimGray || ((btnMode2.BackColor != Color.DimGray) && testOK)))
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

                            if (chkAutoSave.Checked)
                                btnSave_Click(null, null);
                            else
                                MessageBox.Show("Đã hoàn thành bước kiểm cuối cùng");
                            //Tắt nguồn:
                            SetPower(false);
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

            if (btnStop.Enabled)
            {
                btnNext.Enabled = true;
                btnTest.Enabled = true;
                btnTest.Focus();
            }
            else
            {
                panSettings.Enabled = true;
                btnReload.Enabled = true;
                btnEdit.Enabled = true;
            }
            picTesting.Visible = false;
            
            thread_ended = true;
        }
    } 

    private void dgvTestList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (!panSettings.Enabled && thread_ended) EnterTestSteep(e.RowIndex);
    }

    private void btnHome_Click(object sender, EventArgs e)
    {
        panelLeft.Visible = !panelLeft.Visible;
        powerBoard1.Visible = panelLeft.Visible;
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
        frm.ShowForm(BOARD_NAME); 
        btnReload_Click(null, null);
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
        EnterTestSteep(Conv.atoi32(txtTestSteep.Text) - 1);
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        int steep = Conv.atoi32(txtTestSteep.Text);
        if (steep >= dgvTestList.Rows.Count)
            MessageBox.Show("Đã hết chu trình kiểm tra, lưu dữ liệu hoặc thực hiện chu trình mới.");
        else
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
}
