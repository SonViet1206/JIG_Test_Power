using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DefaultNS
{
    public partial class frmFlashFirmware : Form
    {
        SerialPort serialPort1;
        public bool state = false;
        string rxBuff = "";
        public frmFlashFirmware()
        {
            InitializeComponent();
        }

        public void ShowForm(string image_path)
        {
            txtPortName.Text = image_path;
            this.ShowDialog();
        }

        private void btnSendQR_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen == false)
                    serialPort1.Open();
                serialPort1.Write(txtSerialNo.Text);
                rxBuff = "";
                timerPollingCOMM.Interval = 500;
                timerPollingCOMM.Enabled = true;
                timerPollingCOMM.Start();

                lblMessage.Text = "Gửi số chế tạo thành công.\r\nCửa sổ này sẽ tự đóng lại khi có kết quả từ phần mềm nạp FIRMWARE.\r\n...";
            }
            catch (Exception ex)
            {
                lblMessage.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\nTruyền QR qua cổng '" + txtPortName.Text + "' không thành công.\r\n" + ex.Message;
            }
        }

        private void frmFlashFirmware_Load(object sender, EventArgs e)
        {

        }

        private void frmFlashFirmware_Shown(object sender, EventArgs e)
        {
            try
            {
                serialPort1 = new SerialPort();
                serialPort1.PortName = txtPortName.Text;
                serialPort1.BaudRate = 115200;
                serialPort1.Parity = Parity.None;
                serialPort1.DataBits = 8;
            }
            catch (Exception ex)
            {

            }
            btnSendQR_Click(null, null);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            state = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            state = false;
            this.Close();
        }

        private void timerPollingCOMM_Tick(object sender, EventArgs e)
        {
            if (rxBuff != "")
            {
                try { if (serialPort1.IsOpen) rxBuff += serialPort1.ReadExisting(); } catch { }
                string[] items = rxBuff.Split('|');
                if (items.Length > 3)
                {
                    if (items[0] == txtSerialNo.Text && items[2] == "OK")
                    {
                        btnOK_Click(null, null);
                    }    
                }    
                lblMessage.Text += "\r\nRX:" + rxBuff; 
                rxBuff = "";
            }
            else
            {
                if(serialPort1!=null)
                try { if (serialPort1.IsOpen) rxBuff += serialPort1.ReadExisting(); } catch { }
            }
        }

        private void frmFlashFirmware_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (serialPort1 != null)
                {
                    serialPort1.Close();
                    serialPort1 = null;
                }    
            }
            catch {}
        }
    }
}
