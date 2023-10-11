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
    public partial class frmShowChoice : Form
    {
        public int State = 0;

        public int Counter 
        { 
            get { return Conv.atoi32(lblCounter.Text); }
            set { lblCounter.Text = value.ToString(); }
        }

        public string CounterText
        {
            get { return lblCounter.Text; }
        }

        public frmShowChoice()
        {
            InitializeComponent();
        }

        private void frmShowImage_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Enter)
            //    btnSetOK_Click(null, null);
            //else if (e.KeyCode == Keys.Escape)
            //    btnSetNG_Click(null, null);
        }

        private void btnSetOK_Click(object sender, EventArgs e)
        {
            State = 1;
        }

        private void btnSetNG_Click(object sender, EventArgs e)
        {
            State = 2;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            State = -1;
            this.Close();
        }

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private void lblHeader_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void lblHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void lblHeader_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}
