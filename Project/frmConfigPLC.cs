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
            //cboBoardType.SelectedIndex = Conv.atoi32(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='BoardType'"));
            numDeltaT.Value = (decimal)Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaT'"));
            numDeltaU.Value = (decimal)Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaU'"));
            numDeltaI.Value = (decimal)Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaI'"));
            numDeltaP.Value = (decimal)Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='DeltaP'"));
            numLoadPower.Value = (decimal)Conv.atod(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='LoadPower'"));
            txtSoftwareVersion.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='SoftwareVersion'"));
            txtHardwareVersion.Text = Conv.atos(SQLite.ExecuteScalar("SELECT ItemValue FROM tbl_common_settings WHERE ItemName='HardwareVersion'"));
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
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numDeltaT.Value.ToString() + "' WHERE ItemName='DeltaT'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numDeltaU.Value.ToString() + "' WHERE ItemName='DeltaU'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numDeltaI.Value.ToString() + "' WHERE ItemName='DeltaI'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numDeltaP.Value.ToString() + "' WHERE ItemName='DeltaP'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numDeltaP.Value.ToString() + "' WHERE ItemName='DeltaP'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + numLoadPower.Value.ToString() + "' WHERE ItemName='LoadPower'");

            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + txtHardwareVersion.Text + "' WHERE ItemName='HardwareVersion'");
            SQLite.ExecuteNonQuery("UPDATE tbl_common_settings SET ItemValue='" + txtSoftwareVersion.Text + "' WHERE ItemName='SoftwareVersion'");

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
