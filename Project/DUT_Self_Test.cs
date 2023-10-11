using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO.Ports;
using ECAN;
using System.Linq;

namespace DefaultNS
{
    public partial class DUT_Self_Test : UserControl
    {
        const byte CAN_DUT_ADDRESS = 0x01;
        const byte CAN_SOFT_ADDRESS = 0x0A;

        const byte CAN_CMDID_START_TEST = 0x60;
        const byte CAN_CMDID_STOP_TEST = 0x61;
        const byte CAN_CMDID_REQUEST = 0x62;
        const byte CAN_CMDID_RESPONSE = 0x63;

        bool thread_busy = false;
        bool command_busy = false;
        bool comm_started = false;

        bool thread_started = false;
        bool thread_ended = true;

        bool tcp_thread_ended = true;
        bool tcp_thread_started = false; 
        Thread threadCOMM;
        Thread threadCAN;

        bool tcp_thread_busy = false;
        bool tcp_command_busy = false;
        bool tcp_comm_started = false;

        string _IP_address = "192.168.1.1";
        //SerialPort portCOMM = null;

        //string _port_name = "COM8";
        double _unit_factor = 1;
        SerialPort portCOMM = null;

        string can_serial = "---";

        TcpClient client = null;
        
        public DUT_Self_Test()
        {
            InitializeComponent();
        }

        private void Control_Load(object sender, EventArgs e)
        {
            this.Font = new Font("Tahoma", 12);
        }

        public bool CommState { get { return tcp_comm_started; } }

        double _pvalue = 0;
        public double PValue { get { return _pvalue; } }

        public void Start(string port_name)
        {
            //_port_name = port_name;
            //lblCaption.Text = "DEVICE UNDER TEST - " + port_name;
            
            textBox1.Text = GetLocalIP();
            
            
            //threadCOMM = new Thread(() => COMMProcess());
            //threadCOMM.Start();

            //threadCAN = new Thread(() => CANProcess());
            //threadCAN.Start();
        }

        public void Stop()
        {
            tcp_thread_started = false;
            while (!tcp_thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }

            thread_started = false;
            while (!thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (!tcp_thread_started && !thread_started)
                Start("");
            else
                Stop();
        }

        //private byte[] ExecuteCommand(string cmd_hex, int timeout)
        //{
        //    byte[] retBytes = null;
        //    while (tcp_thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
        //    tcp_command_busy = true;
        //    try
        //    {
        //        if (tcp_comm_started) //Kết nối với DUT tốt
        //        {

        //            this.Invoke((MethodInvoker)delegate
        //            {
        //                //if (txtCAN_LOG.Text.Length > 100000) txtCAN_LOG.Text = txtCAN_LOG.Text.Substring(txtCAN_LOG.Text.Length - 100000);

        //            });

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //    tcp_command_busy = false;

        //    return retBytes;
        //}

        private void btnPing_Click(object sender, EventArgs e)
        {
            Ping p1 = new Ping();
            PingReply PR;
            btnScan.Enabled = btnPing.Enabled = false;
            PR = p1.Send(txtIP.Text, 1000);
            MessageBox.Show(PR.Status.ToString());
            btnScan.Enabled = btnPing.Enabled = true;
        }

        
        private string ExcuteHtml(string url, int ms_timeout)
        {
            string html = "";
            try
            {
                DateTime cmd_starttime = DateTime.Now;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.ReadWriteTimeout = request.Timeout = ms_timeout;
                
                // Set some reasonable limits on resources used by this request
                //request.MaximumAutomaticRedirections = 4;
                //request.MaximumResponseHeadersLength = 4;
                // Set credentials to use for this request.
                //request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Console.WriteLine("Content length is {0}", response.ContentLength);
                //Console.WriteLine("Content type is {0}", response.ContentType);

                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format.
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                html = readStream.ReadToEnd();
                //txtTCP_LOG.AppendText("\r\n[Response][" + ((int)(DateTime.Now - cmd_starttime).TotalMilliseconds).ToString() + "ms]:" + html);
                response.Close();
                readStream.Close();
            }
            catch (Exception ex)
            {
                html = url + "\r\n" + ex.Message;
            }
            return html;
        }

        private string ExecuteTCPCommand(string cmd, string key_word, int ms_timeout)
        {
            string res;
            //txtTCP_LOG.AppendText("\r\n[Command][" + DateTime.Now.ToString("HH:mm:ss") + "]:" + cmd);
            //http://192.168.1.1:13150/api/v1.0/maintenance/start
            res = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/maintain/" + cmd, ms_timeout);
            //RX: {"current mode": "maintenance"}
            if (res.Length > 0)
            {
                res = res.Replace(": ", ":");
                res = res.Replace("{\"", "");
                res = res.Replace("\"}", "");
                string[] separatingStrings = { "\":\"" };
                string[] sItems = res.Split(separatingStrings, StringSplitOptions.None);
                if (sItems.Length > 1)
                {
                    if (sItems[0] == key_word)
                    {
                        return sItems[1];
                    }
                }    
            }
            return "";
        }

        private string ExecuteTCPCommand(string cmd, int ms_timeout)
        {
            string res;
            //txtTCP_LOG.AppendText("\r\n[Command][" + DateTime.Now.ToString("HH:mm:ss") + "]:" + cmd);
            res = ExcuteHtml("http://" + txtIP.Text + ":13150/api/v1.0/maintain/" + cmd, ms_timeout);

            if (res.Length > 0)
            {
                return res;
            }
            return "";
        }

        public bool TCP_StartTest()
        {
            string res = "";
            //if (ScanIP() != "")
            //{
            //    res = ExecuteTCPCommand("start", "current mode", 2000);
            //}



            if (ScanIP("10.128.128.71") != "")
            {
                Conv.delay_ms(1000);
                res = ExecuteTCPCommand("start", "current mode", 2000);
            }
            return (res == "maintenance");
        }

        public bool TCP_StopTest()
        {
            string res;
            res = ExecuteTCPCommand("stop", "current mode", 3000);
            return (res == "normal");
        }

        public string TCP_SendQuery()
        {
            string sBuff;
            //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            sBuff = ExecuteTCPCommand("query", "Test status", 2000);
            if (sBuff.Length > 0)
            {
                //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
            {
                //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            }
            return "";
        }

        public string TCP_GetDetail()
        {
            string sBuff;
            //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            sBuff = ExecuteTCPCommand("detail", 2000);
            if (sBuff.Length > 0)
            {
                //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
            {
                //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            }
            return "";
        }

        public string TCP_GetDiag()
        {
            string sBuff;
            //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = ""; });
            sBuff = ExecuteTCPCommand("diag", 2000);
            if (sBuff.Length > 0)
            {
                //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = sBuff; });
                return sBuff;
            }
            else
            {
                //this.Invoke((MethodInvoker)delegate { lblHardwareVersion.Text = "---"; });
            }
            return "";
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            string ip = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DUTStaticIP'"));
            btnPing.Enabled = btnScan.Enabled = false;
            //ScanIP();
            //ScanIP("10.150.92.88");
            ScanIP("ip"); //VF HP
            btnPing.Enabled = btnScan.Enabled = true;
        }

        private string GetLocalIP()
        {
            string retValue = "";
            try 
            {
                //IPAddress[] ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                foreach (var item in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    string ip = item.ToString();
                    if (ip.Length <= 15)
                        retValue += ip + " ";
                }
                retValue = retValue.Trim();
                retValue = retValue.Replace(" ", "|");
                //retValue = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString(); 
            }
            catch { }
            return retValue;
        }

        public string ScanIP()
        {
            DateTime cmd_starttime = DateTime.Now;
            
            txtIP.Text = "";
            Application.DoEvents();

            textBox1.Text = GetLocalIP();

            Thread t1 = new Thread(() => TestConnection("192.168.1", 1, 10, 13150, 3, 0, 200));
            Thread t2 = new Thread(() => TestConnection("192.168.1", 1, 10, 13150, 3, 1, 200));
            Thread t3 = new Thread(() => TestConnection("192.168.1", 1, 10, 13150, 3, 2, 200));

            t1.Start();
            t2.Start();
            t3.Start();
            //Chờ cho các luồng kết thúc:
            while (t1.IsAlive || t2.IsAlive || t3.IsAlive) Application.DoEvents();

            //txtTCP_LOG.AppendText("\r\nScan completed in [" + ((int)(DateTime.Now - cmd_starttime).TotalMilliseconds).ToString() + "ms]");
            return txtIP.Text;
        }

        public string ScanIP(string ip)
        {
            int s_count = 0;
            textBox1.Text = GetLocalIP();
            this.Invoke((MethodInvoker)delegate { txtIP.Text = ""; });
            while(++s_count < 5 && txtIP.Text == "")
            {
                if (txtIP.Text != "") break;
                TestConnection(ip, 13150, 500);
            }          
            return txtIP.Text;
        }

        protected void TestConnection(string ip_master, int ip_low, int ip_high, int port, int num_scan, int num_mod, int ms_timeout)
        {
            string ip = "";
            TcpClientWithTimeout t = new TcpClientWithTimeout();
            for (int i = ip_low; i <= ip_high; i++)
            {
                if (i % num_scan == num_mod)
                {
                    ip = ip_master + "." + i.ToString();
                    //this.Invoke((MethodInvoker)delegate { txtTCP_LOG.AppendText("\r\nScan IP:" + ip + " ... "); }); 
                    if (t.TestConnect(ip, port, ms_timeout))
                    {
                        this.Invoke((MethodInvoker)delegate { txtIP.Text = ip; });
                    }
                }
                if (txtIP.Text != "") break;
            }
        }

        protected void TestConnection(string ip_master, int port, int ms_timeout)
        {
            TcpClientWithTimeout t = new TcpClientWithTimeout();
            //this.Invoke((MethodInvoker)delegate { txtTCP_LOG.AppendText("\r\nScan IP:" + ip_master + " ... "); });
            if (t.TestConnect(ip_master, port, ms_timeout))
            {
                this.Invoke((MethodInvoker)delegate { txtIP.Text = ip_master; });
            }
        }
    }
}
