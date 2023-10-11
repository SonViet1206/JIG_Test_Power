namespace DefaultNS
{
    partial class DUT_MainAC11
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DUT_MainAC11));
            this.btnOnOff = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.lblHardwareVersion = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSoftwareVersion = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblDUTSerial = new System.Windows.Forms.Label();
            this.lblCANSerial = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.chkHex = new System.Windows.Forms.CheckBox();
            this.cboCMDs = new System.Windows.Forms.ComboBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblTmprValue = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtErr1 = new System.Windows.Forms.TextBox();
            this.txtErr2 = new System.Windows.Forms.TextBox();
            this.txtErr3 = new System.Windows.Forms.TextBox();
            this.label44 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblADC = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.lblFlashTest = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOutN = new System.Windows.Forms.Button();
            this.btnOutL1 = new System.Windows.Forms.Button();
            this.btnOutL2 = new System.Windows.Forms.Button();
            this.btnOutL3 = new System.Windows.Forms.Button();
            this.label40 = new System.Windows.Forms.Label();
            this.btnOutG = new System.Windows.Forms.Button();
            this.label41 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.chkInp1 = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnReadAllInput = new System.Windows.Forms.Button();
            this.chkInp0 = new System.Windows.Forms.CheckBox();
            this.chkInp3 = new System.Windows.Forms.CheckBox();
            this.chkInp2 = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkOut3 = new System.Windows.Forms.CheckBox();
            this.chkOut2 = new System.Windows.Forms.CheckBox();
            this.chkOut1 = new System.Windows.Forms.CheckBox();
            this.chkOut0 = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOnOff
            // 
            this.btnOnOff.BackColor = System.Drawing.Color.Black;
            this.btnOnOff.ForeColor = System.Drawing.Color.DarkRed;
            this.btnOnOff.Location = new System.Drawing.Point(5, 5);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(32, 32);
            this.btnOnOff.TabIndex = 119;
            this.btnOnOff.UseVisualStyleBackColor = false;
            this.btnOnOff.Click += new System.EventHandler(this.btnOnOff_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.DarkBlue;
            this.label15.Location = new System.Drawing.Point(43, 6);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(274, 29);
            this.label15.TabIndex = 126;
            this.label15.Text = "DUT - MAIN AC 11KW";
            // 
            // lblHardwareVersion
            // 
            this.lblHardwareVersion.AutoSize = true;
            this.lblHardwareVersion.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHardwareVersion.ForeColor = System.Drawing.Color.Navy;
            this.lblHardwareVersion.Location = new System.Drawing.Point(153, 78);
            this.lblHardwareVersion.Name = "lblHardwareVersion";
            this.lblHardwareVersion.Size = new System.Drawing.Size(30, 19);
            this.lblHardwareVersion.TabIndex = 127;
            this.lblHardwareVersion.Text = "---";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 19);
            this.label1.TabIndex = 127;
            this.label1.Text = "Software Version:";
            this.label1.DoubleClick += new System.EventHandler(this.label1_DoubleClick);
            // 
            // lblSoftwareVersion
            // 
            this.lblSoftwareVersion.AutoSize = true;
            this.lblSoftwareVersion.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSoftwareVersion.ForeColor = System.Drawing.Color.Navy;
            this.lblSoftwareVersion.Location = new System.Drawing.Point(153, 59);
            this.lblSoftwareVersion.Name = "lblSoftwareVersion";
            this.lblSoftwareVersion.Size = new System.Drawing.Size(30, 19);
            this.lblSoftwareVersion.TabIndex = 127;
            this.lblSoftwareVersion.Text = "---";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 40);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 19);
            this.label9.TabIndex = 127;
            this.label9.Text = "Serial:";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            this.label9.DoubleClick += new System.EventHandler(this.label9_DoubleClick);
            // 
            // lblDUTSerial
            // 
            this.lblDUTSerial.AutoSize = true;
            this.lblDUTSerial.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDUTSerial.ForeColor = System.Drawing.Color.Navy;
            this.lblDUTSerial.Location = new System.Drawing.Point(69, 40);
            this.lblDUTSerial.Name = "lblDUTSerial";
            this.lblDUTSerial.Size = new System.Drawing.Size(30, 19);
            this.lblDUTSerial.TabIndex = 127;
            this.lblDUTSerial.Text = "---";
            // 
            // lblCANSerial
            // 
            this.lblCANSerial.AutoSize = true;
            this.lblCANSerial.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCANSerial.ForeColor = System.Drawing.Color.Navy;
            this.lblCANSerial.Location = new System.Drawing.Point(412, 12);
            this.lblCANSerial.Name = "lblCANSerial";
            this.lblCANSerial.Size = new System.Drawing.Size(30, 19);
            this.lblCANSerial.TabIndex = 135;
            this.lblCANSerial.Text = "---";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(315, 12);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(91, 19);
            this.label24.TabIndex = 136;
            this.label24.Text = "CAN Serial:";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(12, 277);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1170, 95);
            this.txtLog.TabIndex = 137;
            // 
            // chkHex
            // 
            this.chkHex.AutoSize = true;
            this.chkHex.Checked = true;
            this.chkHex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHex.Enabled = false;
            this.chkHex.Location = new System.Drawing.Point(371, 247);
            this.chkHex.Name = "chkHex";
            this.chkHex.Size = new System.Drawing.Size(63, 23);
            this.chkHex.TabIndex = 139;
            this.chkHex.Text = "HEX:";
            this.chkHex.UseVisualStyleBackColor = true;
            // 
            // cboCMDs
            // 
            this.cboCMDs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCMDs.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.cboCMDs.FormattingEnabled = true;
            this.cboCMDs.Items.AddRange(new object[] {
            "02 01 60",
            "02 01 61",
            "02 02 62 01"});
            this.cboCMDs.Location = new System.Drawing.Point(440, 245);
            this.cboCMDs.Name = "cboCMDs";
            this.cboCMDs.Size = new System.Drawing.Size(563, 27);
            this.cboCMDs.TabIndex = 140;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(1015, 244);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(71, 29);
            this.btnSend.TabIndex = 141;
            this.btnSend.Text = "SEND";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblTmprValue
            // 
            this.lblTmprValue.AutoSize = true;
            this.lblTmprValue.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTmprValue.ForeColor = System.Drawing.Color.Navy;
            this.lblTmprValue.Location = new System.Drawing.Point(153, 97);
            this.lblTmprValue.Name = "lblTmprValue";
            this.lblTmprValue.Size = new System.Drawing.Size(30, 19);
            this.lblTmprValue.TabIndex = 143;
            this.lblTmprValue.Text = "---";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 19);
            this.label4.TabIndex = 144;
            this.label4.Text = "Temprature (°C):";
            this.label4.DoubleClick += new System.EventHandler(this.label4_DoubleClick);
            // 
            // txtErr1
            // 
            this.txtErr1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtErr1.Location = new System.Drawing.Point(106, 245);
            this.txtErr1.Name = "txtErr1";
            this.txtErr1.Size = new System.Drawing.Size(63, 27);
            this.txtErr1.TabIndex = 145;
            this.txtErr1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtErr2
            // 
            this.txtErr2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtErr2.Location = new System.Drawing.Point(174, 245);
            this.txtErr2.Name = "txtErr2";
            this.txtErr2.Size = new System.Drawing.Size(63, 27);
            this.txtErr2.TabIndex = 145;
            this.txtErr2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtErr3
            // 
            this.txtErr3.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtErr3.Location = new System.Drawing.Point(243, 245);
            this.txtErr3.Name = "txtErr3";
            this.txtErr3.Size = new System.Drawing.Size(63, 27);
            this.txtErr3.TabIndex = 145;
            this.txtErr3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(8, 249);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(92, 19);
            this.label44.TabIndex = 146;
            this.label44.Text = "Error Code:";
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(1092, 244);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 29);
            this.btnClear.TabIndex = 147;
            this.btnClear.Text = "CLEAR";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lblADC
            // 
            this.lblADC.AutoSize = true;
            this.lblADC.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblADC.ForeColor = System.Drawing.Color.Navy;
            this.lblADC.Location = new System.Drawing.Point(153, 154);
            this.lblADC.Name = "lblADC";
            this.lblADC.Size = new System.Drawing.Size(30, 19);
            this.lblADC.TabIndex = 148;
            this.lblADC.Text = "---";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(9, 154);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(105, 19);
            this.label45.TabIndex = 149;
            this.label45.Text = "ADC Voltage:";
            this.label45.DoubleClick += new System.EventHandler(this.label45_DoubleClick);
            // 
            // lblFlashTest
            // 
            this.lblFlashTest.AutoSize = true;
            this.lblFlashTest.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFlashTest.ForeColor = System.Drawing.Color.Navy;
            this.lblFlashTest.Location = new System.Drawing.Point(153, 116);
            this.lblFlashTest.Name = "lblFlashTest";
            this.lblFlashTest.Size = new System.Drawing.Size(30, 19);
            this.lblFlashTest.TabIndex = 152;
            this.lblFlashTest.Text = "---";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(9, 116);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(86, 19);
            this.label30.TabIndex = 153;
            this.label30.Text = "Flash Test:";
            this.label30.DoubleClick += new System.EventHandler(this.label30_DoubleClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 19);
            this.label2.TabIndex = 154;
            this.label2.Text = "Tony Version:";
            this.label2.DoubleClick += new System.EventHandler(this.label2_DoubleClick);
            // 
            // btnOutN
            // 
            this.btnOutN.BackColor = System.Drawing.Color.DimGray;
            this.btnOutN.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOutN.BackgroundImage")));
            this.btnOutN.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOutN.Location = new System.Drawing.Point(791, 77);
            this.btnOutN.Name = "btnOutN";
            this.btnOutN.Size = new System.Drawing.Size(32, 32);
            this.btnOutN.TabIndex = 164;
            this.btnOutN.UseVisualStyleBackColor = false;
            this.btnOutN.Click += new System.EventHandler(this.btnOutN_L1_Click);
            // 
            // btnOutL1
            // 
            this.btnOutL1.BackColor = System.Drawing.Color.DimGray;
            this.btnOutL1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOutL1.BackgroundImage")));
            this.btnOutL1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOutL1.Location = new System.Drawing.Point(791, 112);
            this.btnOutL1.Name = "btnOutL1";
            this.btnOutL1.Size = new System.Drawing.Size(32, 32);
            this.btnOutL1.TabIndex = 165;
            this.btnOutL1.UseVisualStyleBackColor = false;
            this.btnOutL1.Click += new System.EventHandler(this.btnOutL1_Click);
            // 
            // btnOutL2
            // 
            this.btnOutL2.BackColor = System.Drawing.Color.DimGray;
            this.btnOutL2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOutL2.BackgroundImage")));
            this.btnOutL2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOutL2.Location = new System.Drawing.Point(791, 147);
            this.btnOutL2.Name = "btnOutL2";
            this.btnOutL2.Size = new System.Drawing.Size(32, 32);
            this.btnOutL2.TabIndex = 166;
            this.btnOutL2.UseVisualStyleBackColor = false;
            this.btnOutL2.Click += new System.EventHandler(this.btnOutL2_L3_Click);
            // 
            // btnOutL3
            // 
            this.btnOutL3.BackColor = System.Drawing.Color.DimGray;
            this.btnOutL3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOutL3.BackgroundImage")));
            this.btnOutL3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOutL3.Location = new System.Drawing.Point(791, 182);
            this.btnOutL3.Name = "btnOutL3";
            this.btnOutL3.Size = new System.Drawing.Size(32, 32);
            this.btnOutL3.TabIndex = 167;
            this.btnOutL3.UseVisualStyleBackColor = false;
            this.btnOutL3.Click += new System.EventHandler(this.btnOutL3_Click);
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label40.Location = new System.Drawing.Point(764, 84);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(21, 19);
            this.label40.TabIndex = 168;
            this.label40.Text = "N";
            // 
            // btnOutG
            // 
            this.btnOutG.BackColor = System.Drawing.Color.DimGray;
            this.btnOutG.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOutG.BackgroundImage")));
            this.btnOutG.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOutG.Location = new System.Drawing.Point(791, 42);
            this.btnOutG.Name = "btnOutG";
            this.btnOutG.Size = new System.Drawing.Size(32, 32);
            this.btnOutG.TabIndex = 169;
            this.btnOutG.UseVisualStyleBackColor = false;
            this.btnOutG.Click += new System.EventHandler(this.btnOutG_Click);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label41.Location = new System.Drawing.Point(757, 119);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(28, 19);
            this.label41.TabIndex = 168;
            this.label41.Text = "L1";
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label42.Location = new System.Drawing.Point(757, 154);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(28, 19);
            this.label42.TabIndex = 168;
            this.label42.Text = "L2";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label43.Location = new System.Drawing.Point(757, 189);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(28, 19);
            this.label43.TabIndex = 168;
            this.label43.Text = "L3";
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label46.Location = new System.Drawing.Point(764, 49);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(21, 19);
            this.label46.TabIndex = 168;
            this.label46.Text = "G";
            // 
            // chkInp1
            // 
            this.chkInp1.AutoSize = true;
            this.chkInp1.Enabled = false;
            this.chkInp1.Location = new System.Drawing.Point(6, 60);
            this.chkInp1.Name = "chkInp1";
            this.chkInp1.Size = new System.Drawing.Size(15, 14);
            this.chkInp1.TabIndex = 170;
            this.chkInp1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnReadAllInput);
            this.groupBox1.Controls.Add(this.chkInp0);
            this.groupBox1.Controls.Add(this.chkInp3);
            this.groupBox1.Controls.Add(this.chkInp2);
            this.groupBox1.Controls.Add(this.chkInp1);
            this.groupBox1.Location = new System.Drawing.Point(365, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 144);
            this.groupBox1.TabIndex = 171;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "GPIO Input";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(170, 19);
            this.label7.TabIndex = 179;
            this.label7.Text = "MCU_EARTH_DETECT";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 19);
            this.label6.TabIndex = 178;
            this.label6.Text = "MCU_GFIC_DC";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 19);
            this.label5.TabIndex = 177;
            this.label5.Text = "MCU_GFIC_AC";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 19);
            this.label3.TabIndex = 176;
            this.label3.Text = "MCU_EMERGENCY";
            // 
            // btnReadAllInput
            // 
            this.btnReadAllInput.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReadAllInput.Location = new System.Drawing.Point(130, -1);
            this.btnReadAllInput.Name = "btnReadAllInput";
            this.btnReadAllInput.Size = new System.Drawing.Size(63, 24);
            this.btnReadAllInput.TabIndex = 175;
            this.btnReadAllInput.Text = "ReadAll";
            this.btnReadAllInput.UseVisualStyleBackColor = true;
            this.btnReadAllInput.Click += new System.EventHandler(this.btnReadAllInput_Click);
            // 
            // chkInp0
            // 
            this.chkInp0.AutoSize = true;
            this.chkInp0.Enabled = false;
            this.chkInp0.Location = new System.Drawing.Point(6, 31);
            this.chkInp0.Name = "chkInp0";
            this.chkInp0.Size = new System.Drawing.Size(15, 14);
            this.chkInp0.TabIndex = 174;
            this.chkInp0.UseVisualStyleBackColor = true;
            // 
            // chkInp3
            // 
            this.chkInp3.AutoSize = true;
            this.chkInp3.Enabled = false;
            this.chkInp3.Location = new System.Drawing.Point(6, 118);
            this.chkInp3.Name = "chkInp3";
            this.chkInp3.Size = new System.Drawing.Size(15, 14);
            this.chkInp3.TabIndex = 172;
            this.chkInp3.UseVisualStyleBackColor = true;
            // 
            // chkInp2
            // 
            this.chkInp2.AutoSize = true;
            this.chkInp2.Enabled = false;
            this.chkInp2.Location = new System.Drawing.Point(6, 89);
            this.chkInp2.Name = "chkInp2";
            this.chkInp2.Size = new System.Drawing.Size(15, 14);
            this.chkInp2.TabIndex = 171;
            this.chkInp2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkOut3);
            this.groupBox2.Controls.Add(this.chkOut2);
            this.groupBox2.Controls.Add(this.chkOut1);
            this.groupBox2.Controls.Add(this.chkOut0);
            this.groupBox2.Location = new System.Drawing.Point(571, 35);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(170, 144);
            this.groupBox2.TabIndex = 172;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "GPIO Output";
            // 
            // chkOut3
            // 
            this.chkOut3.AutoSize = true;
            this.chkOut3.Location = new System.Drawing.Point(6, 113);
            this.chkOut3.Name = "chkOut3";
            this.chkOut3.Size = new System.Drawing.Size(153, 23);
            this.chkOut3.TabIndex = 170;
            this.chkOut3.Text = "MCU_RESET_PLC";
            this.chkOut3.UseVisualStyleBackColor = true;
            this.chkOut3.CheckedChanged += new System.EventHandler(this.chkOut0_CheckedChanged);
            // 
            // chkOut2
            // 
            this.chkOut2.AutoSize = true;
            this.chkOut2.Location = new System.Drawing.Point(6, 84);
            this.chkOut2.Name = "chkOut2";
            this.chkOut2.Size = new System.Drawing.Size(144, 23);
            this.chkOut2.TabIndex = 170;
            this.chkOut2.Text = "MCU_LED_BLUE";
            this.chkOut2.UseVisualStyleBackColor = true;
            this.chkOut2.CheckedChanged += new System.EventHandler(this.chkOut0_CheckedChanged);
            // 
            // chkOut1
            // 
            this.chkOut1.AutoSize = true;
            this.chkOut1.Location = new System.Drawing.Point(6, 55);
            this.chkOut1.Name = "chkOut1";
            this.chkOut1.Size = new System.Drawing.Size(157, 23);
            this.chkOut1.TabIndex = 170;
            this.chkOut1.Text = "MCU_LED_GREEN";
            this.chkOut1.UseVisualStyleBackColor = true;
            this.chkOut1.CheckedChanged += new System.EventHandler(this.chkOut0_CheckedChanged);
            // 
            // chkOut0
            // 
            this.chkOut0.AutoSize = true;
            this.chkOut0.Location = new System.Drawing.Point(6, 26);
            this.chkOut0.Name = "chkOut0";
            this.chkOut0.Size = new System.Drawing.Size(137, 23);
            this.chkOut0.TabIndex = 170;
            this.chkOut0.Text = "MCU_LED_RED";
            this.chkOut0.UseVisualStyleBackColor = true;
            this.chkOut0.CheckedChanged += new System.EventHandler(this.chkOut0_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Navy;
            this.label8.Location = new System.Drawing.Point(153, 135);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 19);
            this.label8.TabIndex = 173;
            this.label8.Text = "---";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 135);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(92, 19);
            this.label10.TabIndex = 174;
            this.label10.Text = "UART Test:";
            this.label10.DoubleClick += new System.EventHandler(this.label10_DoubleClick);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Navy;
            this.label11.Location = new System.Drawing.Point(153, 173);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(30, 19);
            this.label11.TabIndex = 176;
            this.label11.Text = "---";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(9, 173);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 19);
            this.label16.TabIndex = 177;
            this.label16.Text = "AC Tony:";
            this.label16.DoubleClick += new System.EventHandler(this.label16_DoubleClick);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Navy;
            this.label17.Location = new System.Drawing.Point(153, 192);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(30, 19);
            this.label17.TabIndex = 178;
            this.label17.Text = "---";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(9, 192);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(132, 19);
            this.label18.TabIndex = 179;
            this.label18.Text = "Residual Current:";
            this.label18.DoubleClick += new System.EventHandler(this.label18_DoubleClick);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ForeColor = System.Drawing.Color.Navy;
            this.label19.Location = new System.Drawing.Point(153, 215);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(30, 19);
            this.label19.TabIndex = 180;
            this.label19.Text = "---";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(9, 215);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(113, 19);
            this.label20.TabIndex = 181;
            this.label20.Text = "Input Capture:";
            this.label20.DoubleClick += new System.EventHandler(this.label20_DoubleClick);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.textBox1.Location = new System.Drawing.Point(365, 186);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(376, 27);
            this.textBox1.TabIndex = 182;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DUT_MainAC11
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOutG);
            this.Controls.Add(this.label41);
            this.Controls.Add(this.label42);
            this.Controls.Add(this.label43);
            this.Controls.Add(this.label46);
            this.Controls.Add(this.label40);
            this.Controls.Add(this.btnOutL3);
            this.Controls.Add(this.btnOutL2);
            this.Controls.Add(this.btnOutL1);
            this.Controls.Add(this.btnOutN);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblFlashTest);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.lblADC);
            this.Controls.Add(this.label45);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.label44);
            this.Controls.Add(this.txtErr3);
            this.Controls.Add(this.txtErr2);
            this.Controls.Add(this.txtErr1);
            this.Controls.Add(this.lblTmprValue);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.cboCMDs);
            this.Controls.Add(this.chkHex);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblCANSerial);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.lblDUTSerial);
            this.Controls.Add(this.lblSoftwareVersion);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.lblHardwareVersion);
            this.Controls.Add(this.btnOnOff);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DUT_MainAC11";
            this.Size = new System.Drawing.Size(1194, 659);
            this.Load += new System.EventHandler(this.DUT_MainAC11_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblHardwareVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSoftwareVersion;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblDUTSerial;
        private System.Windows.Forms.Label lblCANSerial;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.CheckBox chkHex;
        private System.Windows.Forms.ComboBox cboCMDs;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblTmprValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtErr1;
        private System.Windows.Forms.TextBox txtErr2;
        private System.Windows.Forms.TextBox txtErr3;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblADC;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label lblFlashTest;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOutN;
        private System.Windows.Forms.Button btnOutL1;
        private System.Windows.Forms.Button btnOutL2;
        private System.Windows.Forms.Button btnOutL3;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Button btnOutG;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.CheckBox chkInp1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkInp0;
        private System.Windows.Forms.CheckBox chkInp3;
        private System.Windows.Forms.CheckBox chkInp2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkOut3;
        private System.Windows.Forms.CheckBox chkOut2;
        private System.Windows.Forms.CheckBox chkOut1;
        private System.Windows.Forms.CheckBox chkOut0;
        private System.Windows.Forms.Button btnReadAllInput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBox1;
    }
}
