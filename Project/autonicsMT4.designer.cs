namespace DefaultNS
{
    partial class autonicsMT4
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
            this.lblUnit = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.ledValue = new DmitryBrant.CustomControls.SevenSegmentArray();
            this.panel1.SuspendLayout();
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
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.BackColor = System.Drawing.SystemColors.Control;
            this.lblUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnit.ForeColor = System.Drawing.Color.Black;
            this.lblUnit.Location = new System.Drawing.Point(162, 53);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(46, 20);
            this.lblUnit.TabIndex = 158;
            this.lblUnit.Text = "VDC";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gainsboro;
            this.panel1.Controls.Add(this.lblCaption);
            this.panel1.Controls.Add(this.btnOnOff);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(245, 24);
            this.panel1.TabIndex = 166;
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCaption.Location = new System.Drawing.Point(31, 2);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(122, 19);
            this.lblCaption.TabIndex = 155;
            this.lblCaption.Text = "MT4W METER";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ledValue
            // 
            this.ledValue.ArrayCount = 4;
            this.ledValue.ColorBackground = System.Drawing.SystemColors.Control;
            this.ledValue.ColorDark = System.Drawing.SystemColors.ControlLight;
            this.ledValue.ColorLight = System.Drawing.Color.Black;
            this.ledValue.DecimalShow = true;
            this.ledValue.ElementPadding = new System.Windows.Forms.Padding(4);
            this.ledValue.ElementWidth = 10;
            this.ledValue.ItalicFactor = -0.12F;
            this.ledValue.Location = new System.Drawing.Point(35, 26);
            this.ledValue.Name = "ledValue";
            this.ledValue.Size = new System.Drawing.Size(121, 47);
            this.ledValue.TabIndex = 156;
            this.ledValue.TabStop = false;
            this.ledValue.Value = "0.0";
            // 
            // autonicsMT4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblUnit);
            this.Controls.Add(this.ledValue);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "autonicsMT4";
            this.Size = new System.Drawing.Size(245, 138);
            this.Load += new System.EventHandler(this.Control_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Label lblUnit;
        private DmitryBrant.CustomControls.SevenSegmentArray ledValue;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblCaption;
    }
}
