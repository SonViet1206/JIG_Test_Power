using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;


using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace projectRU_New
{
    public class DCL6104
    {
        public SerialPort port = new SerialPort();
        byte[] val=new byte[100];
        public DCL6104(string portname)
        {
            
            try
            {
                string receve = "";
                port.PortName = portname;
                port.BaudRate = 9600;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Open();
                port.Write("*IDN?\r\n");
                Thread.Sleep(100);

                receve = port.ReadExisting().ToString();


                if (receve.CompareTo("DINGCHEN,DCL6104,L614X331621A0007,V03.00\r\n") == 0)
                {
                    this.isOpen = true;
                }
                else
                {
                    this.isOpen = false;
                }
                Thread threadCheckConnect = new Thread(CheckConnect);
                threadCheckConnect.IsBackground = true;
                threadCheckConnect.Start();
            }
            catch
            {
                return;
            }
            

           
                

        }

        private void CheckConnect()
        {
            while(true)
            {
                if(!port.IsOpen)
                {
                    try
                    {
                        port.Open();
                    }
                    catch 
                    {

                        
                    }
                    
                }
                Thread.Sleep(1000);
            }    
        }

        public void Disconnect()
        {
            port.Close();
            isOpen = false;
        }
        public  bool isOpen { get; set; }

        public double Read_Vol(string chose)
        {
            double value=-1;
            
            if(chose == "CURRENT")
            {
                port.Write("FETCh:VOLTage?\r\n");
                Thread.Sleep(100);
                value = double.Parse(port.ReadExisting());
                
            }
            else if(chose == "SET")
            {
                
                port.Write("VOLTage?\r\n");
                Thread.Sleep(100);
                value = double.Parse(port.ReadExisting());
                

            }
            
            return value;
        }
        public double Read_Current(string chose)
        {
            double value = -1;

            if (chose == "CURRENT")
            {
                port.Write("FETCh:CURRENT?\r\n");
                Thread.Sleep(100);
               value = double.Parse(port.ReadExisting());
            }
            else if (chose == "SET")
            {
                port.Write("CURRENT?\r\n");
                Thread.Sleep(100);
               value = double.Parse(port.ReadExisting());
            }
            Global.databuff.Remove(0, Global.databuff.Length);
            return value;

        }
        public double Read_Res()
        {
            double value = -1;
            port.Write("RESistance?\r\n");
            Thread.Sleep(100);
           value = double.Parse(port.ReadExisting());
           
            return value;

        }
        public void Set_Res(double value)
        {
            port.Write("RESistance "+ value + "\r\n");
            Thread.Sleep(100);
        }
        public void Set_Voltage(double value)
        {
            string cmd = "VOLTage " + value + "\r\n";
            port.Write(cmd);
            Thread.Sleep(100);
        }
        public void Set_Curr(double value)
        {
            port.Write("CURRent " + value + "\r\n");
            Thread.Sleep(100);
        }
        public void ON_LOAD()
        {
            port.Write("LOAD ON\r\n");
            Thread.Sleep(100);
        }
        public void OFF_LOAD()
        {
            try
            {
                port.Write("LOAD OFF\r\n");
                Thread.Sleep(100);
            }
            catch
            {

            }
            
        }
        public void MODE(string mode)//mode= CC| mode = CV
        {
            port.Write("TRAN:MODE "+mode+"\r\n");
            Thread.Sleep(100);
        }
        public string MODE_is()
        {
            port.Write("TRAN:MODE?\r\n");
            Thread.Sleep(100);
            string s = port.ReadExisting();
            return s;
        }
    }
}
