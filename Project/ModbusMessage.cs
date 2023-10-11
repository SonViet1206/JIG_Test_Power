using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DefaultNS
{
    public static class ModbusASCIIMessage
    {
        const byte FUN_01_READ_COIL_STATUS = 1;
        const byte FUN_02_READ_INPUT_STATUS = 2;
        const byte FUN_03_READ_HOLDING_REGISTER = 3;
        const byte FUN_04_READ_INPUT_REGISTER = 4;
        const byte FUN_05_WRITE_SINGLE_COIL = 5;
        const byte FUN_06_WRITE_SINGLE_REGISTER = 6;
        const byte FUN_15_WRITE_MULTI_COILS = 15;
        const byte FUN_16_WRITE_MULTI_REGISTERS = 16;
        const byte FUN_17__REPORT_SLAVE_ID = 17;

        private const char Header = ':';
        private const char CR = '\r';
        private const char LF = '\n';
        private const string Trailer = "\r\n"; // string.Empty + CR;

        public static string Read(byte slaveAddress, byte functionCode, uint startAddress, uint numberOfPoints)
        {
            string frame = string.Format("{0:X2}", slaveAddress);
            frame += string.Format("{0:X2}", functionCode);
            frame += string.Format("{0:X4}", startAddress);
            frame += string.Format("{0:X4}", numberOfPoints);
            byte[] bytes = Conv.HexToBytes(frame);
            byte lrc = LRC(bytes);
            return Header + frame + lrc.ToString("X2") + Trailer;
        }

        public static string Write(byte slaveAddress, byte functionCode, uint startAddress, byte[] value)
        {
            string frame = string.Format("{0:X2}", slaveAddress); // Địa chỉ slave.
            frame += string.Format("{0:X2}", functionCode); // Mã hàm modbus.
            frame += string.Format("{0:X4}", startAddress); // Địa chỉ bắt đầu của coil.
            frame += string.Format("{0:X2}", value[0]); // Dữ liệu cần ghi xuống coil.
            frame += string.Format("{0:X2}", value[1]); // Dữ liệu cần ghi xuống coil.
            byte[] bytes = Conv.HexToBytes(frame);
            byte lrc = LRC(bytes);
            return Header + frame + lrc.ToString("X2") + Trailer;
        }

        public static string WriteAll(byte slaveAddress, byte functionCode, uint startAddress, byte[] values)
        {
            string frame = string.Format("{0:X2}", slaveAddress); // Địa chỉ slave.
            frame += string.Format("{0:X2}", functionCode); // Mã hàm modbus.
            frame += string.Format("{0:X4}", startAddress); // Địa chỉ bắt đầu của coils.
            frame += string.Format("{0:X4}", (functionCode == 15) ? values.Length * 8 : values.Length / 2); // Số lượng coils.
            frame += string.Format("{0:X2}", values.Length); // Số byte cần ghi.
            foreach (byte item in values)
            {
                frame += string.Format("{0:X2}", item);
            }
            byte[] bytes = Conv.HexToBytes(frame);
            byte lrc = LRC(bytes);
            return Header + frame + lrc.ToString("X2") + Trailer;
        }

        private static byte LRC(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("Đối số truyền vào là null không hợp lệ");
            byte lrc = 0;
            foreach (byte byt in data)
                lrc += byt;
            lrc = (byte)((lrc ^ 0xFF) + 1);
            return lrc;
        }

        public static string ReadCoilStatusMessage(byte slaveAddress, uint startAddress, ushort numberOfPoints)
        {
            try
            {
                string frame = string.Format("{0:X2}", slaveAddress);   // Slave Address
                frame += string.Format("{0:X2}", FUN_01_READ_COIL_STATUS);          // Function  
                frame += string.Format("{0:X4}", startAddress);         // Starting Address
                frame += string.Format("{0:X4}", numberOfPoints);       // Quantity of Coils.
                byte[] bytes = Conv.HexToBytes(frame);            // Calculate LRC.
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ReadInputStatusMessage(byte slaveAddress, uint startAddress, ushort numberOfPoints)
        {
            try
            {
                string frame = string.Format("{0:X2}", slaveAddress);      // Slave Address
                frame += string.Format("{0:X2}", FUN_02_READ_INPUT_STATUS);             // Function  
                frame += string.Format("{0:X4}", startAddress);            // Starting Address
                frame += string.Format("{0:X4}", numberOfPoints);          // Quantity of Coils.
                byte[] bytes = Conv.HexToBytes(frame);               // Calculate LRC.
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ReadHoldingRegistersMessage(byte slaveAddress, uint startAddress, ushort numberOfPoints)
        {
            return Read(slaveAddress, FUN_03_READ_HOLDING_REGISTER, startAddress, numberOfPoints);
        }

        public static string ReadInputRegistersMessage(byte slaveAddress, uint startAddress, ushort numberOfPoints)
        {
            try
            {
                string frame = string.Format("{0:X2}", slaveAddress);   // Slave Address
                frame += string.Format("{0:X2}", FUN_04_READ_INPUT_REGISTER);          // Function  
                frame += string.Format("{0:X4}", startAddress);         // Starting Address
                frame += string.Format("{0:X4}", numberOfPoints);       // Quantity of Coils.
                byte[] bytes = Conv.HexToBytes(frame);            // Calculate LRC.
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string calc_crc(byte[] ptbuf, int num)
        {
            string retValue = "";
            uint crc16 = 0xffff;
            uint temp;
            uint flag;

            for (int i = 0; i < num; i++)
            {
                temp = (uint)ptbuf[i]; // temp has the first byte 
                temp &= 0x00ff; // mask the MSB 
                crc16 = crc16 ^ temp; //crc16 XOR with temp 
                for (uint c = 0; c < 8; c++)
                {
                    flag = crc16 & 0x01; // LSBit di crc16 is mantained
                    crc16 = crc16 >> 1; // Lsbit di crc16 is lost 
                    if (flag != 0)
                        crc16 = crc16 ^ 0x0a001; // crc16 XOR with 0x0a001 
                }
            }
            //crc16 = (crc16 >> 8) | (crc16 << 8); // LSB is exchanged with MSB
            retValue += (char)(crc16 & 0xFF);
            retValue += (char)((crc16 >> 8) & 0xFF);
            return retValue;
        }

        public static string WriteSingleCoilMessage(byte slaveAddress, uint startAddress, bool value)
        {
            try
            {
                string frame = "";
                
                //frame = string.Format("{0:X2}", slaveAddress);   // Slave Address.
                //frame += string.Format("{0:X2}", FUN_05_WRITE_SINGLE_COIL);          // Function.
                //frame += string.Format("{0:X4}", startAddress);         // Coil Address.
                //frame += string.Format("{0:X4}", value ? 65280 : 0);    // Write Data.
                //byte[] bytes = Conv.HexToBytes(frame);
                //byte lrc = LRC(bytes);
                //return Header + frame + lrc.ToString("X2") + Trailer;

                frame += (char)slaveAddress; // Slave Address.
                frame += (char)FUN_05_WRITE_SINGLE_COIL; // Function.
                frame += (char)((startAddress >> 8) & 0xFF); // Coil Address.
                frame += (char)(startAddress & 0xFF); // Coil Address.
                frame += value ? (char)0xFF : (char)0x0; // Write Data.
                frame += (char)0x0; // Write Data.
                frame += calc_crc(Conv.stringToBytes(frame), frame.Length);

                //string sBuff = Conv.S2SH(frame);

                return frame;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string WriteMultipleCoilsMessage(byte slaveAddress, uint startAddress, byte[] values)
        {
            try
            {
                string frame = "";

                frame = string.Format("{0:X2}", slaveAddress); // Slave Address.
                frame += string.Format("{0:X2}", FUN_15_WRITE_MULTI_COILS); // Function.
                frame += string.Format("{0:X4}", startAddress); // Coil Address.
                frame += string.Format("{0:X4}", values.Length * 8); // (FUN_15_WRITE_MULTI_COILS == 15) ? (values.Length * 8) : (values.Length / 2)); // Quantity of Coils.
                frame += string.Format("{0:X2}", values.Length); // Byte Count.
                foreach (byte item in values)
                {
                    frame += string.Format("{0:X2}", item); // Write Data
                }
                byte[] bytes = Conv.HexToBytes(frame);
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string WriteSingleRegisterMessage(byte slaveAddress, uint startAddress, byte[] values)
        {
            try
            {
                string frame = string.Format("{0:X2}", slaveAddress);   // Slave Address.
                frame += string.Format("{0:X2}", FUN_06_WRITE_SINGLE_REGISTER);         // Function.
                frame += string.Format("{0:X4}", startAddress);         // Register Address.
                foreach (byte item in values)
                {
                    frame += string.Format("{0:X2}", item);               // Write Data.
                }                
                byte[] bytes = Conv.HexToBytes(frame);
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string WriteMultipleRegistersMessage(byte slaveAddress, uint startAddress, byte[] values)
        {
            try
            {
                string frame = string.Format("{0:X2}", slaveAddress); // Slave Address.
                frame += string.Format("{0:X2}", FUN_16_WRITE_MULTI_REGISTERS); // Function.
                frame += string.Format("{0:X4}", startAddress); // Starting Address.
                frame += string.Format("{0:X4}", (FUN_16_WRITE_MULTI_REGISTERS == 15) ? values.Length * 8 : values.Length / 2); // Quantity of Registers.
                frame += string.Format("{0:X2}", values.Length); // Byte Count.
                foreach (byte item in values)
                {
                    frame += string.Format("{0:X2}", item); // Write Data
                }
                byte[] bytes = Conv.HexToBytes(frame);
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Command Code：17, Report Slave ID
        /// </summary>
        /// <param name="slaveAddress">Địa chỉ slave</param>
        /// <returns>Trả về chuỗi message</returns>
        public static string ReportSlaveIDMessage(byte slaveAddress)
        {
            try
            {
                string frame = string.Format("{0:X2}", slaveAddress); // Slave Address.
                frame += string.Format("{0:X2}", FUN_17__REPORT_SLAVE_ID); // Function.
                byte[] bytes = Conv.HexToBytes(frame);
                byte lrc = LRC(bytes);
                return Header + frame + lrc.ToString("X2") + Trailer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
