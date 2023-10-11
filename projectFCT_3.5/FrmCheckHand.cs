using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projectRU_New
{
    public partial class FrmCheckHand : Form
    {
        public FrmCheckHand()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnOK.BackColor = Color.Green;
            btnNG.BackColor = ActiveControl.BackColor;
        }

        private void btnNG_Click(object sender, EventArgs e)
        {
            btnNG.BackColor = Color.Red;
            btnOK.BackColor = ActiveControl.BackColor;
        }
    }
}
