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
    public partial class frmConfigPLC : Form
    {
        public frmConfigPLC()
        {
            InitializeComponent();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            string DataLogPath = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DataLogPath'"));
            
            txtLogPath.Text = Directory.Exists(DataLogPath) ? DataLogPath : (Application.StartupPath + "\\DataLogs");
           
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string DataLogPath = Application.StartupPath + @"\DataLogs";
            if (Directory.Exists(txtLogPath.Text))
                DataLogPath = txtLogPath.Text;
            else
                MessageBox.Show("Thư mục '" + txtLogPath.Text + "' không tồn tại.\r\nThư mục mặc định được sử dụng.");
            
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + DataLogPath + "' WHERE ItemName='DataLogPath'");


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

        private void btnPassword_Click(object sender, EventArgs e)
        {
            string password = Global.PasswordInput();
            if (password.Length > 0)
            {
                if (password == Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='Password1'")))
                {
                    btnPassword.Enabled = false;
                    btnSavePassword.Enabled = true;
                    btnSave.Enabled = true;
                }
                else MessageBox.Show("Sai mật khẩu.", "LỖI THAO TÁC", MessageBoxButtons.OK);
            }
        }

        private void btnSavePassword_Click(object sender, EventArgs e)
        {
            if (txtNewPassword.Text != "" && txtNewPassword.Text == txtRepeatPassword.Text)
            {
                DialogResult dlr = MessageBox.Show("Đổi mật khẩu?", "XÁC NHẬN", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dlr == System.Windows.Forms.DialogResult.Yes)
                {
                    SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + txtNewPassword.Text + "' WHERE ItemName='Password1'");
                    txtNewPassword.Text = "";
                    txtRepeatPassword.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Mật khẩu không hợp lệ.");
            }
        }
    }
}
