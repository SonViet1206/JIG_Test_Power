namespace DefaultNS
{
    partial class DUT_PLC_AC
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
            this.btnOnOff = new System.Windows.Forms.Button();
            this.btnPing = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.cboCMDs = new System.Windows.Forms.ComboBox();
            this.txtCAN_LOG = new System.Windows.Forms.TextBox();
            this.txtTCP_LOG = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.txtErr1 = new System.Windows.Forms.TextBox();
            this.cboCANCmd = new System.Windows.Forms.ComboBox();
            this.btnCANSend = new System.Windows.Forms.Button();
            this.btnClearCAN = new System.Windows.Forms.Button();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.lblCANSerial = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.btnScan = new System.Windows.Forms.Button();
            this.lblDUTSerial = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSoftwareVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblHardwareVersion = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOnOff
            // 
            this.btnOnOff.BackColor = System.Drawing.Color.Black;
            this.btnOnOff.Location = new System.Drawing.Point(3, 0);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(26, 24);
            this.btnOnOff.TabIndex = 119;
            this.btnOnOff.UseVisualStyleBackColor = false;
            this.btnOnOff.Click += new System.EventHandler(this.btnOnOff_Click);
            // 
            // btnPing
            // 
            this.btnPing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPing.Location = new System.Drawing.Point(395, 3);
            this.btnPing.Name = "btnPing";
            this.btnPing.Size = new System.Drawing.Size(59, 28);
            this.btnPing.TabIndex = 121;
            this.btnPing.Text = "Ping";
            this.btnPing.UseVisualStyleBackColor = true;
            this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(483, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(35, 29);
            this.btnClear.TabIndex = 151;
            this.btnClear.Text = "X";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(406, 3);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(71, 29);
            this.btnSend.TabIndex = 150;
            this.btnSend.Text = "SEND";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // cboCMDs
            // 
            this.cboCMDs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCMDs.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.cboCMDs.FormattingEnabled = true;
            this.cboCMDs.Items.AddRange(new object[] {
            "check_condition",
            "start",
            "test_ddr",
            "test_emmc",
            "test_flash",
            "set_gpio/<number>/<value>",
            "get_gpio/<number>",
            "test_uart",
            "test_can",
            "test_i2c",
            "test_eth",
            "test_usb",
            "get_sw_version",
            "get_hw_version",
            "get_serial_number",
            "get_plc_ic_info",
            "stop"});
            this.cboCMDs.Location = new System.Drawing.Point(2, 4);
            this.cboCMDs.Name = "cboCMDs";
            this.cboCMDs.Size = new System.Drawing.Size(398, 27);
            this.cboCMDs.TabIndex = 149;
            // 
            // txtCAN_LOG
            // 
            this.txtCAN_LOG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCAN_LOG.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCAN_LOG.Location = new System.Drawing.Point(0, 206);
            this.txtCAN_LOG.Multiline = true;
            this.txtCAN_LOG.Name = "txtCAN_LOG";
            this.txtCAN_LOG.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCAN_LOG.Size = new System.Drawing.Size(520, 373);
            this.txtCAN_LOG.TabIndex = 148;
            // 
            // txtTCP_LOG
            // 
            this.txtTCP_LOG.BackColor = System.Drawing.Color.Black;
            this.txtTCP_LOG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTCP_LOG.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTCP_LOG.ForeColor = System.Drawing.Color.White;
            this.txtTCP_LOG.Location = new System.Drawing.Point(0, 204);
            this.txtTCP_LOG.Multiline = true;
            this.txtTCP_LOG.Name = "txtTCP_LOG";
            this.txtTCP_LOG.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTCP_LOG.Size = new System.Drawing.Size(520, 375);
            this.txtTCP_LOG.TabIndex = 153;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.txtCAN_LOG);
            this.panel1.Controls.Add(this.panel7);
            this.panel1.Controls.Add(this.panel6);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(524, 583);
            this.panel1.TabIndex = 154;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.txtErr1);
            this.panel7.Controls.Add(this.cboCANCmd);
            this.panel7.Controls.Add(this.btnCANSend);
            this.panel7.Controls.Add(this.btnClearCAN);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(0, 171);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(520, 35);
            this.panel7.TabIndex = 157;
            // 
            // txtErr1
            // 
            this.txtErr1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtErr1.Location = new System.Drawing.Point(3, 4);
            this.txtErr1.Name = "txtErr1";
            this.txtErr1.Size = new System.Drawing.Size(116, 27);
            this.txtErr1.TabIndex = 157;
            this.txtErr1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cboCANCmd
            // 
            this.cboCANCmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCANCmd.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.cboCANCmd.FormattingEnabled = true;
            this.cboCANCmd.Items.AddRange(new object[] {
            "01 06 01 00 00 00 B3 79",
            "01 01 03",
            "01 01 05",
            "01 02 05 01",
            "01 02 05 02",
            "01 02 07 00",
            "01 04 07 45 04 8B",
            "01 04 07 41 04 8B",
            "01 02 09 00",
            "01 02 09 01",
            "01 02 09 02",
            "01 02 09 03",
            "01 02 09 04",
            "01 01 0B",
            "01 01 0D"});
            this.cboCANCmd.Location = new System.Drawing.Point(125, 4);
            this.cboCANCmd.Name = "cboCANCmd";
            this.cboCANCmd.Size = new System.Drawing.Size(275, 27);
            this.cboCANCmd.TabIndex = 152;
            // 
            // btnCANSend
            // 
            this.btnCANSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCANSend.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCANSend.Location = new System.Drawing.Point(406, 3);
            this.btnCANSend.Name = "btnCANSend";
            this.btnCANSend.Size = new System.Drawing.Size(71, 29);
            this.btnCANSend.TabIndex = 153;
            this.btnCANSend.Text = "SEND";
            this.btnCANSend.UseVisualStyleBackColor = true;
            this.btnCANSend.Click += new System.EventHandler(this.btnCANSend_Click);
            // 
            // btnClearCAN
            // 
            this.btnClearCAN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCAN.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearCAN.Location = new System.Drawing.Point(483, 3);
            this.btnClearCAN.Name = "btnClearCAN";
            this.btnClearCAN.Size = new System.Drawing.Size(35, 29);
            this.btnClearCAN.TabIndex = 154;
            this.btnClearCAN.Text = "X";
            this.btnClearCAN.UseVisualStyleBackColor = true;
            this.btnClearCAN.Click += new System.EventHandler(this.btnClearCAN_Click);
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label7);
            this.panel6.Controls.Add(this.label8);
            this.panel6.Controls.Add(this.label24);
            this.panel6.Controls.Add(this.lblCANSerial);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(520, 171);
            this.panel6.TabIndex = 156;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(120, 19);
            this.label7.TabIndex = 157;
            this.label7.Text = "CAN Heartbeat:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Navy;
            this.label8.Location = new System.Drawing.Point(151, 28);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 19);
            this.label8.TabIndex = 156;
            this.label8.Text = "---";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(3, 9);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(91, 19);
            this.label24.TabIndex = 155;
            this.label24.Text = "CAN Serial:";
            // 
            // lblCANSerial
            // 
            this.lblCANSerial.AutoSize = true;
            this.lblCANSerial.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCANSerial.ForeColor = System.Drawing.Color.Navy;
            this.lblCANSerial.Location = new System.Drawing.Point(151, 9);
            this.lblCANSerial.Name = "lblCANSerial";
            this.lblCANSerial.Size = new System.Drawing.Size(30, 19);
            this.lblCANSerial.TabIndex = 154;
            this.lblCANSerial.Text = "---";
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lblCaption.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCaption.Location = new System.Drawing.Point(32, 2);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(180, 19);
            this.lblCaption.TabIndex = 153;
            this.lblCaption.Text = "DEVICE UNDER TEST";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gainsboro;
            this.panel2.Controls.Add(this.btnOnOff);
            this.panel2.Controls.Add(this.lblCaption);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1060, 24);
            this.panel2.TabIndex = 155;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1060, 589);
            this.tableLayoutPanel1.TabIndex = 156;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.txtTCP_LOG);
            this.panel3.Controls.Add(this.panel5);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(533, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(524, 583);
            this.panel3.TabIndex = 155;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.cboCMDs);
            this.panel5.Controls.Add(this.btnSend);
            this.panel5.Controls.Add(this.btnClear);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 171);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(520, 33);
            this.panel5.TabIndex = 155;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label5);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.btnScan);
            this.panel4.Controls.Add(this.lblDUTSerial);
            this.panel4.Controls.Add(this.label9);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.lblSoftwareVersion);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.lblHardwareVersion);
            this.panel4.Controls.Add(this.txtIP);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Controls.Add(this.btnPing);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(520, 171);
            this.panel4.TabIndex = 154;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Navy;
            this.label4.Location = new System.Drawing.Point(391, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 19);
            this.label4.TabIndex = 167;
            this.label4.Text = "---";
            // 
            // btnScan
            // 
            this.btnScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScan.Location = new System.Drawing.Point(458, 3);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(59, 28);
            this.btnScan.TabIndex = 165;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // lblDUTSerial
            // 
            this.lblDUTSerial.AutoSize = true;
            this.lblDUTSerial.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDUTSerial.ForeColor = System.Drawing.Color.Navy;
            this.lblDUTSerial.Location = new System.Drawing.Point(148, 43);
            this.lblDUTSerial.Name = "lblDUTSerial";
            this.lblDUTSerial.Size = new System.Drawing.Size(30, 19);
            this.lblDUTSerial.TabIndex = 163;
            this.lblDUTSerial.Text = "---";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(116, 19);
            this.label9.TabIndex = 164;
            this.label9.Text = "Serial Number:";
            this.label9.DoubleClick += new System.EventHandler(this.label9_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 19);
            this.label1.TabIndex = 162;
            this.label1.Text = "Hardware Version:";
            this.label1.DoubleClick += new System.EventHandler(this.label1_DoubleClick);
            // 
            // lblSoftwareVersion
            // 
            this.lblSoftwareVersion.AutoSize = true;
            this.lblSoftwareVersion.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSoftwareVersion.ForeColor = System.Drawing.Color.Navy;
            this.lblSoftwareVersion.Location = new System.Drawing.Point(148, 62);
            this.lblSoftwareVersion.Name = "lblSoftwareVersion";
            this.lblSoftwareVersion.Size = new System.Drawing.Size(30, 19);
            this.lblSoftwareVersion.TabIndex = 159;
            this.lblSoftwareVersion.Text = "---";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 19);
            this.label3.TabIndex = 160;
            this.label3.Text = "Software Version:";
            this.label3.DoubleClick += new System.EventHandler(this.label3_DoubleClick);
            // 
            // lblHardwareVersion
            // 
            this.lblHardwareVersion.AutoSize = true;
            this.lblHardwareVersion.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHardwareVersion.ForeColor = System.Drawing.Color.Navy;
            this.lblHardwareVersion.Location = new System.Drawing.Point(148, 81);
            this.lblHardwareVersion.Name = "lblHardwareVersion";
            this.lblHardwareVersion.Size = new System.Drawing.Size(30, 19);
            this.lblHardwareVersion.TabIndex = 161;
            this.lblHardwareVersion.Text = "---";
            // 
            // txtIP
            // 
            this.txtIP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIP.Location = new System.Drawing.Point(100, 4);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(289, 27);
            this.txtIP.TabIndex = 158;
            this.txtIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 19);
            this.label2.TabIndex = 157;
            this.label2.Text = "IP Address:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Navy;
            this.label6.Location = new System.Drawing.Point(148, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 19);
            this.label6.TabIndex = 168;
            this.label6.Text = "---";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 19);
            this.label5.TabIndex = 169;
            this.label5.Text = "Booting:";
            // 
            // DUT_PLC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DUT_PLC";
            this.Size = new System.Drawing.Size(1060, 613);
            this.Load += new System.EventHandler(this.Control_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Button btnPing;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox cboCMDs;
        private System.Windows.Forms.TextBox txtCAN_LOG;
        private System.Windows.Forms.TextBox txtTCP_LOG;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblCANSerial;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.ComboBox cboCANCmd;
        private System.Windows.Forms.Button btnCANSend;
        private System.Windows.Forms.Button btnClearCAN;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSoftwareVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblHardwareVersion;
        private System.Windows.Forms.Label lblDUTSerial;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtErr1;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}
