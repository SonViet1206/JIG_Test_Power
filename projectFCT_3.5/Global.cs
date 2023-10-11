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
    public static string PctoolVersion = "1.0.0";
    public static bool checkHand=true;
    public static Color COLOR_OFF = Color.DarkSlateGray;
    public static Color COLOR_OK = Color.Lime;
    public static Color COLOR_NG = Color.Red;

    public static int StatesInputPowerboard = 0;
    public static string databuff ="";

    static public string[] BOARD_NAMES = new string[] {
        "POWER_BOARD",   //0
        "LED_BOARD",   //1
    };
    static public string[] TITLES = new string[] {
        "POWER BOARD R_U TEST JIG",
        "LED BOARD R_U TEST JIG",
    };
    static public string[] RELAY_BOARDS = new string[] {
        "4,4,4,2,4|DO1:|DO2:|DO3:|DO4:|DO5:|DO6:|DO7:|DO8:|DO9:|DO10:|DO11:|DO12:|DO13:|DO14:|DO15:|DO16:|DO17:|DO18:|DO19:|DO20:|DO21:|DO22:|DO23:|DO24:|DO25:|DO26:|DO27:|DO28:|DO29:|DO30:|DO31:|DO32:" +
        "|DO33:|DO34:|DO35:|DO36:|DO37:|DO38:|DO39:|DO40:|DO41:|DO42:|DO43:|DO44:|DO45:|DO46:|DO47:|DO48:|DO49:|DO50:|DO51:|DO52:|DO53:|DO54:|DO55:|DO56:|DO57:|DO58:|DO59:|DO60:|DO61:|DO62:|DO63:|DO64:" +
        "|DO65:|DO66:|DO67:|DO68:|DO69:|DO70:|DO71:|DO72:|DO73:|DO74:|DO75:|DO76:|DO77:|DO78:|DO79:|DO80:|DO81:|DO82:|DO83:|DO84:|DO85:|DO86:|DO87:|DO88:|DO89:|DO90:|DO91:|DO92:|DO93:|DO94:|DO95:|DO96:" +
        "|DO97:|DO98:|DO99:|DO100:|DO101:|DO102:|DO103:|DO104:|DO105:|DO106:|DO107:|DO108:|DO109:|DO110:|DO111:|DO112:" +
        "|DO113:|DO114:|DO115:|DO116:|DO117:|DO118:|DO119:|DO120:|DO121:|DO122:|DO123:|DO124:|DO125:|DO126:|DO127:|DO128:|DO129:|DO130:|DO131:|DO132:|DO133:|DO134:|DO135:|DO136:|DO137:|DO138:|DO139:|DO140:|DO141:|DO142:|DO143:|DO144:",

    };//
    //static public string[] RELAY_BOARDS = new string[] {
    //    "2|DO1:|DO2:|DO3:|DO4:|DO5:|DO6:|DO7:|DO8:|DO9:|DO10:|DO11:|DO12:|DO13:|DO14:|DO15:|DO16:" ,
    //    //"|DO33:|DO34:|DO35:|DO36:|DO37:|DO38:|DO39:|DO40:|DO41:|DO42:|DO43:|DO44:|DO45:|DO46:|DO47:|DO48:|DO49:|DO50:|DO51:|DO52:|DO53:|DO54:|DO55:|DO56:|DO57:|DO58:|DO59:|DO60:|DO61:|DO62:|DO63:|DO64:" +
    //    //"|DO65:|DO66:|DO67:|DO68:|DO69:|DO70:|DO71:|DO72:|DO73:|DO74:|DO75:|DO76:|DO77:|DO78:|DO79:|DO80:|DO81:|DO82:|DO83:|DO84:|DO85:|DO86:|DO87:|DO88:|DO89:|DO90:|DO91:|DO92:|DO93:|DO94:|DO95:|DO96:" ,
    //    //"|DO97:|DO98:|DO99:|DO100:|DO101:|DO102:|DO103:|DO104:|DO105:|DO106:|DO107:|DO108:|DO109:|DO110:|DO111:|DO112:" +
    //    //"|DO113:|DO114:|DO115:|DO116:|DO117:|DO118:|DO119:|DO120:|DO121:|DO122:|DO123:|DO124:|DO125:|DO126:|DO127:|DO128:|DO129:|DO130:|DO131:|DO132:|DO133:|DO134:|DO135:|DO136:|DO137:|DO138:|DO139:|DO140:|DO141:|DO142:|DO143:|DO144:",

    //};//
    static public double delta = 0; 

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
                string createText = "SUNHOUSE STATION EVENT LOG\r\n";
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
