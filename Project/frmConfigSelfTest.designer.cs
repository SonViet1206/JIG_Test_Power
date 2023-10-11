namespace DefaultNS
{
    partial class frmConfigSelfTest
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
            this.numDeltaT = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMesCOMMPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDUTStaticIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numDeltaU = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numDeltaI = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numDeltaP = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.numLoadPower = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnDefault = new System.Windows.Forms.Button();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtRepeatPassword = new System.Windows.Forms.TextBox();
            this.btnSavePassword = new System.Windows.Forms.Button();
            this.btnPassword = new System.Windows.Forms.Button();
            this.btnTestCOM = new System.Windows.Forms.Button();
            this.txtBoardType = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaU)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLoadPower)).BeginInit();
            this.SuspendLayout();
            // 
            // numDeltaT
            // 
            this.numDeltaT.DecimalPlaces = 1;
            this.numDeltaT.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.numDeltaT.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numDeltaT.Location = new System.Drawing.Point(898, 20);
            this.numDeltaT.Name = "numDeltaT";
            this.numDeltaT.Size = new System.Drawing.Size(125, 27);
            this.numDeltaT.TabIndex = 125;
            this.numDeltaT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numDeltaT.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(548, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(237, 19);
            this.label3.TabIndex = 123;
            this.label3.Text = "Sai lệch nhiệt độ cho phép (°C):";
            // 
            // txtMesCOMMPort
            // 
            this.txtMesCOMMPort.BackColor = System.Drawing.Color.White;
            this.txtMesCOMMPort.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtMesCOMMPort.Location = new System.Drawing.Point(209, 89);
            this.txtMesCOMMPort.Name = "txtMesCOMMPort";
            this.txtMesCOMMPort.Size = new System.Drawing.Size(125, 27);
            this.txtMesCOMMPort.TabIndex = 124;
            this.txtMesCOMMPort.Text = "COM30";
            this.txtMesCOMMPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 19);
            this.label1.TabIndex = 123;
            this.label1.Text = "Cổng COM ghép QR:";
            // 
            // txtDUTStaticIP
            // 
            this.txtDUTStaticIP.BackColor = System.Drawing.Color.White;
            this.txtDUTStaticIP.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtDUTStaticIP.Location = new System.Drawing.Point(209, 56);
            this.txtDUTStaticIP.Name = "txtDUTStaticIP";
            this.txtDUTStaticIP.Size = new System.Drawing.Size(290, 27);
            this.txtDUTStaticIP.TabIndex = 124;
            this.txtDUTStaticIP.Text = "10.128.128.71";
            this.txtDUTStaticIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(24, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(156, 19);
            this.label2.TabIndex = 123;
            this.label2.Text = "IP tĩnh của trạm sạc:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(548, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(239, 19);
            this.label4.TabIndex = 123;
            this.label4.Text = "Sai lệch cho phép điện áp U (V):";
            // 
            // numDeltaU
            // 
            this.numDeltaU.DecimalPlaces = 1;
            this.numDeltaU.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.numDeltaU.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numDeltaU.Location = new System.Drawing.Point(898, 119);
            this.numDeltaU.Name = "numDeltaU";
            this.numDeltaU.Size = new System.Drawing.Size(125, 27);
            this.numDeltaU.TabIndex = 125;
            this.numDeltaU.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numDeltaU.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(548, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(268, 19);
            this.label5.TabIndex = 123;
            this.label5.Text = "Sai lệch cho phép dòng điện I (mA):";
            // 
            // numDeltaI
            // 
            this.numDeltaI.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.numDeltaI.Location = new System.Drawing.Point(898, 152);
            this.numDeltaI.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numDeltaI.Name = "numDeltaI";
            this.numDeltaI.Size = new System.Drawing.Size(125, 27);
            this.numDeltaI.TabIndex = 125;
            this.numDeltaI.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numDeltaI.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(548, 187);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(257, 19);
            this.label6.TabIndex = 123;
            this.label6.Text = "Sai lệch cho phép công suất P (W):";
            // 
            // numDeltaP
            // 
            this.numDeltaP.DecimalPlaces = 1;
            this.numDeltaP.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.numDeltaP.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numDeltaP.Location = new System.Drawing.Point(898, 185);
            this.numDeltaP.Name = "numDeltaP";
            this.numDeltaP.Size = new System.Drawing.Size(125, 27);
            this.numDeltaP.TabIndex = 125;
            this.numDeltaP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numDeltaP.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(24, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(315, 19);
            this.label7.TabIndex = 123;
            this.label7.Text = "Trạm sạc 60kW trong chuỗi Serial có chứa:";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.BackColor = System.Drawing.SystemColors.Control;
            this.btnSave.Enabled = false;
            this.btnSave.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(361, 423);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(138, 51);
            this.btnSave.TabIndex = 127;
            this.btnSave.Text = "LƯU";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.Control;
            this.btnCancel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(195, 423);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(138, 51);
            this.btnCancel.TabIndex = 128;
            this.btnCancel.Text = "HỦY";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // numLoadPower
            // 
            this.numLoadPower.DecimalPlaces = 1;
            this.numLoadPower.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.numLoadPower.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numLoadPower.Location = new System.Drawing.Point(898, 218);
            this.numLoadPower.Name = "numLoadPower";
            this.numLoadPower.Size = new System.Drawing.Size(125, 27);
            this.numLoadPower.TabIndex = 130;
            this.numLoadPower.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numLoadPower.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(548, 220);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(290, 19);
            this.label8.TabIndex = 129;
            this.label8.Text = "Công suất tải đinh mức được kiểm (W):";
            // 
            // txtLogPath
            // 
            this.txtLogPath.BackColor = System.Drawing.Color.White;
            this.txtLogPath.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLogPath.Location = new System.Drawing.Point(28, 320);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(435, 23);
            this.txtLogPath.TabIndex = 132;
            this.txtLogPath.Text = "d:\\\\";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(24, 289);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(258, 19);
            this.label9.TabIndex = 129;
            this.label9.Text = "Đường dẫn lưu trữ DATA LOG file:";
            // 
            // btnDefault
            // 
            this.btnDefault.Location = new System.Drawing.Point(374, 287);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(125, 27);
            this.btnDefault.TabIndex = 133;
            this.btnDefault.Text = "Mặc Định";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(470, 320);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(29, 23);
            this.btnSelectFolder.TabIndex = 134;
            this.btnSelectFolder.Text = "...";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // txtNewPassword
            // 
            this.txtNewPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtNewPassword.BackColor = System.Drawing.Color.White;
            this.txtNewPassword.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtNewPassword.Location = new System.Drawing.Point(209, 349);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.Size = new System.Drawing.Size(159, 27);
            this.txtNewPassword.TabIndex = 138;
            this.txtNewPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtNewPassword.UseSystemPasswordChar = true;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Tahoma", 12F);
            this.label10.Location = new System.Drawing.Point(24, 352);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(111, 19);
            this.label10.TabIndex = 137;
            this.label10.Text = "Mật khẩu mới:";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Tahoma", 12F);
            this.label11.Location = new System.Drawing.Point(24, 385);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(142, 19);
            this.label11.TabIndex = 136;
            this.label11.Text = "Nhắc lại mật khẩu:";
            // 
            // txtRepeatPassword
            // 
            this.txtRepeatPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtRepeatPassword.BackColor = System.Drawing.Color.White;
            this.txtRepeatPassword.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtRepeatPassword.Location = new System.Drawing.Point(209, 382);
            this.txtRepeatPassword.Name = "txtRepeatPassword";
            this.txtRepeatPassword.Size = new System.Drawing.Size(159, 27);
            this.txtRepeatPassword.TabIndex = 135;
            this.txtRepeatPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtRepeatPassword.UseSystemPasswordChar = true;
            // 
            // btnSavePassword
            // 
            this.btnSavePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSavePassword.Enabled = false;
            this.btnSavePassword.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnSavePassword.Location = new System.Drawing.Point(374, 349);
            this.btnSavePassword.Name = "btnSavePassword";
            this.btnSavePassword.Size = new System.Drawing.Size(125, 60);
            this.btnSavePassword.TabIndex = 139;
            this.btnSavePassword.Text = "Lưu Mật Khẩu";
            this.btnSavePassword.UseVisualStyleBackColor = true;
            this.btnSavePassword.Click += new System.EventHandler(this.btnSavePassword_Click);
            // 
            // btnPassword
            // 
            this.btnPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPassword.BackColor = System.Drawing.SystemColors.Control;
            this.btnPassword.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPassword.Location = new System.Drawing.Point(28, 423);
            this.btnPassword.Name = "btnPassword";
            this.btnPassword.Size = new System.Drawing.Size(138, 51);
            this.btnPassword.TabIndex = 140;
            this.btnPassword.Text = "MẬT KHẨU";
            this.btnPassword.UseVisualStyleBackColor = false;
            this.btnPassword.Click += new System.EventHandler(this.btnPassword_Click);
            // 
            // btnTestCOM
            // 
            this.btnTestCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTestCOM.BackColor = System.Drawing.SystemColors.Control;
            this.btnTestCOM.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestCOM.Location = new System.Drawing.Point(374, 89);
            this.btnTestCOM.Name = "btnTestCOM";
            this.btnTestCOM.Size = new System.Drawing.Size(125, 27);
            this.btnTestCOM.TabIndex = 141;
            this.btnTestCOM.Text = "TEST";
            this.btnTestCOM.UseVisualStyleBackColor = false;
            this.btnTestCOM.Click += new System.EventHandler(this.btnTestCOM_Click);
            // 
            // txtBoardType
            // 
            this.txtBoardType.BackColor = System.Drawing.Color.White;
            this.txtBoardType.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtBoardType.Location = new System.Drawing.Point(374, 19);
            this.txtBoardType.Name = "txtBoardType";
            this.txtBoardType.Size = new System.Drawing.Size(125, 27);
            this.txtBoardType.TabIndex = 142;
            this.txtBoardType.Text = "pcb00008284";
            this.txtBoardType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // frmConfigSelfTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 486);
            this.Controls.Add(this.txtBoardType);
            this.Controls.Add(this.btnTestCOM);
            this.Controls.Add(this.btnPassword);
            this.Controls.Add(this.btnSavePassword);
            this.Controls.Add(this.txtNewPassword);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtRepeatPassword);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.txtLogPath);
            this.Controls.Add(this.numLoadPower);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.numDeltaP);
            this.Controls.Add(this.numDeltaI);
            this.Controls.Add(this.numDeltaU);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numDeltaT);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDUTStaticIP);
            this.Controls.Add(this.txtMesCOMMPort);
            this.Name = "frmConfigSelfTest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Định nghĩa thông tin bước kiểm";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaU)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeltaP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLoadPower)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numDeltaT;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMesCOMMPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDUTStaticIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numDeltaU;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numDeltaI;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numDeltaP;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown numLoadPower;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnDefault;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtRepeatPassword;
        private System.Windows.Forms.Button btnSavePassword;
        private System.Windows.Forms.Button btnPassword;
        private System.Windows.Forms.Button btnTestCOM;
        private System.Windows.Forms.TextBox txtBoardType;
    }
}