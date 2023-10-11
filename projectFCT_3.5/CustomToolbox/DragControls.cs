using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace DefaultApp.CustomToolbox
{
    [DebuggerStepThrough]
    [ProvideProperty("PMTTFramework", typeof(Control))]
    public class DragControl : Component
    {
        private Drag drag_0 = new Drag();
        private bool bool_0 = true;
        private bool bool_1 = true;
        private bool bool_2 = true;
        private Control control_0;
        private ContainerControl containerControl_0;
        private IContainer icontainer_0;
        private Timer timer_0;

        public DragControl()
        {
            this.method_3();
            int usageMode = (int)LicenseManager.UsageMode;
        }

        public DragControl(IContainer container)
        {
            container.Add((IComponent)this);
            this.method_3();
        }

        public void Grab(Control _control)
        {
            this.drag_0.Grab(_control);
        }

        public void Grab()
        {
            this.drag_0.Grab((Control)this.containerControl_0);
        }

        public void Release()
        {
            this.drag_0.Release();
        }

        public void Drag(bool horixontal = true, bool Vertical = true)
        {
            this.drag_0.MoveObject(Vertical, horixontal);
        }

        public Control TargetControl
        {
            get
            {
                return this.control_0;
            }
            set
            {
                this.control_0 = value;
            }
        }

        private ContainerControl containerControl
        {
            get
            {
                return this.containerControl_0;
            }
            set
            {
                this.containerControl_0 = value;
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (value == null)
                    return;
                IDesignerHost service = value.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    IComponent rootComponent = service.RootComponent;
                    if (rootComponent is ContainerControl)
                    {
                        this.containerControl = rootComponent as ContainerControl;
                        return;
                    }
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

        public bool Fixed
        {
            get
            {
                return this.bool_0;
            }
            set
            {
                this.bool_0 = value;
            }
        }

        public bool Vertical
        {
            get
            {
                return this.bool_1;
            }
            set
            {
                this.bool_1 = value;
            }
        }

        public bool Horizontal
        {
            get
            {
                return this.bool_2;
            }
            set
            {
                this.bool_2 = value;
            }
        }

        private void timer_0_Tick(object sender, EventArgs e)
        {
            try
            {
                this.timer_0.Stop();
                Control control = (Control)this.containerControl;
                if (this.control_0 != null)
                    control = this.control_0;
                control.MouseDown += new MouseEventHandler(this.method_2);
                control.MouseMove += new MouseEventHandler(this.method_0);
                control.MouseUp += new MouseEventHandler(this.method_1);
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

        private void method_0(object sender, MouseEventArgs e)
        {
            this.Drag(this.Vertical, this.Horizontal);
        }

        private void method_1(object sender, MouseEventArgs e)
        {
            this.Release();
        }

        private void method_2(object sender, MouseEventArgs e)
        {
            if (this.bool_0)
            {
                Control _control = (Control)sender;
                while (_control.Parent != null)
                    _control = _control.Parent;
                this.Grab(_control);
            }
            else
                this.Grab((Control)sender);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.icontainer_0 != null)
                this.icontainer_0.Dispose();
            base.Dispose(disposing);
        }

        private void method_3()
        {
            this.icontainer_0 = (IContainer)new Container();
            this.timer_0 = new Timer(this.icontainer_0);
            this.timer_0.Enabled = true;
            this.timer_0.Interval = 1;
            this.timer_0.Tick += new EventHandler(this.timer_0_Tick);
        }
    }
}
