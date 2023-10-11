partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.timer1s = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.CustomStatusStrip1 = new System.Windows.Forms.CustomStatusStrip();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel7 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblRelaysBoardPortName = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnMaximize = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.lblRole = new System.Windows.Forms.Label();
            this.lblBuildInfo = new System.Windows.Forms.Label();
            this.btnHome = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.btnclose = new System.Windows.Forms.Button();
            this.panel25 = new System.Windows.Forms.Panel();
            this.dgvTestList = new System.Windows.Forms.DataGridView();
            this.TestOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TestName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CheckValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReadValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ElapseTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BeginTest = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customToolStrip4 = new System.Windows.Forms.CustomToolStrip();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnReload = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lstLogWindow = new System.Windows.Forms.ListBox();
            this.customToolStrip3 = new System.Windows.Forms.CustomToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.btnClearLog = new System.Windows.Forms.ToolStripButton();
            this.txtTotalSteep = new System.Windows.Forms.TextBox();
            this.picTesting = new System.Windows.Forms.PictureBox();
            this.lblTestTime = new System.Windows.Forms.Label();
            this.btnMode1 = new System.Windows.Forms.Button();
            this.btnMode3 = new System.Windows.Forms.Button();
            this.btnMode2 = new System.Windows.Forms.Button();
            this.txtTestSteep = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTestName = new System.Windows.Forms.TextBox();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.DUT = new DefaultNS.DUT_RelayCtrl();
            this.panel17 = new System.Windows.Forms.Panel();
            this.panel21 = new System.Windows.Forms.Panel();
            this.lbMeshStatus = new System.Windows.Forms.Label();
            this.ledResult = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label12 = new System.Windows.Forms.Label();
            this.btnReady = new System.Windows.Forms.Button();
            this.cboOperator = new System.Windows.Forms.ComboBox();
            this.numTemprature = new System.Windows.Forms.NumericUpDown();
            this.txtSerialNo = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panelBody = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.relayBoards1 = new DefaultNS.RelayBoards();
            this.autonicsMT41 = new DefaultNS.autonicsMT4();
            this.spM931 = new DefaultNS.SPM93();
            this.powerBoard1 = new DefaultNS.PowerBoard();
            this.lbPassRate = new System.Windows.Forms.Label();
            this.CustomStatusStrip1.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel25.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTestList)).BeginInit();
            this.customToolStrip4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.customToolStrip3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTesting)).BeginInit();
            this.panelHeader.SuspendLayout();
            this.panel17.SuspendLayout();
            this.panel21.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTemprature)).BeginInit();
            this.panelBody.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1s
            // 
            this.timer1s.Interval = 1000;
            this.timer1s.Tick += new System.EventHandler(this.timer1s_Tick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Excel Files|*.xls,*.xlsx";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.CheckFileExists = true;
            // 
            // CustomStatusStrip1
            // 
            this.CustomStatusStrip1.ForeColor = System.Drawing.Color.Black;
            this.CustomStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel2,
            this.lblTime,
            this.toolStripStatusLabel7,
            this.lblRelaysBoardPortName});
            this.CustomStatusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.CustomStatusStrip1.Location = new System.Drawing.Point(0, 678);
            this.CustomStatusStrip1.Name = "CustomStatusStrip1";
            this.CustomStatusStrip1.Size = new System.Drawing.Size(1366, 22);
            this.CustomStatusStrip1.StatusStripForeColor = System.Drawing.Color.Black;
            this.CustomStatusStrip1.StatusStripGradientBegin = System.Drawing.Color.LightBlue;
            this.CustomStatusStrip1.StatusStripGradientEnd = System.Drawing.Color.LightYellow;
            this.CustomStatusStrip1.TabIndex = 120;
            this.CustomStatusStrip1.Text = "CustomStatusStrip1";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.ForeColor = System.Drawing.Color.Black;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(36, 17);
            this.toolStripStatusLabel3.Text = "Time:";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel2.ForeColor = System.Drawing.Color.Black;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(28, 17);
            this.toolStripStatusLabel2.Text = "OK";
            // 
            // lblTime
            // 
            this.lblTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.ForeColor = System.Drawing.Color.Black;
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(86, 17);
            this.lblTime.Text = "000-00:00:00";
            // 
            // toolStripStatusLabel7
            // 
            this.toolStripStatusLabel7.AccessibleDescription = "Status";
            this.toolStripStatusLabel7.ForeColor = System.Drawing.Color.Black;
            this.toolStripStatusLabel7.Name = "toolStripStatusLabel7";
            this.toolStripStatusLabel7.Size = new System.Drawing.Size(12, 17);
            this.toolStripStatusLabel7.Text = "-";
            // 
            // lblRelaysBoardPortName
            // 
            this.lblRelaysBoardPortName.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblRelaysBoardPortName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRelaysBoardPortName.ForeColor = System.Drawing.Color.Black;
            this.lblRelaysBoardPortName.Name = "lblRelaysBoardPortName";
            this.lblRelaysBoardPortName.Size = new System.Drawing.Size(28, 17);
            this.lblRelaysBoardPortName.Text = "OK";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.ForestGreen;
            this.panelTop.Controls.Add(this.lblCaption);
            this.panelTop.Controls.Add(this.pictureBox2);
            this.panelTop.Controls.Add(this.btnMaximize);
            this.panelTop.Controls.Add(this.btnMinimize);
            this.panelTop.Controls.Add(this.lblRole);
            this.panelTop.Controls.Add(this.lblBuildInfo);
            this.panelTop.Controls.Add(this.btnHome);
            this.panelTop.Controls.Add(this.btnConfig);
            this.panelTop.Controls.Add(this.btnclose);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1366, 39);
            this.panelTop.TabIndex = 119;
            this.panelTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseDown);
            this.panelTop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseMove);
            this.panelTop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseUp);
            // 
            // lblCaption
            // 
            this.lblCaption.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.White;
            this.lblCaption.Location = new System.Drawing.Point(373, 3);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(599, 33);
            this.lblCaption.TabIndex = 13;
            this.lblCaption.Text = "REALAY CONTROL BOARD FUNCTION TEST";
            this.lblCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseDown);
            this.lblCaption.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseMove);
            this.lblCaption.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseUp);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(1200, 1);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(44, 37);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 12;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseDown);
            this.pictureBox2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseMove);
            this.pictureBox2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseUp);
            // 
            // btnMaximize
            // 
            this.btnMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMaximize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnMaximize.FlatAppearance.BorderSize = 0;
            this.btnMaximize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaximize.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnMaximize.Image = ((System.Drawing.Image)(resources.GetObject("btnMaximize.Image")));
            this.btnMaximize.Location = new System.Drawing.Point(1289, 4);
            this.btnMaximize.Name = "btnMaximize";
            this.btnMaximize.Size = new System.Drawing.Size(32, 32);
            this.btnMaximize.TabIndex = 11;
            this.btnMaximize.UseVisualStyleBackColor = true;
            this.btnMaximize.Click += new System.EventHandler(this.btnMaximize_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnMinimize.Image = ((System.Drawing.Image)(resources.GetObject("btnMinimize.Image")));
            this.btnMinimize.Location = new System.Drawing.Point(1249, 5);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(32, 32);
            this.btnMinimize.TabIndex = 9;
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // lblRole
            // 
            this.lblRole.AutoSize = true;
            this.lblRole.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRole.ForeColor = System.Drawing.Color.White;
            this.lblRole.Location = new System.Drawing.Point(94, 20);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(92, 13);
            this.lblRole.TabIndex = 6;
            this.lblRole.Text = "www.pmtt.com.vn";
            this.lblRole.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseDown);
            this.lblRole.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseMove);
            this.lblRole.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseUp);
            // 
            // lblBuildInfo
            // 
            this.lblBuildInfo.AutoSize = true;
            this.lblBuildInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBuildInfo.ForeColor = System.Drawing.Color.White;
            this.lblBuildInfo.Location = new System.Drawing.Point(95, 5);
            this.lblBuildInfo.Name = "lblBuildInfo";
            this.lblBuildInfo.Size = new System.Drawing.Size(71, 16);
            this.lblBuildInfo.TabIndex = 6;
            this.lblBuildInfo.Text = "Build: 1.0";
            this.lblBuildInfo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseDown);
            this.lblBuildInfo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseMove);
            this.lblBuildInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelTop_MouseUp);
            // 
            // btnHome
            // 
            this.btnHome.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnHome.BackgroundImage")));
            this.btnHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnHome.FlatAppearance.BorderSize = 0;
            this.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHome.ForeColor = System.Drawing.Color.Transparent;
            this.btnHome.Location = new System.Drawing.Point(7, 4);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(32, 32);
            this.btnHome.TabIndex = 4;
            this.btnHome.UseVisualStyleBackColor = true;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // btnConfig
            // 
            this.btnConfig.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnConfig.FlatAppearance.BorderSize = 0;
            this.btnConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfig.ForeColor = System.Drawing.Color.Transparent;
            this.btnConfig.Image = ((System.Drawing.Image)(resources.GetObject("btnConfig.Image")));
            this.btnConfig.Location = new System.Drawing.Point(46, 4);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(44, 32);
            this.btnConfig.TabIndex = 4;
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // btnclose
            // 
            this.btnclose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnclose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnclose.FlatAppearance.BorderSize = 0;
            this.btnclose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnclose.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnclose.Image = ((System.Drawing.Image)(resources.GetObject("btnclose.Image")));
            this.btnclose.Location = new System.Drawing.Point(1327, 4);
            this.btnclose.Name = "btnclose";
            this.btnclose.Size = new System.Drawing.Size(32, 32);
            this.btnclose.TabIndex = 4;
            this.btnclose.UseVisualStyleBackColor = true;
            this.btnclose.Click += new System.EventHandler(this.btnclose_Click);
            // 
            // panel25
            // 
            this.panel25.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel25.Controls.Add(this.dgvTestList);
            this.panel25.Controls.Add(this.customToolStrip4);
            this.panel25.Controls.Add(this.panel2);
            this.panel25.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel25.Font = new System.Drawing.Font("Tahoma", 12F);
            this.panel25.Location = new System.Drawing.Point(0, 240);
            this.panel25.Name = "panel25";
            this.panel25.Size = new System.Drawing.Size(628, 395);
            this.panel25.TabIndex = 121;
            // 
            // dgvTestList
            // 
            this.dgvTestList.AllowUserToAddRows = false;
            this.dgvTestList.AllowUserToDeleteRows = false;
            this.dgvTestList.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.LemonChiffon;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTestList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvTestList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTestList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TestOrder,
            this.TestName,
            this.Description,
            this.CheckValue,
            this.ReadValue,
            this.ElapseTime,
            this.Result,
            this.BeginTest});
            this.dgvTestList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTestList.GridColor = System.Drawing.Color.Gainsboro;
            this.dgvTestList.Location = new System.Drawing.Point(0, 25);
            this.dgvTestList.MultiSelect = false;
            this.dgvTestList.Name = "dgvTestList";
            this.dgvTestList.ReadOnly = true;
            this.dgvTestList.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTestList.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvTestList.RowHeadersWidth = 4;
            this.dgvTestList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvTestList.Size = new System.Drawing.Size(624, 276);
            this.dgvTestList.TabIndex = 123;
            this.dgvTestList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTestList_CellDoubleClick);
            // 
            // TestOrder
            // 
            this.TestOrder.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.TestOrder.DefaultCellStyle = dataGridViewCellStyle2;
            this.TestOrder.HeaderText = "#";
            this.TestOrder.Name = "TestOrder";
            this.TestOrder.ReadOnly = true;
            this.TestOrder.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.TestOrder.Width = 40;
            // 
            // TestName
            // 
            this.TestName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.TestName.HeaderText = "TEST NAME";
            this.TestName.Name = "TestName";
            this.TestName.ReadOnly = true;
            this.TestName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.TestName.Width = 101;
            // 
            // Description
            // 
            this.Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Description.HeaderText = "DESCRIPTION";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // CheckValue
            // 
            this.CheckValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.CheckValue.DefaultCellStyle = dataGridViewCellStyle3;
            this.CheckValue.HeaderText = "CHECK VALUE";
            this.CheckValue.Name = "CheckValue";
            this.CheckValue.ReadOnly = true;
            this.CheckValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.CheckValue.Width = 106;
            // 
            // ReadValue
            // 
            this.ReadValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ReadValue.DefaultCellStyle = dataGridViewCellStyle4;
            this.ReadValue.HeaderText = "READ VALUE";
            this.ReadValue.Name = "ReadValue";
            this.ReadValue.ReadOnly = true;
            this.ReadValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ReadValue.Width = 99;
            // 
            // ElapseTime
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ElapseTime.DefaultCellStyle = dataGridViewCellStyle5;
            this.ElapseTime.HeaderText = "ELAPSE";
            this.ElapseTime.Name = "ElapseTime";
            this.ElapseTime.ReadOnly = true;
            this.ElapseTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ElapseTime.Width = 80;
            // 
            // Result
            // 
            this.Result.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Result.DefaultCellStyle = dataGridViewCellStyle6;
            this.Result.HeaderText = "RESULT";
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            this.Result.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Result.Width = 80;
            // 
            // BeginTest
            // 
            this.BeginTest.HeaderText = "START TEST";
            this.BeginTest.Name = "BeginTest";
            this.BeginTest.ReadOnly = true;
            this.BeginTest.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.BeginTest.Visible = false;
            // 
            // customToolStrip4
            // 
            this.customToolStrip4.ForeColor = System.Drawing.Color.Black;
            this.customToolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel5,
            this.toolStripSeparator3,
            this.btnReload,
            this.toolStripSeparator7});
            this.customToolStrip4.Location = new System.Drawing.Point(0, 0);
            this.customToolStrip4.Name = "customToolStrip4";
            this.customToolStrip4.Size = new System.Drawing.Size(624, 25);
            this.customToolStrip4.TabIndex = 79;
            this.customToolStrip4.Text = "customToolStrip4";
            this.customToolStrip4.ToolStripBorder = System.Drawing.Color.Black;
            this.customToolStrip4.ToolStripContentPanelGradientBegin = System.Drawing.Color.Silver;
            this.customToolStrip4.ToolStripContentPanelGradientEnd = System.Drawing.Color.LightGray;
            this.customToolStrip4.ToolStripDropDownBackground = System.Drawing.Color.LightGray;
            this.customToolStrip4.ToolStripForeColor = System.Drawing.Color.Black;
            this.customToolStrip4.ToolStripGradientBegin = System.Drawing.Color.LightBlue;
            this.customToolStrip4.ToolStripGradientEnd = System.Drawing.Color.LightYellow;
            this.customToolStrip4.ToolStripGradientMiddle = System.Drawing.Color.LightGray;
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(88, 22);
            this.toolStripLabel5.Text = "TEST PROCESS";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnReload
            // 
            this.btnReload.Image = ((System.Drawing.Image)(resources.GetObject("btnReload.Image")));
            this.btnReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(63, 22);
            this.btnReload.Text = "Reload";
            this.btnReload.ToolTipText = "Đọc lại danh sách từ cơ sở dữ liệu";
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.lstLogWindow);
            this.panel2.Controls.Add(this.customToolStrip3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 301);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(624, 90);
            this.panel2.TabIndex = 122;
            // 
            // lstLogWindow
            // 
            this.lstLogWindow.BackColor = System.Drawing.Color.White;
            this.lstLogWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstLogWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLogWindow.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstLogWindow.FormattingEnabled = true;
            this.lstLogWindow.ItemHeight = 21;
            this.lstLogWindow.Location = new System.Drawing.Point(0, 25);
            this.lstLogWindow.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstLogWindow.Name = "lstLogWindow";
            this.lstLogWindow.Size = new System.Drawing.Size(624, 65);
            this.lstLogWindow.TabIndex = 265;
            // 
            // customToolStrip3
            // 
            this.customToolStrip3.ForeColor = System.Drawing.Color.Black;
            this.customToolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.btnClearLog});
            this.customToolStrip3.Location = new System.Drawing.Point(0, 0);
            this.customToolStrip3.Name = "customToolStrip3";
            this.customToolStrip3.Size = new System.Drawing.Size(624, 25);
            this.customToolStrip3.TabIndex = 34;
            this.customToolStrip3.Text = "customToolStrip3";
            this.customToolStrip3.ToolStripBorder = System.Drawing.Color.Black;
            this.customToolStrip3.ToolStripContentPanelGradientBegin = System.Drawing.Color.Silver;
            this.customToolStrip3.ToolStripContentPanelGradientEnd = System.Drawing.Color.LightGray;
            this.customToolStrip3.ToolStripDropDownBackground = System.Drawing.Color.LightGray;
            this.customToolStrip3.ToolStripForeColor = System.Drawing.Color.Black;
            this.customToolStrip3.ToolStripGradientBegin = System.Drawing.Color.LightBlue;
            this.customToolStrip3.ToolStripGradientEnd = System.Drawing.Color.LightYellow;
            this.customToolStrip3.ToolStripGradientMiddle = System.Drawing.Color.LightGray;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(70, 22);
            this.toolStripLabel1.Text = "EVENT LOG";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnClearLog.Image = ((System.Drawing.Image)(resources.GetObject("btnClearLog.Image")));
            this.btnClearLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(54, 22);
            this.btnClearLog.Text = "Clear";
            this.btnClearLog.ToolTipText = "Xóa kết quả thử nghiệm đang chọn";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // txtTotalSteep
            // 
            this.txtTotalSteep.BackColor = System.Drawing.Color.White;
            this.txtTotalSteep.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtTotalSteep.Location = new System.Drawing.Point(143, 181);
            this.txtTotalSteep.Name = "txtTotalSteep";
            this.txtTotalSteep.ReadOnly = true;
            this.txtTotalSteep.Size = new System.Drawing.Size(59, 27);
            this.txtTotalSteep.TabIndex = 138;
            this.txtTotalSteep.Text = "-";
            this.txtTotalSteep.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // picTesting
            // 
            this.picTesting.Image = ((System.Drawing.Image)(resources.GetObject("picTesting.Image")));
            this.picTesting.Location = new System.Drawing.Point(210, 157);
            this.picTesting.Name = "picTesting";
            this.picTesting.Size = new System.Drawing.Size(48, 48);
            this.picTesting.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picTesting.TabIndex = 137;
            this.picTesting.TabStop = false;
            this.picTesting.Visible = false;
            // 
            // lblTestTime
            // 
            this.lblTestTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTestTime.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTestTime.Location = new System.Drawing.Point(4, 242);
            this.lblTestTime.Name = "lblTestTime";
            this.lblTestTime.Size = new System.Drawing.Size(123, 36);
            this.lblTestTime.TabIndex = 136;
            this.lblTestTime.Text = "00:00";
            this.lblTestTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMode1
            // 
            this.btnMode1.BackColor = System.Drawing.Color.DimGray;
            this.btnMode1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMode1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMode1.ForeColor = System.Drawing.Color.White;
            this.btnMode1.Location = new System.Drawing.Point(3, 3);
            this.btnMode1.Name = "btnMode1";
            this.btnMode1.Size = new System.Drawing.Size(80, 34);
            this.btnMode1.TabIndex = 133;
            this.btnMode1.Text = "MANUAL";
            this.btnMode1.UseVisualStyleBackColor = false;
            this.btnMode1.Click += new System.EventHandler(this.btnMode_Click);
            // 
            // btnMode3
            // 
            this.btnMode3.BackColor = System.Drawing.Color.DimGray;
            this.btnMode3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMode3.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMode3.ForeColor = System.Drawing.Color.White;
            this.btnMode3.Location = new System.Drawing.Point(177, 3);
            this.btnMode3.Name = "btnMode3";
            this.btnMode3.Size = new System.Drawing.Size(81, 34);
            this.btnMode3.TabIndex = 134;
            this.btnMode3.Text = "ALL NEXT";
            this.btnMode3.UseVisualStyleBackColor = false;
            this.btnMode3.Click += new System.EventHandler(this.btnMode_Click);
            // 
            // btnMode2
            // 
            this.btnMode2.BackColor = System.Drawing.Color.Red;
            this.btnMode2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMode2.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMode2.ForeColor = System.Drawing.Color.White;
            this.btnMode2.Location = new System.Drawing.Point(89, 3);
            this.btnMode2.Name = "btnMode2";
            this.btnMode2.Size = new System.Drawing.Size(82, 34);
            this.btnMode2.TabIndex = 133;
            this.btnMode2.Text = "OK AUTO";
            this.btnMode2.UseVisualStyleBackColor = false;
            this.btnMode2.Click += new System.EventHandler(this.btnMode_Click);
            // 
            // txtTestSteep
            // 
            this.txtTestSteep.BackColor = System.Drawing.Color.White;
            this.txtTestSteep.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtTestSteep.Location = new System.Drawing.Point(61, 181);
            this.txtTestSteep.Name = "txtTestSteep";
            this.txtTestSteep.ReadOnly = true;
            this.txtTestSteep.Size = new System.Drawing.Size(59, 27);
            this.txtTestSteep.TabIndex = 86;
            this.txtTestSteep.Text = "-";
            this.txtTestSteep.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(124, 184);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(12, 13);
            this.label6.TabIndex = 84;
            this.label6.Text = "/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 187);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 16);
            this.label1.TabIndex = 84;
            this.label1.Text = "STEP:";
            // 
            // txtTestName
            // 
            this.txtTestName.BackColor = System.Drawing.Color.White;
            this.txtTestName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtTestName.Location = new System.Drawing.Point(4, 210);
            this.txtTestName.Name = "txtTestName";
            this.txtTestName.ReadOnly = true;
            this.txtTestName.Size = new System.Drawing.Size(254, 27);
            this.txtTestName.TabIndex = 85;
            this.txtTestName.Text = "-";
            // 
            // chkAutoSave
            // 
            this.chkAutoSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.Checked = true;
            this.chkAutoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoSave.Enabled = false;
            this.chkAutoSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAutoSave.Location = new System.Drawing.Point(6, 328);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(174, 20);
            this.chkAutoSave.TabIndex = 83;
            this.chkAutoSave.Text = "Auto save when last step";
            this.chkAutoSave.UseVisualStyleBackColor = true;
            this.chkAutoSave.CheckedChanged += new System.EventHandler(this.chkAutoSave_CheckedChanged);
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.SystemColors.Control;
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.Enabled = false;
            this.btnNext.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNext.Image = ((System.Drawing.Image)(resources.GetObject("btnNext.Image")));
            this.btnNext.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNext.Location = new System.Drawing.Point(133, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(125, 40);
            this.btnNext.TabIndex = 81;
            this.btnNext.Text = "NEXT";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnTest
            // 
            this.btnTest.BackColor = System.Drawing.SystemColors.Control;
            this.btnTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTest.Enabled = false;
            this.btnTest.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTest.Image = ((System.Drawing.Image)(resources.GetObject("btnTest.Image")));
            this.btnTest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTest.Location = new System.Drawing.Point(3, 3);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(124, 40);
            this.btnTest.TabIndex = 81;
            this.btnTest.Text = "TEST";
            this.btnTest.UseVisualStyleBackColor = false;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(4, 354);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(254, 34);
            this.btnSave.TabIndex = 81;
            this.btnSave.Text = "SAVE";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click1);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.SystemColors.Control;
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStop.Location = new System.Drawing.Point(133, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(125, 40);
            this.btnStop.TabIndex = 126;
            this.btnStop.Text = "STOP";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // panelHeader
            // 
            this.panelHeader.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelHeader.BackgroundImage")));
            this.panelHeader.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelHeader.Controls.Add(this.DUT);
            this.panelHeader.Controls.Add(this.panel17);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(893, 240);
            this.panelHeader.TabIndex = 124;
            // 
            // DUT
            // 
            this.DUT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.DUT.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DUT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DUT.Font = new System.Drawing.Font("Tahoma", 12F);
            this.DUT.Location = new System.Drawing.Point(0, 0);
            this.DUT.Name = "DUT";
            this.DUT.Size = new System.Drawing.Size(628, 240);
            this.DUT.TabIndex = 131;
            // 
            // panel17
            // 
            this.panel17.Controls.Add(this.panel21);
            this.panel17.Controls.Add(this.panel1);
            this.panel17.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel17.Location = new System.Drawing.Point(628, 0);
            this.panel17.Name = "panel17";
            this.panel17.Size = new System.Drawing.Size(265, 240);
            this.panel17.TabIndex = 120;
            // 
            // panel21
            // 
            this.panel21.BackColor = System.Drawing.Color.Black;
            this.panel21.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel21.Controls.Add(this.lbMeshStatus);
            this.panel21.Controls.Add(this.ledResult);
            this.panel21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel21.Location = new System.Drawing.Point(0, 0);
            this.panel21.Name = "panel21";
            this.panel21.Size = new System.Drawing.Size(265, 196);
            this.panel21.TabIndex = 36;
            // 
            // lbMeshStatus
            // 
            this.lbMeshStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbMeshStatus.BackColor = System.Drawing.Color.DarkSlateGray;
            this.lbMeshStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMeshStatus.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMeshStatus.ForeColor = System.Drawing.SystemColors.Control;
            this.lbMeshStatus.Location = new System.Drawing.Point(5, 151);
            this.lbMeshStatus.Name = "lbMeshStatus";
            this.lbMeshStatus.Size = new System.Drawing.Size(251, 37);
            this.lbMeshStatus.TabIndex = 143;
            this.lbMeshStatus.Text = "MES";
            this.lbMeshStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ledResult
            // 
            this.ledResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ledResult.BackColor = System.Drawing.Color.DarkSlateGray;
            this.ledResult.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ledResult.ForeColor = System.Drawing.Color.Yellow;
            this.ledResult.Location = new System.Drawing.Point(5, 4);
            this.ledResult.Name = "ledResult";
            this.ledResult.Size = new System.Drawing.Size(251, 143);
            this.ledResult.TabIndex = 59;
            this.ledResult.Text = "---";
            this.ledResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 196);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(265, 44);
            this.panel1.TabIndex = 135;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.Controls.Add(this.btnMode3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnMode1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnMode2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(261, 40);
            this.tableLayoutPanel1.TabIndex = 136;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(0, 4);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(63, 19);
            this.label12.TabIndex = 70;
            this.label12.Text = "User ID";
            // 
            // btnReady
            // 
            this.btnReady.BackColor = System.Drawing.SystemColors.Control;
            this.btnReady.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReady.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReady.Image = ((System.Drawing.Image)(resources.GetObject("btnReady.Image")));
            this.btnReady.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReady.Location = new System.Drawing.Point(3, 3);
            this.btnReady.Name = "btnReady";
            this.btnReady.Size = new System.Drawing.Size(124, 40);
            this.btnReady.TabIndex = 125;
            this.btnReady.Text = "   START";
            this.btnReady.UseVisualStyleBackColor = false;
            this.btnReady.Click += new System.EventHandler(this.btnReady_Click);
            // 
            // cboOperator
            // 
            this.cboOperator.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.cboOperator.FormattingEnabled = true;
            this.cboOperator.Location = new System.Drawing.Point(4, 26);
            this.cboOperator.MaxLength = 4;
            this.cboOperator.Name = "cboOperator";
            this.cboOperator.Size = new System.Drawing.Size(183, 27);
            this.cboOperator.TabIndex = 0;
            this.cboOperator.Text = "1196";
            this.cboOperator.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboOperator_KeyPress);
            // 
            // numTemprature
            // 
            this.numTemprature.DecimalPlaces = 1;
            this.numTemprature.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.numTemprature.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numTemprature.Location = new System.Drawing.Point(193, 26);
            this.numTemprature.Name = "numTemprature";
            this.numTemprature.Size = new System.Drawing.Size(65, 27);
            this.numTemprature.TabIndex = 124;
            this.numTemprature.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numTemprature.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.BackColor = System.Drawing.Color.White;
            this.txtSerialNo.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.txtSerialNo.Location = new System.Drawing.Point(4, 78);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.Size = new System.Drawing.Size(254, 27);
            this.txtSerialNo.TabIndex = 72;
            this.txtSerialNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtSerialNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSerialNo_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(190, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 19);
            this.label3.TabIndex = 123;
            this.label3.Text = "T (°C):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(0, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 19);
            this.label4.TabIndex = 70;
            this.label4.Text = "QR Serial";
            // 
            // panelBody
            // 
            this.panelBody.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelBody.Controls.Add(this.panel25);
            this.panelBody.Controls.Add(this.panel5);
            this.panelBody.Controls.Add(this.panelHeader);
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Location = new System.Drawing.Point(469, 39);
            this.panelBody.Name = "panelBody";
            this.panelBody.Size = new System.Drawing.Size(897, 639);
            this.panelBody.TabIndex = 125;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel5.Controls.Add(this.lbPassRate);
            this.panel5.Controls.Add(this.tableLayoutPanel3);
            this.panel5.Controls.Add(this.txtTotalSteep);
            this.panel5.Controls.Add(this.tableLayoutPanel2);
            this.panel5.Controls.Add(this.chkAutoSave);
            this.panel5.Controls.Add(this.btnSave);
            this.panel5.Controls.Add(this.picTesting);
            this.panel5.Controls.Add(this.lblTestTime);
            this.panel5.Controls.Add(this.label12);
            this.panel5.Controls.Add(this.txtTestSteep);
            this.panel5.Controls.Add(this.label6);
            this.panel5.Controls.Add(this.cboOperator);
            this.panel5.Controls.Add(this.label1);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.txtTestName);
            this.panel5.Controls.Add(this.numTemprature);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Controls.Add(this.txtSerialNo);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel5.Location = new System.Drawing.Point(628, 240);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(265, 395);
            this.panel5.TabIndex = 125;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.btnTest, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnNext, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 285);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(261, 46);
            this.tableLayoutPanel3.TabIndex = 139;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnReady, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStop, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 111);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(261, 46);
            this.tableLayoutPanel2.TabIndex = 127;
            // 
            // panelLeft
            // 
            this.panelLeft.AutoScroll = true;
            this.panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelLeft.Controls.Add(this.relayBoards1);
            this.panelLeft.Controls.Add(this.autonicsMT41);
            this.panelLeft.Controls.Add(this.spM931);
            this.panelLeft.Controls.Add(this.powerBoard1);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 39);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(469, 639);
            this.panelLeft.TabIndex = 126;
            // 
            // relayBoards1
            // 
            this.relayBoards1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.relayBoards1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.relayBoards1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.relayBoards1.Font = new System.Drawing.Font("Tahoma", 12F);
            this.relayBoards1.Location = new System.Drawing.Point(0, 260);
            this.relayBoards1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.relayBoards1.Name = "relayBoards1";
            this.relayBoards1.Size = new System.Drawing.Size(465, 375);
            this.relayBoards1.TabIndex = 0;
            // 
            // autonicsMT41
            // 
            this.autonicsMT41.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.autonicsMT41.Dock = System.Windows.Forms.DockStyle.Top;
            this.autonicsMT41.Font = new System.Drawing.Font("Tahoma", 12F);
            this.autonicsMT41.Location = new System.Drawing.Point(0, 185);
            this.autonicsMT41.Name = "autonicsMT41";
            this.autonicsMT41.Size = new System.Drawing.Size(465, 75);
            this.autonicsMT41.TabIndex = 132;
            // 
            // spM931
            // 
            this.spM931.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.spM931.Dock = System.Windows.Forms.DockStyle.Top;
            this.spM931.Font = new System.Drawing.Font("Tahoma", 12F);
            this.spM931.Location = new System.Drawing.Point(0, 92);
            this.spM931.Name = "spM931";
            this.spM931.Size = new System.Drawing.Size(465, 93);
            this.spM931.TabIndex = 131;
            // 
            // powerBoard1
            // 
            this.powerBoard1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.powerBoard1.Dock = System.Windows.Forms.DockStyle.Top;
            this.powerBoard1.Font = new System.Drawing.Font("Tahoma", 12F);
            this.powerBoard1.Location = new System.Drawing.Point(0, 0);
            this.powerBoard1.Name = "powerBoard1";
            this.powerBoard1.Size = new System.Drawing.Size(465, 92);
            this.powerBoard1.TabIndex = 127;
            this.powerBoard1.Load += new System.EventHandler(this.powerBoard1_Load);
            // 
            // lbPassRate
            // 
            this.lbPassRate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbPassRate.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPassRate.Location = new System.Drawing.Point(133, 242);
            this.lbPassRate.Name = "lbPassRate";
            this.lbPassRate.Size = new System.Drawing.Size(125, 36);
            this.lbPassRate.TabIndex = 145;
            this.lbPassRate.Text = "0/0";
            this.lbPassRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1366, 700);
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.CustomStatusStrip1);
            this.Controls.Add(this.panelTop);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(640, 517);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.CustomStatusStrip1.ResumeLayout(false);
            this.CustomStatusStrip1.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel25.ResumeLayout(false);
            this.panel25.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTestList)).EndInit();
            this.customToolStrip4.ResumeLayout(false);
            this.customToolStrip4.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.customToolStrip3.ResumeLayout(false);
            this.customToolStrip3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTesting)).EndInit();
            this.panelHeader.ResumeLayout(false);
            this.panel17.ResumeLayout(false);
            this.panel21.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numTemprature)).EndInit();
            this.panelBody.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Timer timer1s;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.SaveFileDialog saveFileDialog;
    internal System.Windows.Forms.CustomStatusStrip CustomStatusStrip1;
    internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
    internal System.Windows.Forms.ToolStripStatusLabel lblTime;
    private System.Windows.Forms.Panel panelTop;
    public System.Windows.Forms.Label lblCaption;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Button btnMaximize;
    private System.Windows.Forms.Button btnMinimize;
    public System.Windows.Forms.Label lblRole;
    public System.Windows.Forms.Label lblBuildInfo;
    private System.Windows.Forms.Button btnHome;
    public System.Windows.Forms.Button btnConfig;
    private System.Windows.Forms.Button btnclose;
    private System.Windows.Forms.Panel panel25;
    private System.Windows.Forms.CheckBox chkAutoSave;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnTest;
    internal System.Windows.Forms.CustomToolStrip customToolStrip4;
    private System.Windows.Forms.ToolStripLabel toolStripLabel5;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripButton btnReload;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtTestName;
    private System.Windows.Forms.TextBox txtTestSteep;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panelHeader;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox txtSerialNo;
    private System.Windows.Forms.ComboBox cboOperator;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Panel panel17;
    private System.Windows.Forms.Panel panel21;
    private System.Windows.Forms.Label ledResult;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    internal System.Windows.Forms.CustomToolStrip customToolStrip3;
    private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    private System.Windows.Forms.ToolStripButton btnClearLog;
    private System.Windows.Forms.NumericUpDown numTemprature;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnNext;
    private System.Windows.Forms.Button btnReady;
    private System.Windows.Forms.Button btnMode3;
    private System.Windows.Forms.Button btnMode2;
    private System.Windows.Forms.Button btnMode1;
    private System.Windows.Forms.PictureBox picTesting;
    private System.Windows.Forms.Label lblTestTime;
    private DefaultNS.RelayBoards relayBoards1;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.ListBox lstLogWindow;
    private System.Windows.Forms.TextBox txtTotalSteep;
    private System.Windows.Forms.Label label6;
    private DefaultNS.PowerBoard powerBoard1;
    internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    private System.Windows.Forms.Panel panelBody;
    private System.Windows.Forms.Panel panel5;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    internal System.Windows.Forms.DataGridView dgvTestList;
    private System.Windows.Forms.DataGridViewTextBoxColumn TestOrder;
    private System.Windows.Forms.DataGridViewTextBoxColumn TestName;
    private System.Windows.Forms.DataGridViewTextBoxColumn Description;
    private System.Windows.Forms.DataGridViewTextBoxColumn CheckValue;
    private System.Windows.Forms.DataGridViewTextBoxColumn ReadValue;
    private System.Windows.Forms.DataGridViewTextBoxColumn ElapseTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn Result;
    private System.Windows.Forms.DataGridViewTextBoxColumn BeginTest;
    private DefaultNS.DUT_RelayCtrl DUT;
    private System.Windows.Forms.Panel panelLeft;
    private DefaultNS.autonicsMT4 autonicsMT41;
    private DefaultNS.SPM93 spM931;
    internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel7;
    internal System.Windows.Forms.ToolStripStatusLabel lblRelaysBoardPortName;
    private System.Windows.Forms.Label lbMeshStatus;
    private System.Windows.Forms.Label lbPassRate;
}

