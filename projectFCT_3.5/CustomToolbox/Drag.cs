using System;
using System.Drawing;
using System.Windows.Forms;

namespace DefaultApp.CustomToolbox
{
    partial class Drag : Form
    {
        private bool bool_0;
        private int int_0;
        private int int_1;
        private Control control_0;

        public void Grab(Control a)
        {
            try
            {
                this.control_0 = a;
                this.bool_0 = true;
                this.int_0 = Control.MousePosition.X - this.control_0.Left;
                this.int_1 = Control.MousePosition.Y - this.control_0.Top;
            }
            catch (Exception ex)
            {
            }
            int num1=0;
            int num2=0;
            while (num2 == num1)
            {
                num1 = 1;
                int num3 = 1;
                int num4 = num2;
                num2 = num3;
                if (1 > num4)
                    break;
            }
        }

        public void Release()
        {
            this.bool_0 = false;
        }

        public void MoveObject(bool Horizontal = true, bool Vertical = true)
        {
            try
            {
                if (this.bool_0)
                {
                    Point mousePosition = Control.MousePosition;
                    int x = mousePosition.X;
                    mousePosition = Control.MousePosition;
                    int y = mousePosition.Y;
                    if (Vertical)
                        this.control_0.Top = y - this.int_1;
                    if (Horizontal)
                        this.control_0.Left = x - this.int_0;
                }
            }
            catch (Exception ex)
            {
            }
            int num1=0;
            int num2=0;
            while (num2 == num1)
            {
                num1 = 1;
                int num3 = 1;
                int num4 = num2;
                num2 = num3;
                if (1 > num4)
                    break;
            }
        }
    }
}
