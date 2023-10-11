namespace DefaultNS
{
    partial class frmShowChoice
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTestName = new System.Windows.Forms.Label();
            this.lblCounter = new System.Windows.Forms.Label();
            this.lblTestSteep = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnSetNG = new System.Windows.Forms.Button();
            this.btnSetOK = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblTestName);
            this.panel1.Controls.Add(this.lblCounter);
            this.panel1.Controls.Add(this.lblTestSteep);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 37);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(748, 28);
            this.panel1.TabIndex = 1;
            // 
            // lblTestName
            // 
            this.lblTestName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTestName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTestName.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTestName.Location = new System.Drawing.Point(87, 2);
            this.lblTestName.Name = "lblTestName";
            this.lblTestName.Size = new System.Drawing.Size(574, 25);
            this.lblTestName.TabIndex = 2;
            this.lblTestName.Text = "-";
            this.lblTestName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCounter
            // 
            this.lblCounter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCounter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblCounter.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCounter.Location = new System.Drawing.Point(667, 2);
            this.lblCounter.Name = "lblCounter";
            this.lblCounter.Size = new System.Drawing.Size(78, 25);
            this.lblCounter.TabIndex = 2;
            this.lblCounter.Text = "0";
            this.lblCounter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTestSteep
            // 
            this.lblTestSteep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTestSteep.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTestSteep.Location = new System.Drawing.Point(3, 2);
            this.lblTestSteep.Name = "lblTestSteep";
            this.lblTestSteep.Size = new System.Drawing.Size(78, 25);
            this.lblTestSteep.TabIndex = 2;
            this.lblTestSteep.Text = "10";
            this.lblTestSteep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMessage
            // 
            this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMessage.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(0, 65);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(748, 117);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "1\r\n2\r\n3\r\n4\r\n5";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSetNG
            // 
            this.btnSetNG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetNG.BackColor = System.Drawing.Color.Red;
            this.btnSetNG.Enabled = false;
            this.btnSetNG.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetNG.Location = new System.Drawing.Point(535, 185);
            this.btnSetNG.Name = "btnSetNG";
            this.btnSetNG.Size = new System.Drawing.Size(213, 42);
            this.btnSetNG.TabIndex = 127;
            this.btnSetNG.Text = "Set NG";
            this.btnSetNG.UseVisualStyleBackColor = false;
            this.btnSetNG.Click += new System.EventHandler(this.btnSetNG_Click);
            // 
            // btnSetOK
            // 
            this.btnSetOK.BackColor = System.Drawing.Color.Lime;
            this.btnSetOK.Enabled = false;
            this.btnSetOK.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetOK.Location = new System.Drawing.Point(-1, 185);
            this.btnSetOK.Name = "btnSetOK";
            this.btnSetOK.Size = new System.Drawing.Size(211, 42);
            this.btnSetOK.TabIndex = 126;
            this.btnSetOK.Text = "Set OK";
            this.btnSetOK.UseVisualStyleBackColor = false;
            this.btnSetOK.Click += new System.EventHandler(this.btnSetOK_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(748, 37);
            this.lblHeader.TabIndex = 13;
            this.lblHeader.Text = "HƯỚNG DẪN THAO TÁC BƯỚC KIỂM";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseDown);
            this.lblHeader.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseMove);
            this.lblHeader.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseUp);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnClose.Enabled = false;
            this.btnClose.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(217, 185);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(312, 42);
            this.btnClose.TabIndex = 128;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmShowChoice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 230);
            this.Controls.Add(this.btnSetNG);
            this.Controls.Add(this.btnSetOK);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmShowChoice";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HƯỚNG DẪN THAO TÁC";
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmShowImage_KeyDown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label lblTestSteep;
        public System.Windows.Forms.Label lblTestName;
        public System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblCounter;
        public System.Windows.Forms.Label lblHeader;
        public System.Windows.Forms.Button btnSetNG;
        public System.Windows.Forms.Button btnSetOK;
        public System.Windows.Forms.Button btnClose;
    }
}