namespace DefaultNS
{
    partial class DMM7351E
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DMM7351E));
            this.btnOnOff = new System.Windows.Forms.Button();
            this.lblUnit = new System.Windows.Forms.Label();
            this.ledValue = new DmitryBrant.CustomControls.SevenSegmentArray();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblREC = new System.Windows.Forms.Label();
            this.cboCMDs = new System.Windows.Forms.ComboBox();
            this.lblFunction = new System.Windows.Forms.Label();
            this.lblSubHeader = new System.Windows.Forms.Label();
            this.picDCV = new System.Windows.Forms.PictureBox();
            this.picACV = new System.Windows.Forms.PictureBox();
            this.picOMH = new System.Windows.Forms.PictureBox();
            this.picDCI = new System.Windows.Forms.PictureBox();
            this.picACI = new System.Windows.Forms.PictureBox();
            this.picFREQ = new System.Windows.Forms.PictureBox();
            this.picAUTO = new System.Windows.Forms.PictureBox();
            this.picDOWN = new System.Windows.Forms.PictureBox();
            this.picUP = new System.Windows.Forms.PictureBox();
            this.picHOLD = new System.Windows.Forms.PictureBox();
            this.picTRIG = new System.Windows.Forms.PictureBox();
            this.picRATE = new System.Windows.Forms.PictureBox();
            this.lastRAN = new System.Windows.Forms.Label();
            this.lastFUN = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picDCV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picACV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOMH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDCI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picACI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFREQ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAUTO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDOWN)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picUP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHOLD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTRIG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRATE)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOnOff
            // 
            this.btnOnOff.BackColor = System.Drawing.Color.Black;
            this.btnOnOff.Location = new System.Drawing.Point(287, 9);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(47, 19);
            this.btnOnOff.TabIndex = 119;
            this.btnOnOff.UseVisualStyleBackColor = false;
            this.btnOnOff.Click += new System.EventHandler(this.btnOnOff_Click);
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.BackColor = System.Drawing.Color.Black;
            this.lblUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnit.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblUnit.Location = new System.Drawing.Point(243, 51);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(35, 20);
            this.lblUnit.TabIndex = 120;
            this.lblUnit.Text = "mV";
            // 
            // ledValue
            // 
            this.ledValue.ArrayCount = 9;
            this.ledValue.ColorBackground = System.Drawing.Color.Black;
            this.ledValue.ColorDark = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ledValue.ColorLight = System.Drawing.Color.MediumSpringGreen;
            this.ledValue.DecimalShow = true;
            this.ledValue.ElementPadding = new System.Windows.Forms.Padding(4);
            this.ledValue.ElementWidth = 10;
            this.ledValue.ItalicFactor = -0.12F;
            this.ledValue.Location = new System.Drawing.Point(30, 27);
            this.ledValue.Name = "ledValue";
            this.ledValue.Size = new System.Drawing.Size(215, 44);
            this.ledValue.TabIndex = 118;
            this.ledValue.TabStop = false;
            this.ledValue.Value = "0.0";
            // 
            // btnSend
            // 
            this.btnSend.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(218, 203);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(65, 29);
            this.btnSend.TabIndex = 123;
            this.btnSend.Text = "SEND";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblREC
            // 
            this.lblREC.BackColor = System.Drawing.Color.White;
            this.lblREC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblREC.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lblREC.Location = new System.Drawing.Point(0, 161);
            this.lblREC.Name = "lblREC";
            this.lblREC.Size = new System.Drawing.Size(282, 40);
            this.lblREC.TabIndex = 122;
            this.lblREC.Text = "1\r\n2";
            this.lblREC.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cboCMDs
            // 
            this.cboCMDs.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.cboCMDs.FormattingEnabled = true;
            this.cboCMDs.Location = new System.Drawing.Point(0, 204);
            this.cboCMDs.Name = "cboCMDs";
            this.cboCMDs.Size = new System.Drawing.Size(212, 27);
            this.cboCMDs.TabIndex = 124;
            this.cboCMDs.Text = "*IDN?";
            this.cboCMDs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboCMDs_KeyDown);
            // 
            // lblFunction
            // 
            this.lblFunction.AutoSize = true;
            this.lblFunction.BackColor = System.Drawing.Color.Black;
            this.lblFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFunction.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblFunction.Location = new System.Drawing.Point(232, 9);
            this.lblFunction.Name = "lblFunction";
            this.lblFunction.Size = new System.Drawing.Size(46, 20);
            this.lblFunction.TabIndex = 125;
            this.lblFunction.Text = "DCV";
            // 
            // lblSubHeader
            // 
            this.lblSubHeader.AutoSize = true;
            this.lblSubHeader.BackColor = System.Drawing.Color.Black;
            this.lblSubHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubHeader.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.lblSubHeader.Location = new System.Drawing.Point(8, 51);
            this.lblSubHeader.Name = "lblSubHeader";
            this.lblSubHeader.Size = new System.Drawing.Size(21, 20);
            this.lblSubHeader.TabIndex = 126;
            this.lblSubHeader.Text = "V";
            // 
            // picDCV
            // 
            this.picDCV.Image = ((System.Drawing.Image)(resources.GetObject("picDCV.Image")));
            this.picDCV.Location = new System.Drawing.Point(28, 92);
            this.picDCV.Margin = new System.Windows.Forms.Padding(0);
            this.picDCV.Name = "picDCV";
            this.picDCV.Size = new System.Drawing.Size(24, 16);
            this.picDCV.TabIndex = 127;
            this.picDCV.TabStop = false;
            this.picDCV.Click += new System.EventHandler(this.picDCV_Click);
            // 
            // picACV
            // 
            this.picACV.Image = ((System.Drawing.Image)(resources.GetObject("picACV.Image")));
            this.picACV.Location = new System.Drawing.Point(58, 92);
            this.picACV.Margin = new System.Windows.Forms.Padding(0);
            this.picACV.Name = "picACV";
            this.picACV.Size = new System.Drawing.Size(24, 16);
            this.picACV.TabIndex = 128;
            this.picACV.TabStop = false;
            this.picACV.Click += new System.EventHandler(this.picACV_Click);
            // 
            // picOMH
            // 
            this.picOMH.Image = ((System.Drawing.Image)(resources.GetObject("picOMH.Image")));
            this.picOMH.Location = new System.Drawing.Point(89, 92);
            this.picOMH.Margin = new System.Windows.Forms.Padding(0);
            this.picOMH.Name = "picOMH";
            this.picOMH.Size = new System.Drawing.Size(24, 16);
            this.picOMH.TabIndex = 128;
            this.picOMH.TabStop = false;
            this.picOMH.Click += new System.EventHandler(this.picOMH_Click);
            // 
            // picDCI
            // 
            this.picDCI.Image = ((System.Drawing.Image)(resources.GetObject("picDCI.Image")));
            this.picDCI.Location = new System.Drawing.Point(120, 92);
            this.picDCI.Margin = new System.Windows.Forms.Padding(0);
            this.picDCI.Name = "picDCI";
            this.picDCI.Size = new System.Drawing.Size(24, 16);
            this.picDCI.TabIndex = 128;
            this.picDCI.TabStop = false;
            this.picDCI.Click += new System.EventHandler(this.picDCI_Click);
            // 
            // picACI
            // 
            this.picACI.Image = ((System.Drawing.Image)(resources.GetObject("picACI.Image")));
            this.picACI.Location = new System.Drawing.Point(150, 92);
            this.picACI.Margin = new System.Windows.Forms.Padding(0);
            this.picACI.Name = "picACI";
            this.picACI.Size = new System.Drawing.Size(24, 16);
            this.picACI.TabIndex = 128;
            this.picACI.TabStop = false;
            this.picACI.Click += new System.EventHandler(this.picACI_Click);
            // 
            // picFREQ
            // 
            this.picFREQ.Image = ((System.Drawing.Image)(resources.GetObject("picFREQ.Image")));
            this.picFREQ.Location = new System.Drawing.Point(180, 92);
            this.picFREQ.Margin = new System.Windows.Forms.Padding(0);
            this.picFREQ.Name = "picFREQ";
            this.picFREQ.Size = new System.Drawing.Size(24, 16);
            this.picFREQ.TabIndex = 128;
            this.picFREQ.TabStop = false;
            this.picFREQ.Click += new System.EventHandler(this.picFREQ_Click);
            // 
            // picAUTO
            // 
            this.picAUTO.Image = ((System.Drawing.Image)(resources.GetObject("picAUTO.Image")));
            this.picAUTO.Location = new System.Drawing.Point(56, 126);
            this.picAUTO.Margin = new System.Windows.Forms.Padding(0);
            this.picAUTO.Name = "picAUTO";
            this.picAUTO.Size = new System.Drawing.Size(16, 16);
            this.picAUTO.TabIndex = 129;
            this.picAUTO.TabStop = false;
            this.picAUTO.Click += new System.EventHandler(this.picAUTO_Click);
            // 
            // picDOWN
            // 
            this.picDOWN.Image = ((System.Drawing.Image)(resources.GetObject("picDOWN.Image")));
            this.picDOWN.Location = new System.Drawing.Point(78, 126);
            this.picDOWN.Margin = new System.Windows.Forms.Padding(0);
            this.picDOWN.Name = "picDOWN";
            this.picDOWN.Size = new System.Drawing.Size(16, 16);
            this.picDOWN.TabIndex = 129;
            this.picDOWN.TabStop = false;
            this.picDOWN.Click += new System.EventHandler(this.picDOWN_Click);
            // 
            // picUP
            // 
            this.picUP.Image = ((System.Drawing.Image)(resources.GetObject("picUP.Image")));
            this.picUP.Location = new System.Drawing.Point(100, 126);
            this.picUP.Margin = new System.Windows.Forms.Padding(0);
            this.picUP.Name = "picUP";
            this.picUP.Size = new System.Drawing.Size(16, 16);
            this.picUP.TabIndex = 129;
            this.picUP.TabStop = false;
            this.picUP.Click += new System.EventHandler(this.picUP_Click);
            // 
            // picHOLD
            // 
            this.picHOLD.Image = ((System.Drawing.Image)(resources.GetObject("picHOLD.Image")));
            this.picHOLD.Location = new System.Drawing.Point(142, 126);
            this.picHOLD.Margin = new System.Windows.Forms.Padding(0);
            this.picHOLD.Name = "picHOLD";
            this.picHOLD.Size = new System.Drawing.Size(16, 16);
            this.picHOLD.TabIndex = 129;
            this.picHOLD.TabStop = false;
            // 
            // picTRIG
            // 
            this.picTRIG.Image = ((System.Drawing.Image)(resources.GetObject("picTRIG.Image")));
            this.picTRIG.Location = new System.Drawing.Point(165, 126);
            this.picTRIG.Margin = new System.Windows.Forms.Padding(0);
            this.picTRIG.Name = "picTRIG";
            this.picTRIG.Size = new System.Drawing.Size(16, 16);
            this.picTRIG.TabIndex = 129;
            this.picTRIG.TabStop = false;
            // 
            // picRATE
            // 
            this.picRATE.Image = ((System.Drawing.Image)(resources.GetObject("picRATE.Image")));
            this.picRATE.Location = new System.Drawing.Point(187, 126);
            this.picRATE.Margin = new System.Windows.Forms.Padding(0);
            this.picRATE.Name = "picRATE";
            this.picRATE.Size = new System.Drawing.Size(16, 16);
            this.picRATE.TabIndex = 129;
            this.picRATE.TabStop = false;
            // 
            // lastRAN
            // 
            this.lastRAN.AutoSize = true;
            this.lastRAN.BackColor = System.Drawing.Color.Transparent;
            this.lastRAN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastRAN.ForeColor = System.Drawing.Color.MediumSeaGreen;
            this.lastRAN.Location = new System.Drawing.Point(311, 32);
            this.lastRAN.Name = "lastRAN";
            this.lastRAN.Size = new System.Drawing.Size(23, 13);
            this.lastRAN.TabIndex = 130;
            this.lastRAN.Text = "R0";
            // 
            // lastFUN
            // 
            this.lastFUN.AutoSize = true;
            this.lastFUN.BackColor = System.Drawing.Color.Transparent;
            this.lastFUN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastFUN.ForeColor = System.Drawing.Color.MediumSeaGreen;
            this.lastFUN.Location = new System.Drawing.Point(284, 32);
            this.lastFUN.Name = "lastFUN";
            this.lastFUN.Size = new System.Drawing.Size(21, 13);
            this.lastFUN.TabIndex = 130;
            this.lastFUN.Text = "F0";
            // 
            // DMM7351E
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.lastFUN);
            this.Controls.Add(this.lastRAN);
            this.Controls.Add(this.picRATE);
            this.Controls.Add(this.picTRIG);
            this.Controls.Add(this.picHOLD);
            this.Controls.Add(this.picUP);
            this.Controls.Add(this.picDOWN);
            this.Controls.Add(this.picAUTO);
            this.Controls.Add(this.picFREQ);
            this.Controls.Add(this.picACI);
            this.Controls.Add(this.picDCI);
            this.Controls.Add(this.picOMH);
            this.Controls.Add(this.picACV);
            this.Controls.Add(this.picDCV);
            this.Controls.Add(this.lblSubHeader);
            this.Controls.Add(this.lblFunction);
            this.Controls.Add(this.cboCMDs);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.lblREC);
            this.Controls.Add(this.lblUnit);
            this.Controls.Add(this.btnOnOff);
            this.Controls.Add(this.ledValue);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DMM7351E";
            this.Size = new System.Drawing.Size(391, 235);
            this.Load += new System.EventHandler(this.DMM7351E_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picDCV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picACV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOMH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDCI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picACI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFREQ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAUTO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDOWN)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picUP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHOLD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTRIG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRATE)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DmitryBrant.CustomControls.SevenSegmentArray ledValue;
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblREC;
        private System.Windows.Forms.ComboBox cboCMDs;
        private System.Windows.Forms.Label lblFunction;
        private System.Windows.Forms.Label lblSubHeader;
        private System.Windows.Forms.PictureBox picDCV;
        private System.Windows.Forms.PictureBox picACV;
        private System.Windows.Forms.PictureBox picOMH;
        private System.Windows.Forms.PictureBox picDCI;
        private System.Windows.Forms.PictureBox picACI;
        private System.Windows.Forms.PictureBox picFREQ;
        private System.Windows.Forms.PictureBox picAUTO;
        private System.Windows.Forms.PictureBox picDOWN;
        private System.Windows.Forms.PictureBox picUP;
        private System.Windows.Forms.PictureBox picHOLD;
        private System.Windows.Forms.PictureBox picTRIG;
        private System.Windows.Forms.PictureBox picRATE;
        private System.Windows.Forms.Label lastRAN;
        private System.Windows.Forms.Label lastFUN;
    }
}
