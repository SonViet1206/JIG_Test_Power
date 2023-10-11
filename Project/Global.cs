using System;
using System.Collections.Generic;
//using System.Linq;
using System.Windows.Forms;
using DefaultNS;
using System.Threading;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

static class Global
{
    public static Color COLOR_OFF = Color.DarkSlateGray;
    public static Color COLOR_OK = Color.Lime;
    public static Color COLOR_NG = Color.Red;

    static public string[] BOARD_NAMES = new string[] {
        "MAIN_BOARD_3.5",   //0
        "MAIN_BOARD_7.4",   //1
    };
    static public string[] TITLES = new string[] {
        "MAIN BOARD 3.5KW R_U TEST JIG",
        "MAIN BOARD 7.4KW R_U TEST JIG",
    };
    static public string[] RELAY_BOARDS = new string[] {
        "2,1|DO1:TP57_GND|DO2:TP78_GND_L1|DO3:TP58_15V|DO4:TP15_3V3|DO5:TP13_5V_LOGIC|DO6:TP14_5V_RL|DO7:TP123_1V3_CORE|DO8:TP44_-12V_PILOT|DO9:TP16_5V_PHASE1|DO10:TP17_3V3_PHASE1|DO11:TP76_0V6_VREF|DO12:TP43_+12V_PILOT|DO17:J4.2_CLOCK|DO18:J4.1_DATA|DO19:J4.4_3V3|DO20:J4.3_GND|DO21:J4.5_RESET",
        "2,1|DO1:TP4_GND|DO2:TP78_GND_L1|DO3:TP3_12V_VDC|DO4:TP67_15V_VDC|DO5:TP66_3V3_5V_IR|DO6:TP133_1V3_CORE|DO7:TP15_3V3_VDC|DO8:TP32_5V_RL1|DO9:TP39_5V_RN|DO10:TP13_5V_VDC|DO11:TP108_5V_IN|DO12:TP14_5V_RELAY|DO13:TP44_-12V_OPAMP|DO14:SCREW4|DO15:TP17_3V3_L1|DO16:TP16_5V_L1|DO17:TP71_J4.1_DATA|DO18:TP72_J4.2_CLK|DO19:TP73_J4.2_GND|DO20:TP75_J4.2_RST|DO21:TP74_3V3_CON|DO22:TP76_0V6_REF_L1|DO23:TP77_IN_NA1",
    };//



    static public void ChangeDataGridViewJustify(ref DataGridView dgv)
    {
        if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.AllCells)
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
        else if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader)
        //    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
        //else if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.ColumnHeader)
        //    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        //else if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.DisplayedCells)
        //    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
        //else if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader)
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        //else if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill)
        //    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        else
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        //dgv.AutoResizeColumns(dgv.AutoSizeColumnsMode);
    }

    static public void LoadComboboxFromDatatable(object cbo_object, DataTable data_table)
    {
        if (cbo_object is ToolStripComboBox)
        {
            ToolStripComboBox cbo = (ToolStripComboBox)cbo_object;
            cbo.Items.Clear();
            if (data_table != null)
            {
                foreach (DataRow row in data_table.Rows)
                {
                    cbo.Items.Add(row[0].ToString());
                }
            }
        }
        else if (cbo_object is ComboBox)
        {
            ComboBox cbo = (ComboBox)cbo_object;
            cbo.Items.Clear();
            if (data_table != null)
            {
                foreach (DataRow row in data_table.Rows)
                {
                    cbo.Items.Add(row[0].ToString());
                }
            }
        }
        
    }

    static public void Delay_ms(int ms_count)
    {
        for (int i = 0; i < ms_count; i++)
        {
            Thread.Sleep(1);
            Application.DoEvents();
        }
    }

    public static void ReleaseObject(object obj)
    {
        try
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            obj = null;
        }
        catch
        {
            obj = null;
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public static bool DataGrid2Excel(object datagrid, string filename = "")
    {
        bool bCommOK = true;

        // Save to Excel File:
        SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        saveFileDialog1.Title = "Save as ...";
        saveFileDialog1.Filter = "Microsoft Excel File(*.xlsx, *.xls)|*.xlsx;*.xls";
        saveFileDialog1.CheckFileExists = false;
        saveFileDialog1.OverwritePrompt = true;
        saveFileDialog1.FileName = filename;
        DialogResult result = saveFileDialog1.ShowDialog();
        System.Windows.Forms.Application.DoEvents();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook xlWBook = xlApp.Workbooks.Add();
                Microsoft.Office.Interop.Excel.Worksheet xlSheet = xlWBook.Worksheets.Add();

                xlApp.Visible = false;

                if (datagrid is System.Data.DataTable)
                {
                    System.Data.DataTable dgid = (System.Data.DataTable)datagrid;
                    // insert column names
                    for (var i = 0; i <= dgid.Columns.Count - 1; i++)
                        xlSheet.Cells[1, i + 1].value = dgid.Columns[i].ColumnName.ToString();
                    // insert the actual data
                    for (var j = 0; j <= dgid.Rows.Count - 1; j += 1)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        xlSheet.Range["A" + (j + 2).ToString()].Select();
                        for (var i = 0; i <= dgid.Columns.Count - 1; i += 1)
                            xlSheet.Cells[j + 2, i + 1].value = dgid.Rows[j].ItemArray[i].ToString();
                    }
                }
                else if (datagrid is DataGridView)
                {
                    DataGridView dgview = (DataGridView)datagrid;
                    int col;
                    col = 0;
                    for (var i = 0; i <= dgview.Columns.Count - 1; i++)
                    {
                        if (dgview.Columns[i].Visible)
                        {
                            xlSheet.Cells[1, col + 1].value = dgview.Columns[i].HeaderText;
                            col += 1;
                        }
                    }
                    for (var j = 0; j <= dgview.Rows.Count - 1; j += 1)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        xlSheet.Range["A" + (j + 2).ToString()].Select();
                        col = 0;
                        for (var i = 0; i <= dgview.Columns.Count - 1; i += 1)
                        {
                            if (dgview.Columns[i].Visible)
                            {
                                try
                                {
                                    xlSheet.Cells[j + 2, col + 1].value = "'" + dgview.Rows[j].Cells[i].Value.ToString();
                                }
                                catch
                                {
                                }
                                col += 1;
                            }
                        }
                    }
                }

                // Format :
                {
                    var withBlock = xlSheet.Range["A1:IV1"];
                    withBlock.Font.Bold = true;
                    withBlock.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                    withBlock.VerticalAlignment = Microsoft.Office.Interop.Excel.Constants.xlBottom;
                    withBlock.WrapText = false;
                    withBlock.Orientation = 0;
                    withBlock.AddIndent = false;
                    withBlock.IndentLevel = 0;
                    withBlock.ShrinkToFit = false;
                    //withBlock.ReadingOrder = Excel.Constants.xlContext;
                    withBlock.MergeCells = false;
                }

                xlSheet.Cells.Rows.AutoFit();
                xlSheet.Cells.Columns.AutoFit();
                xlSheet.Range["A1"].Select(); // Chuyen cong tro ve dong du tien
                // Save to Excel File:
                xlWBook.SaveCopyAs(saveFileDialog1.FileName);
                xlWBook.Saved = true;
                xlApp.Quit();

                Global.ReleaseObject(xlSheet);
                Global.ReleaseObject(xlWBook);
                Global.ReleaseObject(xlApp);

                Process.Start(saveFileDialog1.FileName);
            }
            catch (Exception ex)
            {
                //WriteToLogFile("[DataGrid2Excel]-" + ex.Message);
                bCommOK = false;
            }
            Cursor.Current = Cursors.Default;
        }

        return bCommOK;
    }

    static public void WriteLogFile(string stext)
    {
        string MyPath = Application.StartupPath + @"\EventsLog\";
        try
        {
            string appendText = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff] - ") + stext + "\r\n";
            if (!Directory.Exists(MyPath)) Directory.CreateDirectory(MyPath);
            MyPath += DateTime.Now.ToString("yyyyMMdd") + "[" + Process.GetCurrentProcess().ProcessName + "].log";
            if (File.Exists(MyPath) == false)
            {
                // Create a file to write to.
                string createText = "GREEN STATION EVENT LOG\r\n";
                File.WriteAllText(MyPath, createText);
            }
            File.AppendAllText(MyPath, appendText);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    static public string[] GetPortNames()
    {
        string[] comms = SerialPort.GetPortNames();
        if (comms.Length > 0)
        {
            //Sắp sếp theo thứ tự tăng dần:
            for (int i = 0; i < comms.Length; i++)
            {
                int item_i = Conv.atoi32(comms[i].Substring(3));
                for (int j = i + 1; j < comms.Length; j++)
                {
                    int item_j = Conv.atoi32(comms[j].Substring(3));
                    if (item_j < item_i)
                    {
                        string sBuff = comms[i];
                        comms[i] = comms[j];
                        comms[j] = sBuff;
                        item_i = item_j;
                    }
                }
            }
        }
        return comms;
    }

    public static string PasswordInput(string default_value = "", bool show_pass = false, int _type = 1)
    {
        frmInputPassword frm = new frmInputPassword(default_value, show_pass, _type);
        frm.ShowDialog();
        return frm.txtInput.Text;
    }



}
