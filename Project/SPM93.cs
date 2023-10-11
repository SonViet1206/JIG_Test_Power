using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;

namespace DefaultNS
{
    public partial class SPM93 : UserControl
    {
        bool thread_ended = true;
        bool thread_started = false; 
        Thread threadCOMM;
        
        string _port_name = "COM3";
        
        bool comm_state = false;
        public SPM93()
        {
            InitializeComponent();
        }

        enum PARAMS
        {
            U_v,
            I_a,
            PF,
            P_w,
            Q_var,
            S_va,
            F_hz,
            Eact,
            Erac
        }

        private void SPM91_Load(object sender, EventArgs e)
        {
            //this.Width = pictureBox3.Width;
            //this.Height = pictureBox3.Height;
            this.Font = new Font("Tahoma", 12);

            string[] param_name = Enum.GetNames(typeof(PARAMS));
            dgvValue.Rows.Clear();
            for (int i = 0; i < param_name.Length; i++)
            {
                dgvValue.Rows.Add();
                dgvValue.Rows[i].Cells[0].Value = param_name[i];
                dgvValue.Rows[i].Cells[1].Value = "---";
                dgvValue.Rows[i].Cells[2].Value = "---";
                dgvValue.Rows[i].Cells[3].Value = "---";
                dgvValue.Rows[i].Cells[4].Value = "---";
            }

        }

        public bool CommState { get { return comm_state; } }

        public double Ua { get { return Conv.atod(dgvValue.Rows[(int)PARAMS.U_v].Cells["PhaseA"].Value); } }
        public double Ub { get { return Conv.atod(dgvValue.Rows[(int)PARAMS.U_v].Cells["PhaseB"].Value); } }
        public double Uc { get { return Conv.atod(dgvValue.Rows[(int)PARAMS.U_v].Cells["PhaseC"].Value); } }

        public double Ia { get { return Conv.atod(dgvValue.Rows[(int)PARAMS.I_a].Cells["PhaseA"].Value); } }
        public double Ib { get { return Conv.atod(dgvValue.Rows[(int)PARAMS.I_a].Cells["PhaseB"].Value); } }
        public double Ic { get { return Conv.atod(dgvValue.Rows[(int)PARAMS.I_a].Cells["PhaseC"].Value); } }

        public void Start(string port_name)
        {
            _port_name = port_name;
            lblCaption.Text = "SPM93 METER - " + port_name;
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
                Start(_port_name);
            else
                Stop();
        }

        void COMMProcess()
        {
            //[13:48:12+204][RX][0103 0000 000C 45CF][8]
            //[13:48:12+347][RX][0103 18 00000000 5BCF 00000000 00000000 0000000000000000 139A E38D][29]
            //byte[] CMD = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0C, 0x45, 0xCF };
            //000000-Tx:1F 03 00 00 00 21 86 6C
            //000005-Rx:1F 03 42 5A C6 5A C8 5A C3 00 00 00 00 00 00 00 00 00 00 00 00 13 8B 03 E8 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 1A 4B
            byte[] CMD = new byte[] { 0x1F, 0x03, 0x00, 0x00, 0x00, 0x21, 0x86, 0x6C };
            
            SerialPort portCOMM = new SerialPort(_port_name, 9600, Parity.None, 8, StopBits.One);
            bool commOK = false;
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
                    this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });
                    try
                    {
                        commOK = false;
                        if (!portCOMM.IsOpen)
                        {
                            try 
                            { 
                                portCOMM.Close();
                                portCOMM.Open();
                            } catch { }
                            this.Invoke((MethodInvoker)delegate
                            {
                                lblCaption.Text = (portCOMM.IsOpen ? "SPM93 METER - " + _port_name + ":OK" : "SPM93 METER - " + _port_name + ":NG");
                            });
                        }
                        else
                        {
                            int rx_len;
                            //Gửi lệnh đọc:
                            portCOMM.Write(CMD, 0, CMD.Length);
                            Thread.Sleep(150);
                            rx_len = portCOMM.BytesToRead;
                            if (rx_len > 0)
                            {
                                byte[] rxBuff = new byte[rx_len];
                                portCOMM.Read(rxBuff, 0, rx_len);
                                //     0       3        7    9        13       17               25   27    29
                                //[RX][0103 18 00000000 5BCF 00000000 00000000 0000000000000000 139A E38D][29]
                                //Rx:  1F 03 22 5B 7B 5B 7D 5B 78 00 00 00 00 00 00 00 00 00 00 00 00 13 8F 03 E8 00 00 00 00 00 00 00 00 00 00 00 00 BB 0A

                                if (rx_len == 71)
                                {
                                    if (rxBuff[0] == CMD[0] && rxBuff[1] == CMD[1] && rxBuff[2] + 5 == rx_len)
                                    {
                                        //int iTemp;
                                        this.Invoke((MethodInvoker)delegate {
                                            try
                                            {
                                                dgvValue.Rows[(int)PARAMS.U_v].Cells["PhaseA"].Value = ((rxBuff[3] * 0x100 + rxBuff[4]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.U_v].Cells["PhaseB"].Value = ((rxBuff[5] * 0x100 + rxBuff[6]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.U_v].Cells["PhaseC"].Value = ((rxBuff[7] * 0x100 + rxBuff[8]) * 0.01).ToString("0.00");

                                                dgvValue.Rows[(int)PARAMS.I_a].Cells["PhaseA"].Value = ((rxBuff[11] * 0x1000000 + rxBuff[12] * 0x10000 + rxBuff[9] * 0x100 + rxBuff[10]) * 0.001).ToString("0.000");
                                                dgvValue.Rows[(int)PARAMS.I_a].Cells["PhaseB"].Value = ((rxBuff[15] * 0x1000000 + rxBuff[16] * 0x10000 + rxBuff[13] * 0x100 + rxBuff[14]) * 0.001).ToString("0.000");
                                                dgvValue.Rows[(int)PARAMS.I_a].Cells["PhaseC"].Value = ((rxBuff[19] * 0x1000000 + rxBuff[20] * 0x10000 + rxBuff[17] * 0x100 + rxBuff[18]) * 0.001).ToString("0.000");

                                                dgvValue.Rows[(int)PARAMS.F_hz].Cells["Total"].Value = ((rxBuff[21] * 0x100 + rxBuff[22]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.PF].Cells["Total"].Value = ((rxBuff[23] * 0x100 + rxBuff[24]) * 0.001).ToString("0.000");

                                                dgvValue.Rows[(int)PARAMS.S_va].Cells["Total"].Value = ((rxBuff[27] * 0x1000000 + rxBuff[28] * 0x10000 + rxBuff[25] * 0x100 + rxBuff[26]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.Eact].Cells["Total"].Value = ((rxBuff[31] * 0x1000000 + rxBuff[32] * 0x10000 + rxBuff[29] * 0x100 + rxBuff[30]) * 0.1).ToString("0.0");
                                                dgvValue.Rows[(int)PARAMS.Erac].Cells["Total"].Value = ((rxBuff[35] * 0x1000000 + rxBuff[36] * 0x10000 + rxBuff[33] * 0x100 + rxBuff[34]) * 0.1).ToString("0.0");

                                                dgvValue.Rows[(int)PARAMS.P_w].Cells["PhaseA"].Value = ((rxBuff[39] * 0x1000000 + rxBuff[40] * 0x10000 + rxBuff[37] * 0x100 + rxBuff[38]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.P_w].Cells["PhaseB"].Value = ((rxBuff[43] * 0x1000000 + rxBuff[44] * 0x10000 + rxBuff[41] * 0x100 + rxBuff[42]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.P_w].Cells["PhaseC"].Value = ((rxBuff[47] * 0x1000000 + rxBuff[48] * 0x10000 + rxBuff[45] * 0x100 + rxBuff[46]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.P_w].Cells["Total"].Value = ((rxBuff[51] * 0x1000000 + rxBuff[52] * 0x10000 + rxBuff[49] * 0x100 + rxBuff[50]) * 0.01).ToString("0.00");

                                                dgvValue.Rows[(int)PARAMS.Q_var].Cells["PhaseA"].Value = ((rxBuff[55] * 0x1000000 + rxBuff[56] * 0x10000 + rxBuff[53] * 0x100 + rxBuff[54]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.Q_var].Cells["PhaseB"].Value = ((rxBuff[59] * 0x1000000 + rxBuff[60] * 0x10000 + rxBuff[57] * 0x100 + rxBuff[58]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.Q_var].Cells["PhaseC"].Value = ((rxBuff[63] * 0x1000000 + rxBuff[64] * 0x10000 + rxBuff[61] * 0x100 + rxBuff[62]) * 0.01).ToString("0.00");
                                                dgvValue.Rows[(int)PARAMS.Q_var].Cells["Total"].Value = ((rxBuff[67] * 0x1000000 + rxBuff[68] * 0x10000 + rxBuff[65] * 0x100 + rxBuff[66]) * 0.01).ToString("0.00");
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        });
                                        commOK = true;
                                    }
                                }
                            }
                            
                        }
                    }
                    catch {  }

                    comm_state = commOK;
                    //Cap nhat hien thi:
                    this.Invoke((MethodInvoker)delegate
                    {
                        btnOnOff.BackColor = Color.Black;
                        if (!commOK)
                        {
                            for (int i = 0; i < dgvValue.Rows.Count; i++)
                            {
                                dgvValue.Rows[i].Cells["PhaseA"].Value = "---";
                                dgvValue.Rows[i].Cells["PhaseB"].Value = "---";
                                dgvValue.Rows[i].Cells["PhaseC"].Value = "---";
                                dgvValue.Rows[i].Cells["Total"].Value = "---";
                            }
                        }
                        
                    });
                    //Di ngu:
                    if (commOK) 
                        Thread.Sleep(150);
                    else
                        Thread.Sleep(3000);
                }

            }
            catch (Exception)
            {


            }

            try { portCOMM.Close(); } catch { }
            
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    btnOnOff.BackColor = Color.Black;
                    for (int i = 0; i < dgvValue.Rows.Count; i++)
                    {
                        dgvValue.Rows[i].Cells["PhaseA"].Value = "---";
                        dgvValue.Rows[i].Cells["PhaseB"].Value = "---";
                        dgvValue.Rows[i].Cells["PhaseC"].Value = "---";
                        dgvValue.Rows[i].Cells["Total"].Value = "---";
                    }
                });
            }
            catch { }

            thread_ended = true;
        }
    }
}
