
namespace DefaultNS
{
    partial class Evalator
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnModeE = new System.Windows.Forms.Button();
            this.btnModeD = new System.Windows.Forms.Button();
            this.btnModeB = new System.Windows.Forms.Button();
            this.btnModeA = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOnOff = new System.Windows.Forms.Button();
            this.lblCaption = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnCheckEthStatus = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnCheckEthStatus);
            this.panel1.Controls.Add(this.richTextBox1);
            this.panel1.Controls.Add(this.btnModeE);
            this.panel1.Controls.Add(this.btnModeD);
            this.panel1.Controls.Add(this.btnModeB);
            this.panel1.Controls.Add(this.btnModeA);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(262, 278);
            this.panel1.TabIndex = 152;
            // 
            // btnModeE
            // 
            this.btnModeE.BackColor = System.Drawing.Color.Gainsboro;
            this.btnModeE.Location = new System.Drawing.Point(194, 30);
            this.btnModeE.Name = "btnModeE";
            this.btnModeE.Size = new System.Drawing.Size(58, 23);
            this.btnModeE.TabIndex = 177;
            this.btnModeE.Text = "Mode E";
            this.btnModeE.UseVisualStyleBackColor = false;
            this.btnModeE.Click += new System.EventHandler(this.btnSetMode_Click);
            // 
            // btnModeD
            // 
            this.btnModeD.Location = new System.Drawing.Point(130, 30);
            this.btnModeD.Name = "btnModeD";
            this.btnModeD.Size = new System.Drawing.Size(58, 23);
            this.btnModeD.TabIndex = 176;
            this.btnModeD.Text = "Mode D";
            this.btnModeD.UseVisualStyleBackColor = true;
            this.btnModeD.Click += new System.EventHandler(this.btnSetMode_Click);
            // 
            // btnModeB
            // 
            this.btnModeB.Location = new System.Drawing.Point(66, 30);
            this.btnModeB.Name = "btnModeB";
            this.btnModeB.Size = new System.Drawing.Size(58, 23);
            this.btnModeB.TabIndex = 175;
            this.btnModeB.Text = "Mode B";
            this.btnModeB.UseVisualStyleBackColor = true;
            this.btnModeB.Click += new System.EventHandler(this.btnSetMode_Click);
            // 
            // btnModeA
            // 
            this.btnModeA.Location = new System.Drawing.Point(2, 30);
            this.btnModeA.Name = "btnModeA";
            this.btnModeA.Size = new System.Drawing.Size(58, 23);
            this.btnModeA.TabIndex = 174;
            this.btnModeA.Text = "Mode A";
            this.btnModeA.UseVisualStyleBackColor = true;
            this.btnModeA.Click += new System.EventHandler(this.btnSetMode_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(74, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 16);
            this.label4.TabIndex = 170;
            this.label4.Text = "----";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 16);
            this.label1.TabIndex = 170;
            this.label1.Text = "SLAC=";
            this.label1.DoubleClick += new System.EventHandler(this.label1_DoubleClick);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gainsboro;
            this.panel2.Controls.Add(this.btnOnOff);
            this.panel2.Controls.Add(this.lblCaption);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(260, 24);
            this.panel2.TabIndex = 169;
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
            // lblCaption
            // 
            this.lblCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCaption.BackColor = System.Drawing.Color.Gainsboro;
            this.lblCaption.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCaption.Location = new System.Drawing.Point(27, 2);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(234, 20);
            this.lblCaption.TabIndex = 155;
            this.lblCaption.Text = "EVALATOR BOARD";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(3, 87);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(249, 186);
            this.richTextBox1.TabIndex = 178;
            this.richTextBox1.Text = " ";
            // 
            // btnCheckEthStatus
            // 
            this.btnCheckEthStatus.BackColor = System.Drawing.Color.Gainsboro;
            this.btnCheckEthStatus.Location = new System.Drawing.Point(194, 58);
            this.btnCheckEthStatus.Name = "btnCheckEthStatus";
            this.btnCheckEthStatus.Size = new System.Drawing.Size(58, 23);
            this.btnCheckEthStatus.TabIndex = 179;
            this.btnCheckEthStatus.Text = "E Status";
            this.btnCheckEthStatus.UseVisualStyleBackColor = false;
            this.btnCheckEthStatus.Click += new System.EventHandler(this.btnCheckEthStatus_Click);
            // 
            // Evalator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "Evalator";
            this.Size = new System.Drawing.Size(262, 278);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Button btnOnOff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnModeE;
        private System.Windows.Forms.Button btnModeD;
        private System.Windows.Forms.Button btnModeB;
        private System.Windows.Forms.Button btnModeA;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnCheckEthStatus;
    }
}
