using DefaultNS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DefaultNS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RestApiHelper.InitGlobalVarial();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Insert
            OutputUpdateFirmwareInfo res = RestApiHelper.UpdateFirmwareInfo("123123", "123", "123", "123", "123123");
            if (res != null)
            {
                if (res.Result == "OK")
                {
                    //AddLogWindow("Ghi MES thành công!");
                    //btnSave.Enabled = false;
                    //btnTest.Enabled = false;
                    //btnNext.Enabled = false;
                    //btnStop_Click(null, null);
                    //panSettings.Enabled = true;
                    //panTest.Enabled = false;

                    //btnReady.Enabled = true;
                    //txtSerialNo.Text = "";
                    //txtSerialNo.Focus();
                    MessageBox.Show("OK");
                }
                else
                {
                    //AddLogWindow("Ghi MES không thành công\r\nStatus: " + res.Confirm + "\r\nError Code: " + res.ErrorCode);
                    MessageBox.Show("Ghi MES không thành công\r\nSN: " + res.SN + "\r\nMachineName: " + res.MachineName + "\r\nResult: " + res.Result + "\r\nError Code: " + res.ErrorCode + "\r\nUserName: " + res.UserName + "\r\nFirmware: " + res.Firmware + "\r\nFirmwareVersion: " + res.FirmwareVersion, "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                //AddLogWindow("Ghi MES không thành công");
                MessageBox.Show("Ghi MES không thành công, hãy kiểm tra lại kết nối với MES và chạy test lại sản phẩm này!", "MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
