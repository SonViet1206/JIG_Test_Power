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

        public void ShowForm(string image_path)
        {
            img_path = Application.StartupPath + "\\Images\\" + image_path;
            this.ShowDialog();
        }

        private void frmShowImage_Shown(object sender, EventArgs e)
        {
            
        }

        private void frmShowImage_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.Dispose();
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
    }
}
