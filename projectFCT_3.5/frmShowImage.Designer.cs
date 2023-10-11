namespace DefaultNS
{
    partial class frmShowImage
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
            this.picHelp = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTestName = new System.Windows.Forms.Label();
            this.lblTestSteep = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.Label();
            this.timer10ms = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picHelp)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picHelp
            // 
            this.picHelp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picHelp.Location = new System.Drawing.Point(0, 29);
            this.picHelp.Name = "picHelp";
            this.picHelp.Size = new System.Drawing.Size(1023, 325);
            this.picHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picHelp.TabIndex = 0;
            this.picHelp.TabStop = false;
            this.picHelp.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblTestName);
            this.panel1.Controls.Add(this.lblTestSteep);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1023, 29);
            this.panel1.TabIndex = 1;
            // 
            // lblTestName
            // 
            this.lblTestName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTestName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTestName.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTestName.Location = new System.Drawing.Point(49, 2);
            this.lblTestName.Name = "lblTestName";
            this.lblTestName.Size = new System.Drawing.Size(971, 25);
            this.lblTestName.TabIndex = 2;
            this.lblTestName.Text = "-";
            this.lblTestName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTestName.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // lblTestSteep
            // 
            this.lblTestSteep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTestSteep.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTestSteep.Location = new System.Drawing.Point(3, 2);
            this.lblTestSteep.Name = "lblTestSteep";
            this.lblTestSteep.Size = new System.Drawing.Size(44, 25);
            this.lblTestSteep.TabIndex = 2;
            this.lblTestSteep.Text = "10";
            this.lblTestSteep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTestSteep.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.White;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtMessage.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(0, 354);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(1023, 91);
            this.txtMessage.TabIndex = 3;
            this.txtMessage.Text = "-";
            this.txtMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // timer10ms
            // 
            this.timer10ms.Interval = 10;
            this.timer10ms.Tick += new System.EventHandler(this.timer10ms_Tick);
            // 
            // frmShowImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1023, 445);
            this.Controls.Add(this.picHelp);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmShowImage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HƯỚNG DẪN THAO TÁC BƯỚC KIỂM";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmShowImage_FormClosing);
            this.Load += new System.EventHandler(this.frmShowImage_Load);
            this.Shown += new System.EventHandler(this.frmShowImage_Shown);
            this.Click += new System.EventHandler(this.pictureBox1_Click);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmShowImage_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picHelp)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label lblTestSteep;
        public System.Windows.Forms.Label lblTestName;
        public System.Windows.Forms.PictureBox picHelp;
        public System.Windows.Forms.Label txtMessage;
        private System.Windows.Forms.Timer timer10ms;
    }
}