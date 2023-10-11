namespace DefaultNS
{
    partial class SPM93
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnOnOff = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.dgvValue = new System.Windows.Forms.DataGridView();
            this.PKID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhaseA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhaseB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhaseC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Total = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvValue)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOnOff
            // 
            this.btnOnOff.BackColor = System.Drawing.Color.Black;
            this.btnOnOff.Location = new System.Drawing.Point(1, 0);
            this.btnOnOff.Name = "btnOnOff";
            this.btnOnOff.Size = new System.Drawing.Size(26, 24);
            this.btnOnOff.TabIndex = 119;
            this.btnOnOff.UseVisualStyleBackColor = false;
            this.btnOnOff.Click += new System.EventHandler(this.btnOnOff_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gainsboro;
            this.panel1.Controls.Add(this.lblCaption);
            this.panel1.Controls.Add(this.btnOnOff);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(270, 24);
            this.panel1.TabIndex = 166;
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCaption.Location = new System.Drawing.Point(31, 2);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(126, 19);
            this.lblCaption.TabIndex = 155;
            this.lblCaption.Text = "SPM93 METER";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvValue
            // 
            this.dgvValue.AllowUserToAddRows = false;
            this.dgvValue.AllowUserToDeleteRows = false;
            dataGridViewCellStyle17.BackColor = System.Drawing.Color.WhiteSmoke;
            this.dgvValue.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle17;
            this.dgvValue.BackgroundColor = System.Drawing.Color.White;
            this.dgvValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle18.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.Color.LemonChiffon;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvValue.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this.dgvValue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvValue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PKID,
            this.PhaseA,
            this.PhaseB,
            this.PhaseC,
            this.Total});
            this.dgvValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvValue.GridColor = System.Drawing.Color.Gainsboro;
            this.dgvValue.Location = new System.Drawing.Point(0, 0);
            this.dgvValue.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgvValue.MultiSelect = false;
            this.dgvValue.Name = "dgvValue";
            this.dgvValue.ReadOnly = true;
            this.dgvValue.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle24.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle24.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle24.ForeColor = System.Drawing.Color.Blue;
            dataGridViewCellStyle24.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvValue.RowHeadersDefaultCellStyle = dataGridViewCellStyle24;
            this.dgvValue.RowHeadersWidth = 4;
            this.dgvValue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvValue.Size = new System.Drawing.Size(270, 141);
            this.dgvValue.TabIndex = 170;
            // 
            // PKID
            // 
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.PKID.DefaultCellStyle = dataGridViewCellStyle19;
            this.PKID.HeaderText = "#";
            this.PKID.Name = "PKID";
            this.PKID.ReadOnly = true;
            this.PKID.Width = 50;
            // 
            // PhaseA
            // 
            dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.PhaseA.DefaultCellStyle = dataGridViewCellStyle20;
            this.PhaseA.HeaderText = "PhaseA";
            this.PhaseA.Name = "PhaseA";
            this.PhaseA.ReadOnly = true;
            this.PhaseA.Width = 52;
            // 
            // PhaseB
            // 
            dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.PhaseB.DefaultCellStyle = dataGridViewCellStyle21;
            this.PhaseB.HeaderText = "PhaseB";
            this.PhaseB.Name = "PhaseB";
            this.PhaseB.ReadOnly = true;
            this.PhaseB.Width = 52;
            // 
            // PhaseC
            // 
            dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.PhaseC.DefaultCellStyle = dataGridViewCellStyle22;
            this.PhaseC.HeaderText = "PhaseC";
            this.PhaseC.Name = "PhaseC";
            this.PhaseC.ReadOnly = true;
            this.PhaseC.Width = 52;
            // 
            // Total
            // 
            this.Total.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Total.DefaultCellStyle = dataGridViewCellStyle23;
            this.Total.HeaderText = "Total";
            this.Total.Name = "Total";
            this.Total.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgvValue);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(270, 141);
            this.panel2.TabIndex = 171;
            // 
            // SPM93
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SPM93";
            this.Size = new System.Drawing.Size(270, 165);
            this.Load += new System.EventHandler(this.SPM91_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvValue)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblCaption;
        internal System.Windows.Forms.DataGridView dgvValue;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridViewTextBoxColumn PKID;
        private System.Windows.Forms.DataGridViewTextBoxColumn PhaseA;
        private System.Windows.Forms.DataGridViewTextBoxColumn PhaseB;
        private System.Windows.Forms.DataGridViewTextBoxColumn PhaseC;
        private System.Windows.Forms.DataGridViewTextBoxColumn Total;
    }
}
