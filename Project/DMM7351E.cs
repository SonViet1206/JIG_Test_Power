using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DefaultNS
{
    public partial class DMM7351E : UserControl
    {
        const uint MYID = 1;        //Target USBID
        const uint TMOUT = 3;       //Timeout (sec)
        uint hDev = 0;

        bool thread_started = false;
        bool thread_ended = true;
        Thread threadCOMM;
        
        bool thread_busy = false;
        bool command_busy = false;
        bool comm_started = false;

        double _value = 0;
        
        public DMM7351E()
        {
            InitializeComponent();
        }

        private void DMM7351E_Load(object sender, EventArgs e)
        {
            //this.Width = pictureBox3.Width;
            //this.Height = pictureBox3.Height;
            this.Font = new Font("Tahoma", 12);
        }

        public bool SetFunction(string fun)
        {
            if (lastFUN.Text != fun)
            {
                string ret = ExecuteDMMCommand(fun, 1000);
                if (ret != "")
                {
                    lastFUN.Text = fun;
                    SetRange("R0");
                    return true;
                }
            }
            else return true;
            return false;
        }

        public bool SetRange(string range)
        {
            if (lastRAN.Text != range)
            {
                string ret = ExecuteDMMCommand(range, 1000);
                if (ret != "")
                {
                    lastRAN.Text = range;
                    return true;
                }
            }
            else return true;
            return false;
        }

        public string Unit
        {
            get { return lblUnit.Text; }
            //set { ledValue.Value = value.ToString(); }
        }

        public string Header
        {
            get { return lblSubHeader.Text; }
            //set { ledValue.Value = value.ToString(); }
        }

        public double Value
        {
            get { return _value; }
            //set { ledValue.Value = value.ToString(); }
        }

        public string TextValue
        {
            get { return ledValue.Value; }
            set { ledValue.Value = value.ToString(); }
        }

        public void Start()
        {
            threadCOMM = new Thread(() => COMMProcess());
            threadCOMM.Start();
        }

        public void Stop()
        {
            thread_started = false;
            while (!thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (!thread_started)
                Start();
            else
                Stop();
        }

        void COMMProcess()
        {
            string RcvDt = "";
            uint RcvLen = 0;
            
            string led_value = "---------";
            string fun = "";
            string sub = "";
            string unit = "";

            string sBuff = "";
            try
            {
                thread_ended = false;
                thread_started = true;
                this.Invoke((MethodInvoker)delegate
                {
                    btnOnOff.BackColor = Color.Red;
                });
                
                while (thread_started)
                {
                    RcvDt = "";
                    RcvLen = 0;

                    led_value = "---------";
                    fun = "";
                    sub = "";
                    unit = "";

                    while (command_busy) Thread.Sleep(1); //Chờ cho lệnh đọc kết thúc
                    thread_busy = true;
                    this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                    try
                    {
                        if (!comm_started)
                        {
                            try 
                            { 
                                ausb.close(hDev);
                                ausb.end();
                                Thread.Sleep(10);
                            } catch {}
                            if (ausb.start(TMOUT) == 0) comm_started = (ausb.open(ref hDev, MYID) == 0);
                            Thread.Sleep(10);
                            if (comm_started)
                            {
                                comm_started = (ausb.write(hDev, "*RST") == 0);
                                if (comm_started)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        lastFUN.Text = ""; // this.Function;
                                        lastRAN.Text = ""; //this.Range;
                                    });

                                    Conv.delay_ms(1000);
                                    comm_started = (ausb.read(hDev, ref RcvDt, ref RcvLen) == 0);
                                }
                            }
                        }
                        else
                        {
                            comm_started = (ausb.read(hDev, ref RcvDt, ref RcvLen) == 0);
                            //DCV_ -042.429E-03
                            //DCV_ +000.001E-03
                            //ACV_ +000.075E-03
                            //DCI_ -000.003E-03
                            //ACI_ +000.053E-03
                            //R2W_ +120.216E+06
                            //R2W_ +1626.63E+03
                            //R2W_ +000.036E+00
                            //R2WO +9.99999E+37
                            //FRQ_ +2.34395E+03
                            sBuff = RcvDt.Replace(" ", "");
                            if (sBuff.Length == 16)
                            {
                                fun = sBuff.Substring(0, 3);
                                sub = sBuff.Substring(3, 1);
                                sBuff = sBuff.Substring(4);
                            }
                            if (sBuff.Length == 12)
                            {
                                led_value = sBuff.Substring(0, 8);
                                sBuff = sBuff.Substring(9);
                                _value = Conv.atod(led_value) * Math.Pow(10, Conv.atod(sBuff));


                                if (sub == "O") led_value = "0.L  ";
                                
                                if (sBuff == "-06")
                                    unit = "u";
                                else if (sBuff == "-03")
                                    unit = "m";
                                else if (sBuff == "+03")
                                    unit = "k";
                                else if (sBuff == "+06")
                                    unit = "M";
                                
                                if (fun == "DCV" || fun == "ACV" || fun == "ADV")
                                    unit += "V";
                                else if (fun == "DCI" || fun == "ACI" || fun == "ADI")
                                    unit += "A";
                                else if (fun == "R2W" || fun == "R2L")
                                    unit += "Ω";
                                else if (fun == "FRQ")
                                    unit += "Hz";

                            }
                        }
                    }
                    catch {}
                    thread_busy = false;
                    //Cap nhat hien thi:
                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;
                        lblFunction.Text = fun;
                        lblSubHeader.Text = sub;
                        lblUnit.Text = unit;
                        ledValue.Value = led_value;
                    });
                    //Di ngu:
                    if (comm_started) 
                        Thread.Sleep(200);
                    else
                        Thread.Sleep(3000);
                }

            }
            catch (Exception)
            {


            }

            try
            {
                ausb.close(hDev);
                ausb.end();
            }
            catch { }

            this.Invoke((MethodInvoker)delegate
            {
                btnOnOff.BackColor = Color.Black;
                ledValue.Value = "---------";
                lblFunction.Text = "";
                lblSubHeader.Text = "";
                lblUnit.Text = "";
            });
            thread_ended = true;
        }

        private void cboCMDs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(null, null);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            bool found = false;
            string sBuff;

            btnSend.Enabled = false;
            lblREC.Text = "";
            
            sBuff = cboCMDs.Text.Trim().ToUpper();
            for (int i = 0; i < cboCMDs.Items.Count; i++)
            {
                if (sBuff == cboCMDs.Items[i].ToString())
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                cboCMDs.Items.Add(sBuff);
            }
            cboCMDs.Text = sBuff;
            
            lblREC.Text = ExecuteDMMCommand(cboCMDs.Text, 1000);
            btnSend.Enabled = true;
        }

        private string ExecuteDMMCommand(string cmd, int ms_timeout)
        {
            string RcvDt = "";
            uint RcvLen = 50;

            //Conv.delay_ms(100);
            while (thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
            command_busy = true;
            try
            {
                if (comm_started)
                {
                    //if (cmd != "")
                    //{
                    //    comm_started = (ausb.write(hDev, cmd) == 0);
                    //    if (comm_started) Conv.delay_ms(ms_timeout);
                    //}
                    //if (comm_started) comm_started = (ausb.read(hDev, ref RcvDt, ref RcvLen) == 0);

                    if (cmd != "")
                    {
                        comm_started = (ausb.write(hDev, cmd) == 0);
                        if (comm_started)
                        {
                            do
                            {
                                //Conv.delay_ms(ms_timeout);
                                Thread.Sleep(1);
                                Application.DoEvents();
                                --ms_timeout;
                                comm_started = (ausb.read(hDev, ref RcvDt, ref RcvLen) == 0);
                                if (comm_started && RcvLen > 0)
                                {
                                    ms_timeout = 0;
                                }
                            }
                            while (ms_timeout > 0);
                            
                        }
                    }
                    else
                    {
                        comm_started = (ausb.read(hDev, ref RcvDt, ref RcvLen) == 0);
                    }
                }
            }
            catch {}
            command_busy = false;

            return RcvDt;
        }

        private void picDCV_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("F1", 1000);
            SetFunction("F1");
            //this.Range = "R0";
        }

        private void picACV_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("F2", 1000);
            SetFunction("F2");
        }

        private void picOMH_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("F3", 1000);
            SetFunction("F3");
        }

        private void picDCI_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("F5", 1000);
            SetFunction("F5");
        }

        private void picACI_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("F6", 1000);
            SetFunction("F6");
        }

        private void picFREQ_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("F50", 1000);
            SetFunction("F50");
        }

        private void picAUTO_Click(object sender, EventArgs e)
        {
            //ExecuteDMMCommand("R0", 1000);
            SetRange("R0");
        }

        private void picDOWN_Click(object sender, EventArgs e)
        {
            string R = ExecuteDMMCommand("R?", 1000);
            if (R.Length == 2)
            {
                int lev = Conv.atoi32(R.Substring(1));
                if (lev > 3)
                {
                    string ran = "R" + lev.ToString();
                    --lev;
                    //ExecuteDMMCommand(ran, 1000);
                    SetRange("ran");
                }
            }
        }

        private void picUP_Click(object sender, EventArgs e)
        {
            string F = ExecuteDMMCommand("F?", 1000);
            string R = ExecuteDMMCommand("R?", 1000);
            if (R.Length == 2)
            {
                int lev = Conv.atoi32(R.Substring(1));
                if (((F == "F01" || F == "F02" || F == "F07") && lev < 7) || ((F == "F05" || F == "F06" || F == "F08" || F == "F50") && lev < 8) || (F == "F03" && lev < 9))
                {
                    string ran = "R" + lev.ToString();
                    ++lev;
                    //ExecuteDMMCommand("R" + lev.ToString(), 1000);
                    SetRange("ran");
                }
            }
        }
    }
}
