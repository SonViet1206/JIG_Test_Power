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
    public partial class frmTestListEdit : Form
    {
        string jig_name = "";
        public frmTestListEdit()
        {
            InitializeComponent();
        }

        private void UpdateStatus()
        {
            
        }

        private void frmTestListEdit_Load(object sender, EventArgs e)
        {

        }

        private void frmTestListEdit_Shown(object sender, EventArgs e)
        {
            btnReload_Click(null, null);
        }

        public void ShowForm(string board_name)
        {
            jig_name = board_name;
            this.ShowDialog();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            try
            {
                string strSQL = "SELECT * FROM tbl_test_list WHERE jig_name='" + jig_name + "' ORDER BY test_order";
                DataTable dtab = SQLite.ExecuteDataTable(strSQL);
                if (dtab != null)
                {
                    dgvTestList.AllowUserToAddRows = false;
                    dgvTestList.ReadOnly = true;
                    dgvTestList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    dgvTestList.Rows.Clear();
                    for (int r = 0; r < dtab.Rows.Count; r++)
                    {
                        dgvTestList.Rows.Add();

                        dgvTestList.Rows[r].Cells["pkid"].Value = dtab.Rows[r]["pkid"];
                        dgvTestList.Rows[r].Cells["TestOrder"].Value = dtab.Rows[r]["test_order"];
                        dgvTestList.Rows[r].Cells["TestName"].Value = dtab.Rows[r]["test_name"];
                        dgvTestList.Rows[r].Cells["PcbIndex"].Value = dtab.Rows[r]["pcb_index"];
                        dgvTestList.Rows[r].Cells["MinValue"].Value = dtab.Rows[r]["min_value"];
                        dgvTestList.Rows[r].Cells["MaxValue"].Value = dtab.Rows[r]["max_value"];
                        dgvTestList.Rows[r].Cells["Unit"].Value = dtab.Rows[r]["unit"];
                        dgvTestList.Rows[r].Cells["PopupCheck"].Value = dtab.Rows[r]["popup_check"];
                        dgvTestList.Rows[r].Cells["PopupMessage"].Value = dtab.Rows[r]["popup_message"];
                        dgvTestList.Rows[r].Cells["PopupImage"].Value = dtab.Rows[r]["popup_image"];

                        dgvTestList.Rows[r].Cells["DMMFunction"].Value = dtab.Rows[r]["dmm_function"];
                        dgvTestList.Rows[r].Cells["DMMRange"].Value = dtab.Rows[r]["dmm_range"];

                        dgvTestList.Rows[r].Cells["RelayOutputs"].Value = dtab.Rows[r]["relay_outputs"];
                        dgvTestList.Rows[r].Cells["StabMsTime"].Value = dtab.Rows[r]["stab_mstime"];
                        dgvTestList.Rows[r].Cells["MeasMsTime"].Value = dtab.Rows[r]["meas_mstime"];
                        dgvTestList.Rows[r].Cells["TestEnable"].Value = dtab.Rows[r]["test_enable"];
                    }
                }
            }
            catch (Exception ex)
            {

            }

            txtCount.Text = dgvTestList.Rows.Count.ToString();
            //dgvTestList.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            
            btnSave.Enabled = false;
            btnAdd.Enabled = !btnPassword.Enabled;
            btnEdit.Enabled = !btnPassword.Enabled;
            btnDelete.Enabled = !btnPassword.Enabled;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            dgvTestList.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvTestList.ReadOnly = false;
            dgvTestList.SelectionMode = DataGridViewSelectionMode.CellSelect;

            btnSave.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvTestList.ReadOnly = false;
            for (int i = 0; i < dgvTestList.Rows.Count; i++)
            {
                dgvTestList.Rows[i].ReadOnly = true;
            }
            dgvTestList.AllowUserToAddRows = true;
            dgvTestList.SelectionMode = DataGridViewSelectionMode.CellSelect;

            btnSave.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string strSQL, strSQL1, strSQL2;
            string TestName;
            Application.DoEvents();
            if (Conv.atoi32(txtCount.Text) < dgvTestList.Rows.Count)
            {
                //Thêm mới:
                for (int i = Conv.atoi32(txtCount.Text); i < dgvTestList.Rows.Count; i++)
                {
                    string min = Conv.atos(dgvTestList.Rows[i].Cells["MinValue"].Value);
                    string max = Conv.atos(dgvTestList.Rows[i].Cells["MaxValue"].Value);

                    TestName = Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value);
                    if (TestName != "")
                    {
                        strSQL1 = "INSERT INTO tbl_test_list (jig_name"; 
                        strSQL2 = " VALUES('" + jig_name + "'";
                        
                        strSQL1 += ", test_order"; 
                        strSQL2 += ",'" + Conv.atoi32(dgvTestList.Rows[i].Cells["TestOrder"].Value).ToString() + "'";

                        strSQL1 += ", pcb_index";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["PcbIndex"].Value) + "'";

                        strSQL1 += ", test_name";
                        strSQL2 += ",'" + TestName + "'";

                        if (min != "")
                        {
                            strSQL1 += ", min_value";
                            strSQL2 += ",'" + min + "'";
                        }
                        if (max != "")
                        {
                            strSQL1 += ", max_value";
                            strSQL2 += ",'" + max + "'";
                        }

                        strSQL1 += ", unit";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["Unit"].Value) + "'";

                        strSQL1 += ", dmm_function";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["DMMFunction"].Value) + "'";

                        strSQL1 += ", dmm_range";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["DMMRange"].Value) + "'";

                        strSQL1 += ", popup_check";
                        strSQL2 += "," + Conv.atoi32(dgvTestList.Rows[i].Cells["PopupCheck"].Value).ToString();

                        strSQL1 += ", popup_message";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["PopupMessage"].Value) + "'";

                        strSQL1 += ", popup_image";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["PopupImage"].Value) + "'";

                        strSQL1 += ", relay_outputs";
                        strSQL2 += ",'" + Conv.atos(dgvTestList.Rows[i].Cells["RelayOutputs"].Value) + "'";

                        strSQL1 += ", stab_mstime";
                        strSQL2 += "," + Conv.atoi32(dgvTestList.Rows[i].Cells["StabMsTime"].Value).ToString();

                        strSQL1 += ", meas_mstime";
                        strSQL2 += "," + Conv.atoi32(dgvTestList.Rows[i].Cells["MeasMsTime"].Value).ToString();

                        strSQL1 += ", test_enable";
                        strSQL2 += "," + Conv.atoi32(dgvTestList.Rows[i].Cells["TestEnable"].Value).ToString();

                        strSQL = strSQL1 + ")" + strSQL2 + ")";
                        SQLite.ExecuteNonQuery(strSQL);
                    }
                }
            }
            else
            {
                //Cập nhật:
                for (int i = 0; i < Conv.atoi32(txtCount.Text); i++)
                {
                    string min = Conv.atos(dgvTestList.Rows[i].Cells["MinValue"].Value);
                    string max = Conv.atos(dgvTestList.Rows[i].Cells["MaxValue"].Value);

                    strSQL = "UPDATE tbl_test_list SET";
                    strSQL += " test_name = '" + Conv.atos(dgvTestList.Rows[i].Cells["TestName"].Value) + "'";
                    strSQL += ",test_order = '" + Conv.atoi32(dgvTestList.Rows[i].Cells["TestOrder"].Value).ToString() + "'";
                    
                    if (min == "") 
                        strSQL += ",min_value = NULL";
                    else
                        strSQL += ",min_value = " + Conv.atod(min).ToString();
                    
                    if (max == "")
                        strSQL += ",max_value = NULL";
                    else
                        strSQL += ",max_value = " + Conv.atod(max).ToString();

                    strSQL += ", pcb_index = '" + Conv.atos(dgvTestList.Rows[i].Cells["PcbIndex"].Value) + "'";

                    strSQL += ",unit = '" + Conv.atos(dgvTestList.Rows[i].Cells["Unit"].Value) + "'";
                    strSQL += ",dmm_function = '" + Conv.atos(dgvTestList.Rows[i].Cells["DMMFunction"].Value) + "'";
                    strSQL += ",dmm_range = '" + Conv.atos(dgvTestList.Rows[i].Cells["DMMRange"].Value) + "'";
                    strSQL += ",popup_check = '" + Conv.atoi32(dgvTestList.Rows[i].Cells["PopupCheck"].Value).ToString() + "'";
                    strSQL += ",popup_message = '" + Conv.atos(dgvTestList.Rows[i].Cells["PopupMessage"].Value) + "'";
                    strSQL += ",popup_image = '" + Conv.atos(dgvTestList.Rows[i].Cells["PopupImage"].Value) + "'";

                    strSQL += ",relay_outputs = '" + Conv.atos(dgvTestList.Rows[i].Cells["RelayOutputs"].Value) + "'";
                    strSQL += ",stab_mstime = " + Conv.atoi32(dgvTestList.Rows[i].Cells["StabMsTime"].Value).ToString();
                    strSQL += ",meas_mstime = " + Conv.atoi32(dgvTestList.Rows[i].Cells["MeasMsTime"].Value).ToString();

                    strSQL += ",test_enable = " + Conv.atoi32(dgvTestList.Rows[i].Cells["TestEnable"].Value).ToString();

                    strSQL += " WHERE pkid=" + Conv.atoi32(dgvTestList.Rows[i].Cells["pkid"].Value).ToString();
                    SQLite.ExecuteNonQuery(strSQL);
                }
            }
            //Global.WriteLogFile("Ghi ITEM CONFIG");
            btnReload_Click(null, null);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTestList.CurrentRow != null)
            {
                DialogResult dlr = MessageBox.Show("Thực hiện xóa dữ liệu?", "XÁC NHẬN", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dlr == System.Windows.Forms.DialogResult.Yes)
                {
                    SQLite.ExecuteNonQuery("DELETE FROM tbl_test_list WHERE pkid=" + Conv.atoi32(dgvTestList.CurrentRow.Cells["pkid"].Value).ToString());
                    btnReload_Click(null, null);
                }
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
                    btnReload_Click(null, null);
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
