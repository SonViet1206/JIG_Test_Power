using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DefaultNS
{
    public partial class frmConfig : Form
    {
        public frmConfig()
        {
            InitializeComponent();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            string DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
            
            txtLogPath.Text = Directory.Exists(DataLogPath) ? DataLogPath : (Application.StartupPath + "\\DataLogs");
            cboBoardType.Items.Clear();
            cboBoardType.Items.AddRange(Global.BOARD_NAMES);
            cboBoardType.SelectedIndex = Conv.atoi32(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='BoardType'"));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string DataLogPath = Application.StartupPath + @"\DataLogs";
            if (Directory.Exists(txtLogPath.Text))
                DataLogPath = txtLogPath.Text;
            else
                MessageBox.Show("Thư mục '" + txtLogPath.Text + "' không tồn tại.\r\nThư mục mặc định được sử dụng.");
            
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + DataLogPath + "' WHERE ItemName='DataLogPath'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + cboBoardType.SelectedIndex.ToString() + "' WHERE ItemName='BoardType'");
            
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            txtLogPath.Text = Application.StartupPath + @"\DataLogs";
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Chọn thư mục này";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtLogPath.Text = Path.GetDirectoryName(folderBrowser.FileName);
            }
        }

        private void cboBoardType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
