using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace DefaultNS
{
    /// <summary>
    /// TcpClientWithTimeout is used to open a TcpClient connection, with a 
    /// user definable connection timeout in milliseconds (1000=1second)
    /// Use it like this:
    /// TcpClient connection = new TcpClientWithTimeout('127.0.0.1',80,1000).Connect();
    /// </summary>
    public class TcpClientWithTimeout
    {
        protected string _hostname = "127.0.0.1";
        protected int _port = 80;
        protected int _timeout_milliseconds = 1000;
        protected bool connected;

        public string SearchPort(int port, int timeout_milliseconds)
        {
            _hostname = "";
            _port = port;
            _timeout_milliseconds = timeout_milliseconds;

            connected = false;
            //Thread thread = new Thread(new ThreadStart(BeginConnect));
            Thread[] threads = new Thread[100];
            for (int i = 1; i < threads.Length; i++)
            {
                string ip = "192.168.1." + i.ToString();
                threads[i] = new Thread(() => BeginConnect(ip));
                threads[i].IsBackground = true; // So that a failed connection attempt, wont prevent the process from terminating while it does the long timeout
                threads[i].Start();
                threads[i].Join(_timeout_milliseconds); //Wait for either the timeout or the thread to finish
            }

            int ms_count = 0;
            while (_hostname == "" && ms_count < 3000)
            {
                ++ms_count;
                Conv.delay_ms(1);
            }

            return _hostname;
        }

        public bool TestConnect(string hostname, int port, int timeout_milliseconds)
        {
            _hostname = "";
            _port = port;
            _timeout_milliseconds = timeout_milliseconds;

            connected = false;
            //Thread thread = new Thread(new ThreadStart(BeginConnect));
            Thread thread = new Thread(() => BeginConnect(hostname));
            thread.IsBackground = true; // So that a failed connection attempt, wont prevent the process from terminating while it does the long timeout
            thread.Start();
            thread.Join(_timeout_milliseconds); //Wait for either the timeout or the thread to finish

            return connected;
        }
        
        protected void BeginConnect(string hostname)
        {
            try
            {
                TcpClient connection = new TcpClient(hostname, _port);
                connection.Close(); //Dong luon vi muc dich chi test ket noi
                connected = true;
                //_hostname = hostname;
            }
            catch { }
        }
    }
 }
