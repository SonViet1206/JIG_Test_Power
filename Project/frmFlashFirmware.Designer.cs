
namespace DefaultNS
{
    partial class frmFlashFirmware
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFlashFirmware));
            this.label4 = new System.Windows.Forms.Label();
            this.txtSerialNo = new System.Windows.Forms.TextBox();
            this.btnSendQR = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPortName = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.timerPollingCOMM = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(11, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 19);
            this.label4.TabIndex = 73;
            this.label4.Text = "QR Serial";
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.BackColor = System.Drawing.Color.White;
            this.txtSerialNo.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtSerialNo.Location = new System.Drawing.Point(112, 45);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.ReadOnly = true;
            this.txtSerialNo.Size = new System.Drawing.Size(265, 27);
            this.txtSerialNo.TabIndex = 74;
            this.txtSerialNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnSendQR
            // 
            this.btnSendQR.BackColor = System.Drawing.SystemColors.Control;
            this.btnSendQR.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendQR.Image = ((System.Drawing.Image)(resources.GetObject("btnSendQR.Image")));
            this.btnSendQR.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSendQR.Location = new System.Drawing.Point(383, 44);
            this.btnSendQR.Name = "btnSendQR";
            this.btnSendQR.Size = new System.Drawing.Size(98, 29);
            this.btnSendQR.TabIndex = 126;
            this.btnSendQR.Text = "   SEND";
            this.btnSendQR.UseVisualStyleBackColor = false;
            this.btnSendQR.Click += new System.EventHandler(this.btnSendQR_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 19);
            this.label1.TabIndex = 127;
            this.label1.Text = "COMM Port:";
            // 
            // txtPortName
            // 
            this.txtPortName.BackColor = System.Drawing.Color.White;
            this.txtPortName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtPortName.Location = new System.Drawing.Point(112, 12);
            this.txtPortName.Name = "txtPortName";
            this.txtPortName.ReadOnly = true;
            this.txtPortName.Size = new System.Drawing.Size(127, 27);
            this.txtPortName.TabIndex = 128;
            this.txtPortName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.Color.White;
            this.lblMessage.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lblMessage.Location = new System.Drawing.Point(12, 78);
            this.lblMessage.Multiline = true;
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.ReadOnly = true;
            this.lblMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.lblMessage.Size = new System.Drawing.Size(469, 108);
            this.lblMessage.TabIndex = 132;
            this.lblMessage.Text = "Gửi số chế tạo thành công.\r\nCửa sổ này sẽ tự đóng lại khi có kết quả từ phần mềm " +
    "nạp FIRMWARE.\r\n...\r\n";
            this.lblMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.btnOK, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(12, 192);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(469, 46);
            this.tableLayoutPanel3.TabIndex = 140;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Lime;
            this.btnOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOK.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(3, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(228, 40);
            this.btnOK.TabIndex = 81;
            this.btnOK.Text = "SET OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Red;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(237, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(229, 40);
            this.btnCancel.TabIndex = 81;
            this.btnCancel.Text = "SET NG";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // timerPollingCOMM
            // 
            this.timerPollingCOMM.Interval = 500;
            this.timerPollingCOMM.Tick += new System.EventHandler(this.timerPollingCOMM_Tick);
            // 
            // frmFlashFirmware
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 245);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPortName);
            this.Controls.Add(this.btnSendQR);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtSerialNo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmFlashFirmware";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LIÊN KẾT NẠP FIRMWARE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFlashFirmware_FormClosing);
            this.Load += new System.EventHandler(this.frmFlashFirmware_Load);
            this.Shown += new System.EventHandler(this.frmFlashFirmware_Shown);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSendQR;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox txtPortName;
        public System.Windows.Forms.TextBox txtSerialNo;
        private System.Windows.Forms.TextBox lblMessage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer timerPollingCOMM;
    }
}