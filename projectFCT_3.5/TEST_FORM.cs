using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace projectRU_New
{
    public partial class TEST_FORM : Form
    {

        DCL6104 DCL6104 = new DCL6104("COM3");
        public TEST_FORM()
        {
            InitializeComponent();
            //Register_Events();

        }

        private void Register_Events()
        {
            DCL6104.port.DataReceived += Port_DataReceived;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread GetData_thread = new Thread(EventHandling);
            GetData_thread.Start();
        }
        
   
        private void EventHandling()
        {
            Global.databuff += DCL6104.port.ReadExisting();

        }

       
        
        private void button1_Click(object sender, EventArgs e)
        {
            DCL6104.MODE("CV");
            txt1.Clear();
            txt1.Text = DCL6104.MODE_is();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DCL6104.MODE("CC");
            txt1.Clear();
            txt1.Text = DCL6104.MODE_is();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double val = double.Parse(txtValue.Text);
            DCL6104.Set_Res(val);
            txt1.Clear();
            txt1.Text = DCL6104.Read_Res().ToString();
        }
        int cnt = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            cnt++;
            if(cnt%2!=0)
                DCL6104.ON_LOAD();
            else
                DCL6104.OFF_LOAD();
        }
    }
}
