using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;

namespace DefaultNS
{
    public partial class RelayBoards : UserControl
    {
        bool thread_ended = true;
        bool thread_started = false; 
        Thread threadCOMM;

        bool thread_busy = false;
        bool command_busy = false;

        string _port_name = "COM8";
        SerialPort portCOMM = null;
        bool comm_state = false;

        struct BOARD_TYPE
        {
            public int gridStartDI;
            public int gridStartDO;
            public int regLength;
            public byte[] CMD_READALL_DI;
            public byte[] CMD_READALL_DO;
            public byte[] CMD_CLEARALL_DO;
            public byte[] CMD_RELATED_DIDO;
        }

        List<BOARD_TYPE> m_Board = new List<BOARD_TYPE>();
        
        
        public RelayBoards()
        {
            InitializeComponent();
        }

        private void Control_Load(object sender, EventArgs e)
        {
            //this.Width = pictureBox3.Width;
            //this.Height = pictureBox3.Height;
            this.Font = new Font("Tahoma", 12);
        }

        public bool CommState { get { return comm_state; } }

        public void Start(string port_name, string board_type)
        {
            string[] boards = board_type.Split(',');
            dgrDI.Rows.Clear();
            dgrDO.Rows.Clear();
            m_Board.Clear();
            for (int i = 0; i < boards.Length; i++)
            {
                int r_count = 0;
                if (boards[i] == "1")
                    r_count = 8;
                else if (boards[i] == "2")
                    r_count = 16;
                else if (boards[i] == "3")
                    r_count = 24;
                else if (boards[i] == "4")
                    r_count = 32;

                if (r_count > 0)
                {
                    //byte[] CMD_ID1_READ_DOs = ModbusRTU_ReadMultiCoilsFrame(site_id, 0x03, 0x0001, do_count);
                    //byte[] CMD_ID1_READ_DIs = ModbusRTU_ReadMultiCoilsFrame(site_id, 0x03, 0x0081, do_count);
                    //Close all：01 06 00 00 08 00 8E 0A

                    BOARD_TYPE item;
                    item.gridStartDI = dgrDI.Rows.Count; 
                    item.gridStartDO = dgrDO.Rows.Count;
                    item.regLength = r_count;
                    item.CMD_READALL_DI = (r_count == 8) ? ModbusRTU_ReadMultiCoilsFrame(i + 1, 0x0081, r_count) : null;
                    item.CMD_CLEARALL_DO = ModbusRTU_WriteCommandFrame(i + 1, 0x0000, 0x08, 0x00);
                    item.CMD_READALL_DO = ModbusRTU_ReadMultiCoilsFrame(i + 1, 0x0001, r_count);
                    item.CMD_RELATED_DIDO = ModbusRTU_WriteCommandFrame(i + 1, 0x00FD, 0x00, 0x00); //01 06 00FD 00 00 18 3A
                    m_Board.Add(item);
                    for (int j = 0; j < r_count; j++)
                    {
                        dgrDO.Rows.Add();
                        dgrDO.Rows[dgrDO.Rows.Count - 1].Cells["Ind"].Value = dgrDO.Rows.Count;
                        dgrDO.Rows[dgrDO.Rows.Count - 1].Cells["Caption"].Value = "";
                        dgrDO.Rows[dgrDO.Rows.Count - 1].Cells["SiteID"].Value = i + 1;
                        dgrDO.Rows[dgrDO.Rows.Count - 1].Cells["RegNo"].Value = j + 1;
                        dgrDO.Rows[dgrDO.Rows.Count - 1].DefaultCellStyle.BackColor = (i % 2 == 0) ? Color.FromArgb(230, 255, 255) : Color.FromArgb(255, 255, 230);
                    }
                    if (r_count == 8)
                    {
                        for (int j = 0; j < r_count; j++)
                        {
                            dgrDI.Rows.Add();
                            dgrDI.Rows[dgrDI.Rows.Count - 1].Cells["IndDI"].Value = dgrDI.Rows.Count;
                            dgrDI.Rows[dgrDI.Rows.Count - 1].Cells["CaptionDI"].Value = "";
                            dgrDI.Rows[dgrDI.Rows.Count - 1].DefaultCellStyle.BackColor = (i % 2 == 0) ? Color.FromArgb(230, 255, 255) : Color.FromArgb(255, 255, 230);
                        }
                    }
                }
            }
            dgrDI.Visible = (dgrDI.Rows.Count > 0);
            //
            Start(port_name);
        }

        public void Start(string port_name)
        {
            _port_name = port_name;
            try
            {
                portCOMM = new SerialPort(_port_name, 9600, Parity.None, 8, StopBits.One);
                portCOMM.Open();
            }
            catch (Exception ex)
            {
                Global.WriteLogFile("Lỗi kết nối với RELAYSBOARD" + "\r\n" + ex.ToString());
                MessageBox.Show(ex.Message, "Lỗi kết nối với RELAYSBOARD");
            }
            finally
            {
                portCOMM.Close();
            }
            threadCOMM = new Thread(() => COMMProcess());
            threadCOMM.Start();
        }
        
        public void Stop()
        {
            SetAllState(false);
            thread_started = false;
            while (!thread_ended)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        public void SetDOCaption(int ind, string caption)
        {
            if (ind > 0 && ind <= dgrDO.Rows.Count)
            {
                dgrDO.Rows[ind-1].Cells["Caption"].Value = caption;
            }    
        }

        public void SetDICaption(int ind, string caption)
        {
            if (ind > 0 && ind <= dgrDI.Rows.Count)
            {
                dgrDI.Rows[ind - 1].Cells["CaptionDI"].Value = caption;
            }
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (thread_started)
                Stop();
            else if (thread_ended)
                Start(_port_name);
        }

        void COMMProcess()
        {
            bool commOK = false;
            int rx_len, board_index = 0;
            
            try
            {
                thread_ended = false; 
                thread_started = true;
                this.Invoke((MethodInvoker)delegate
                {
                    btnOnOff.BackColor = Color.Red;
                });
                
                while (thread_started)
                {
                    while (command_busy)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }
                    thread_busy = true;
                    commOK = false;
                    try
                    {
                        this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = ((btnOnOff.BackColor == Color.Black) ? Color.Red : Color.Black); });
                        //this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Red; });

                        if (!portCOMM.IsOpen)
                        {
                            try
                            {
                                portCOMM.Close();
                                portCOMM.Open();
                                //Dong tat ca ro le:
                                for (int i = 0; i < m_Board.Count; i++)
                                {
                                    portCOMM.Write(m_Board[i].CMD_CLEARALL_DO, 0, m_Board[i].CMD_CLEARALL_DO.Length);
                                    Thread.Sleep(150);
                                    portCOMM.Write(m_Board[i].CMD_RELATED_DIDO, 0, m_Board[i].CMD_RELATED_DIDO.Length); //01 06 00 FD 00 00 18 3A
                                    Thread.Sleep(150);
                                }
                                portCOMM.DiscardInBuffer();
                            }
                            catch { }
                            this.Invoke((MethodInvoker)delegate
                            {
                                lblCaption.Text = (portCOMM.IsOpen ? "RELAYS BOARD - " + _port_name + ":OK" : "RELAYS BOARD - " + _port_name + ":NG");
                            });
                        }
                        else
                        {
                            commOK = true;

                            //Đọc DIs
                            if (m_Board[board_index].CMD_READALL_DI != null)
                            {
                                portCOMM.Write(m_Board[board_index].CMD_READALL_DI, 0, m_Board[board_index].CMD_READALL_DI.Length);
                                Thread.Sleep(120 + m_Board[board_index].regLength * 2);
                                rx_len = portCOMM.BytesToRead;
                                if (rx_len == m_Board[board_index].regLength * 2 + 5)
                                {
                                    byte[] rxBuff = new byte[rx_len];
                                    portCOMM.Read(rxBuff, 0, rx_len);
                                    //    0  1  2  3 4  5 6      
                                    //RX: 01 03 64 0001 0001 0001 0001.... 
                                    for (int j = 0; j < m_Board[board_index].regLength; j++)
                                    {
                                        bool state = (rxBuff[j * 2 + 4] == 1);
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            dgrDI.Rows[m_Board[board_index].gridStartDI + j].Cells["StateDI"].Value = state;
                                            dgrDI.Rows[m_Board[board_index].gridStartDI + j].Cells["StateDI"].Style.BackColor = state ? Color.Red : Color.DimGray;
                                        });
                                    }
                                }
                                else
                                {
                                    commOK = false;
                                    portCOMM.DiscardInBuffer();
                                }
                            }

                            //Đọc DOs
                            portCOMM.Write(m_Board[board_index].CMD_READALL_DO, 0, m_Board[board_index].CMD_READALL_DO.Length);
                            Thread.Sleep(120 + m_Board[board_index].regLength * 2);
                            rx_len = portCOMM.BytesToRead;
                            if (rx_len == m_Board[board_index].regLength * 2 + 5)
                            {
                                byte[] rxBuff = new byte[rx_len];
                                portCOMM.Read(rxBuff, 0, rx_len);
                                //    0  1  2  3 4  5 6      
                                //RX: 01 03 64 0001 0001 0001 0001.... 
                                for (int j = 0; j < m_Board[board_index].regLength; j++)
                                {
                                    bool state = (rxBuff[j * 2 + 4] == 1);
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        dgrDO.Rows[m_Board[board_index].gridStartDO + j].Cells["State"].Value = state;
                                        dgrDO.Rows[m_Board[board_index].gridStartDO + j].Cells["State"].Style.BackColor = state ? Color.Red : Color.DimGray;
                                    });
                                }
                            }
                            else
                            {
                                commOK = false;
                                portCOMM.DiscardInBuffer();
                            }
                            //Moi vong quet cap nhat 1 board:
                            if (++board_index >= m_Board.Count) board_index = 0;
                        }
                    }
                    catch
                    {

                    }
                    thread_busy = false;
                    comm_state = commOK;
  
                    //this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Black; });
                    if (comm_state)
                        Thread.Sleep(10);
                    else
                        Thread.Sleep(3000);
                }

            }
            catch (Exception)
            {


            }

            try { portCOMM.Close(); } catch { }
            portCOMM = null;

            this.Invoke((MethodInvoker)delegate { btnOnOff.BackColor = Color.Black; });

            thread_ended = true;
        }

        private byte[] ExecuteCommand(string cmd_str, int timeout)
        {
            int rx_len;
            byte[] retBytes = null;
            while (thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
            command_busy = true;
            try
            {
                if (portCOMM != null && portCOMM.IsOpen)
                {
                    byte[] bytes = Conv.stringToBytes(cmd_str);
                    if (bytes.Length > 0) //Frame hợp lệ
                    {
                        //Tx:01 01 00 00 00 08 3D CC
                        //Rx:01 01 01 00 51 88
                        //txtLog.AppendText("\r\n[TX][" + Conv.S2SH(cmd_str).Replace(" ","") + "][" + cmd_str.Length.ToString() + "]"); 
                        portCOMM.Write(bytes, 0, bytes.Length);
                        Conv.delay_ms(timeout);
                        rx_len = portCOMM.BytesToRead;
                        if (rx_len > 0)
                        {
                            retBytes = new byte[rx_len];
                            portCOMM.Read(retBytes, 0, rx_len);
                            //txtLog.AppendText("\r\n[RX][" + Conv.S2SH(Conv.bytesToString(retBytes)).Replace(" ", "") + "][" + retBytes.Length.ToString() + "]");
                        }

                        //if (txtLog.Text.Length > 100000) txtLog.Text = txtLog.Text.Substring(txtLog.Text.Length - 100000);
                        //txtLog.SelectionStart = txtLog.Text.Length;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            command_busy = false;

            return retBytes;
        }

        public bool SetAllState(bool state)
        {
            bool retOK = true;
            string frame = "";
            //Open all：01 06 00 00 07 00 8B FA
            //Close all：01 06 00 00 08 00 8E 0A

            for (int i = 0; i < m_Board.Count; i++)
            {
                if (state)
                {
                    //return (ExecuteCommand(Conv.SH2S("01 06 00 00 07 00 8B FA"), 80) != null);
                }
                else
                {
                    if (ExecuteCommand(Conv.bytesToString(m_Board[i].CMD_CLEARALL_DO), 120) == null) retOK = false;
                }
            }
            return retOK;
        }

        byte[] ModbusRTU_WriteCommandFrame(int siteID, int address, int command, int delay)
        {
            string frame = "";
            frame += (char)siteID;
            frame += (char)0x06; //Function Write
            frame += (char)((address >> 8) & 0xFF);
            frame += (char)(address & 0xFF);
            frame += (char)(command & 0xFF);
            frame += (char)(delay & 0xFF);
            frame += Conv.ModbusCRC(frame);
            return Conv.stringToBytes(frame);
        }

        byte[] ModbusRTU_ReadMultiCoilsFrame(int siteID, int startAdd, int len)
        {
            string frame = "";

            frame += (char)siteID;
            frame += (char)0x03; //Function
            frame += (char)((startAdd >> 8) & 0xFF);
            frame += (char)(startAdd & 0xFF);
            frame += (char)((len >> 8) & 0xFF);
            frame += (char)(len & 0xFF);
            frame += Conv.ModbusCRC(frame);
            return Conv.stringToBytes(frame);
        }

        public bool ReadInputImediate()
        {
            bool commOK = true;
            //Đọc DIs
            while (thread_busy) Conv.delay_ms(1); //Chờ cho lệnh đọc kết thúc
            command_busy = true;
            try
            {
                int rx_len;
                for (int board_index = 0; board_index < m_Board.Count; board_index++)
                {
                    if (m_Board[board_index].CMD_READALL_DI != null)
                    {
                        portCOMM.Write(m_Board[board_index].CMD_READALL_DI, 0, m_Board[board_index].CMD_READALL_DI.Length);
                        Conv.delay_ms(120 + m_Board[board_index].regLength * 2);
                        rx_len = portCOMM.BytesToRead;
                        if (rx_len == m_Board[board_index].regLength * 2 + 5)
                        {
                            byte[] rxBuff = new byte[rx_len];
                            portCOMM.Read(rxBuff, 0, rx_len);
                            //    0  1  2  3 4  5 6      
                            //RX: 01 03 64 0001 0001 0001 0001.... 
                            for (int j = 0; j < m_Board[board_index].regLength; j++)
                            {
                                bool state = (rxBuff[j * 2 + 4] == 1);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    dgrDI.Rows[m_Board[board_index].gridStartDI + j].Cells["StateDI"].Value = state;
                                    dgrDI.Rows[m_Board[board_index].gridStartDI + j].Cells["StateDI"].Style.BackColor = state ? Color.Red : Color.DimGray;
                                });
                            }
                        }
                        else
                        {
                            commOK = false;
                            portCOMM.DiscardInBuffer();
                        }
                    }
                }
            }
            catch {}
            command_busy = false;
            return commOK;
        }

        public bool GetInputState(int index, bool need_update)
        {
            if (need_update) ReadInputImediate();
            if (index >= 1 && index <= dgrDI.Rows.Count)
            {
                return Conv.atob(dgrDI.Rows[index - 1].Cells["StateDI"].Value);
            }
            return false;
        }
        
        public bool GetInputState(int index)
        {
            return GetInputState(index, false);
        }
        
        public bool GetOutputState(int index)
        {
            if (index >= 1 && index <= dgrDO.Rows.Count)
            {
                return Conv.atob(dgrDO.Rows[index - 1].Cells["State"].Value);
            }
            return false;
        }

        public bool SetOutput(int index, bool state)
        {
            return SetOutput(index, state, 0);
        }

        public bool SetOutput(int index, bool state, int ms_delay)
        {
            bool retValue = false;
            if (index >= 1 && index <= dgrDO.Rows.Count)
            {
                string frame = Conv.bytesToString(ModbusRTU_WriteCommandFrame(Conv.atoi32(dgrDO.Rows[index - 1].Cells["SiteID"].Value), Conv.atoi32(dgrDO.Rows[index - 1].Cells["RegNo"].Value), state ? 0x01 : 0x02, 0));
                retValue = (ExecuteCommand(frame, 120) != null);
                if (ms_delay > 0) 
                    Conv.delay_ms(ms_delay);
            }
            return retValue;
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            SetAllState(false);  
        }

        private void btnSetAll_Click(object sender, EventArgs e)
        {
            SetAllState(true);
        }

        private void dgrDO_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgrDO.Columns["State"].Index)
            {
                //if (Conv.atos(dgrDO.Rows[e.RowIndex].Cells["ErrSta"].Value) == "1" && dgrConn.Rows[e.RowIndex].Cells["ErrSta"].Style.BackColor == Color.Red)
                if (chkTestMode.Checked)
                    SetOutput(e.RowIndex + 1, !Conv.atob(dgrDO.Rows[e.RowIndex].Cells["State"].Value));
            }
        }
    }
}
