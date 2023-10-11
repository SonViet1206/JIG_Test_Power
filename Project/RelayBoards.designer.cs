namespace DefaultNS
{
    partial class RelayBoards
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnOnOff = new System.Windows.Forms.Button();
            this.lblCaption = new System.Windows.Forms.Label();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.chkTestMode = new System.Windows.Forms.CheckBox();
            this.dgrDO = new System.Windows.Forms.DataGridView();
            this.IND = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.State = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Caption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SiteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RegNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgrDI = new System.Windows.Forms.DataGridView();
            this.IndDI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StateDI = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.CaptionDI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitter1 = new System.Windows.Forms.Splitter();
            ((System.ComponentModel.ISupportInitialize)(this.dgrDO)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgrDI)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOnOff
            // 
            this.btnOnOff.BackColor = System.Drawing.Color.Black;
            this.btnOnOff.Location = new System.Drawing.Point(1, 0);
            this.btnOnOff.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(26, 24);
            this.btnOnOff.TabIndex = 119;
            this.btnOnOff.UseVisualStyleBackColor = false;
            this.btnOnOff.Click += new System.EventHandler(this.btnOnOff_Click);
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCaption.Location = new System.Drawing.Point(31, 2);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(137, 19);
            this.lblCaption.TabIndex = 155;
            this.lblCaption.Text = "RELAYS BOARD";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(71, 25);
            this.btnClearAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(47, 25);
            this.btnClearAll.TabIndex = 161;
            this.btnClearAll.Text = "OFF";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // chkTestMode
            // 
            this.chkTestMode.AutoSize = true;
            this.chkTestMode.Location = new System.Drawing.Point(49, 32);
            this.chkTestMode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkTestMode.Name = "chkTestMode";
            this.chkTestMode.Size = new System.Drawing.Size(15, 14);
            this.chkTestMode.TabIndex = 132;
            this.chkTestMode.UseVisualStyleBackColor = true;
            // 
            // dgrDO
            // 
            this.dgrDO.AllowUserToAddRows = false;
            this.dgrDO.AllowUserToDeleteRows = false;
            this.dgrDO.BackgroundColor = System.Drawing.Color.White;
            this.dgrDO.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.LemonChiffon;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgrDO.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgrDO.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgrDO.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IND,
            this.State,
            this.Caption,
            this.SiteID,
            this.RegNo});
            this.dgrDO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgrDO.GridColor = System.Drawing.Color.Gainsboro;
            this.dgrDO.Location = new System.Drawing.Point(0, 24);
            this.dgrDO.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgrDO.MultiSelect = false;
            this.dgrDO.Name = "dgrDO";
            this.dgrDO.ReadOnly = true;
            this.dgrDO.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgrDO.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgrDO.RowHeadersWidth = 4;
            this.dgrDO.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgrDO.Size = new System.Drawing.Size(270, 552);
            this.dgrDO.TabIndex = 164;
            this.dgrDO.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgrDO_CellContentClick);
            // 
            // IND
            // 
            this.IND.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.IND.DefaultCellStyle = dataGridViewCellStyle2;
            this.IND.HeaderText = "DO";
            this.IND.Name = "IND";
            this.IND.ReadOnly = true;
            this.IND.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.IND.Width = 38;
            // 
            // State
            // 
            this.State.HeaderText = "";
            this.State.Name = "State";
            this.State.ReadOnly = true;
            this.State.Width = 28;
            // 
            // Caption
            // 
            this.Caption.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.Caption.DefaultCellStyle = dataGridViewCellStyle3;
            this.Caption.HeaderText = "NAME";
            this.Caption.Name = "Caption";
            this.Caption.ReadOnly = true;
            this.Caption.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // SiteID
            // 
            this.SiteID.HeaderText = "SITEID";
            this.SiteID.Name = "SiteID";
            this.SiteID.ReadOnly = true;
            this.SiteID.Visible = false;
            // 
            // RegNo
            // 
            this.RegNo.HeaderText = "REGNO";
            this.RegNo.Name = "RegNo";
            this.RegNo.ReadOnly = true;
            this.RegNo.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gainsboro;
            this.panel1.Controls.Add(this.btnOnOff);
            this.panel1.Controls.Add(this.lblCaption);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(270, 24);
            this.panel1.TabIndex = 165;
            // 
            // dgrDI
            // 
            this.dgrDI.AllowUserToAddRows = false;
            this.dgrDI.AllowUserToDeleteRows = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.WhiteSmoke;
            this.dgrDI.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgrDI.BackgroundColor = System.Drawing.Color.White;
            this.dgrDI.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.LemonChiffon;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgrDI.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgrDI.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgrDI.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IndDI,
            this.StateDI,
            this.CaptionDI});
            this.dgrDI.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgrDI.GridColor = System.Drawing.Color.Gainsboro;
            this.dgrDI.Location = new System.Drawing.Point(0, 586);
            this.dgrDI.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgrDI.MultiSelect = false;
            this.dgrDI.Name = "dgrDI";
            this.dgrDI.ReadOnly = true;
            this.dgrDI.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgrDI.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgrDI.RowHeadersWidth = 4;
            this.dgrDI.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgrDI.Size = new System.Drawing.Size(270, 204);
            this.dgrDI.TabIndex = 166;
            // 
            // IndDI
            // 
            this.IndDI.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.IndDI.DefaultCellStyle = dataGridViewCellStyle7;
            this.IndDI.HeaderText = "DI";
            this.IndDI.Name = "IndDI";
            this.IndDI.ReadOnly = true;
            this.IndDI.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.IndDI.Width = 38;
            // 
            // StateDI
            // 
            this.StateDI.HeaderText = "";
            this.StateDI.Name = "StateDI";
            this.StateDI.ReadOnly = true;
            this.StateDI.Width = 28;
            // 
            // CaptionDI
            // 
            this.CaptionDI.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.CaptionDI.DefaultCellStyle = dataGridViewCellStyle8;
            this.CaptionDI.HeaderText = "NAME";
            this.CaptionDI.Name = "CaptionDI";
            this.CaptionDI.ReadOnly = true;
            this.CaptionDI.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 576);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(270, 10);
            this.splitter1.TabIndex = 167;
            this.splitter1.TabStop = false;
            // 
            // RelayBoards
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.chkTestMode);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.dgrDO);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dgrDI);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "RelayBoards";
            this.Size = new System.Drawing.Size(270, 790);
            this.Load += new System.EventHandler(this.Control_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgrDO)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgrDI)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.CheckBox chkTestMode;
        internal System.Windows.Forms.DataGridView dgrDO;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.DataGridView dgrDI;
        private System.Windows.Forms.DataGridViewTextBoxColumn IND;
        private System.Windows.Forms.DataGridViewCheckBoxColumn State;
        private System.Windows.Forms.DataGridViewTextBoxColumn Caption;
        private System.Windows.Forms.DataGridViewTextBoxColumn SiteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn IndDI;
        private System.Windows.Forms.DataGridViewCheckBoxColumn StateDI;
        private System.Windows.Forms.DataGridViewTextBoxColumn CaptionDI;
        private System.Windows.Forms.Splitter splitter1;
    }
}
