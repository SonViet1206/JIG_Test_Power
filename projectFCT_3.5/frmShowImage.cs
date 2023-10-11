using projectRU_New;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DefaultNS
{
    public partial class frmShowImage : Form
    {
        string img_path = "";
        public frmShowImage()
        {
            InitializeComponent();
        }

        int TestSteep = 0;

        public void ShowForm(string image_path, int testStep)
        {
            TestSteep = testStep;
            img_path = Application.StartupPath + "\\Images\\" + image_path;
            this.ShowDialog();
        }

        private void frmShowImage_Shown(object sender, EventArgs e)
        {
            timer10ms.Enabled = true;
        }

        private void frmShowImage_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.Dispose();
            }
            if(e.KeyCode==Keys.Shift)
            {
                FrmCheckHand frmCheckHand = new FrmCheckHand();
                if (frmCheckHand.btnOK.BackColor == Color.Green)
                {
                    Global.checkHand=true;
                }
                if(frmCheckHand.btnNG.BackColor == Color.Red)
                {
                    Global.checkHand = false;
                }    
            }    
         
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void frmShowImage_Load(object sender, EventArgs e)
        {
            try
            {
                Image img = Image.FromFile(img_path);
                //this.Width = img.Width;
                //this.Height = img.Height;
                this.StartPosition = FormStartPosition.CenterScreen;
                picHelp.Image = img;
               
            }
            catch (Exception ex)
            {

                
            }
        }

        private void timer10ms_Tick(object sender, EventArgs e)
        {
            if ((Global.StatesInputPowerboard & 0x01) != 0)
            {
                this.Close();            
            }
        }

        private void frmShowImage_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer10ms.Stop();
            timer10ms.Enabled = false;
        }
    }
}
