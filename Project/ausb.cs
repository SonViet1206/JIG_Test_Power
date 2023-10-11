using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DefaultNS //ausbdll
{
    public static class ausb {
        [DllImport("ausb.dll", EntryPoint = "ausb_start")]
        public extern static int start(uint dwTmout);
        [DllImport("ausb.dll", EntryPoint = "ausb_open")]
        public extern static int open(ref uint hDev, uint dwMyid);
        [DllImport("ausb.dll", EntryPoint = "ausb_write")]
        public extern static int ausb_write32(uint hDev, uint pWrtBuffer, uint dwCount);
        [DllImport("ausb.dll", EntryPoint = "ausb_write")]
        public extern static int ausb_write64(uint hDev, ulong pWrtBuffer, uint dwCount);
        [DllImport("ausb.dll", EntryPoint = "ausb_read")]
        public extern static int ausb_read32(uint hDev, uint pRdBuffer, uint dwCount, ref uint pRdCnt);
        [DllImport("ausb.dll", EntryPoint = "ausb_read")]
        public extern static int ausb_read64(uint hDev, ulong pRdBuffer, uint dwCount, ref uint pRdCnt);
        [DllImport("ausb.dll", EntryPoint = "ausb_close")]
        public extern static int close(uint dDev);
        [DllImport("ausb.dll", EntryPoint = "ausb_end")]
        public extern static int end();
        [DllImport("ausb.dll", EntryPoint = "ausb_clear")]
        public extern static int clear(uint hDev);
        [DllImport("ausb.dll", EntryPoint = "ausb_trigger")]
        public extern static int trigger(uint hDev);
        [DllImport("ausb.dll", EntryPoint = "ausb_readstb")]
        public extern static int readstb(uint hDev, ref int lngSTB);
        [DllImport("ausb.dll", EntryPoint = "ausb_timeout")]
        public extern static int timeout(uint Tmout);
        [DllImport("ausb.dll", EntryPoint = "ausb_local")]
        public extern static int local(uint hDev);
        [DllImport("ausb.dll", EntryPoint = "ausb_llo")]
        public extern static int llo(uint hDev);
        [DllImport("ausb.dll", EntryPoint = "ausb_reset")]
        public extern static int reset(uint hDev);

        public static int write(uint hDev, string strCmd) {
            uint CmdCnt;
            int i;
            ulong pBuf;
            Console.WriteLine(strCmd);
            CmdCnt = (uint)strCmd.Length;

            byte[] pBuffer;
            pBuffer = new byte[CmdCnt + 1];

            for (i = 0; i < CmdCnt; i++) {
                pBuffer[i] = (byte)strCmd[i];
            }
            int ret = 0;
            unsafe {
                fixed (byte* p = &pBuffer[0]) pBuf = (ulong)p;
                if (IntPtr.Size == 4) {
                    ret = ausb_write32(hDev, (uint)pBuf, CmdCnt);
                }
                else {
                    ret = ausb_write64(hDev, pBuf, CmdCnt);
                }
            }
            return ret;
        }

        public static int read(uint hDev, ref string ReadDt, ref uint RdCnt, uint lngCnt = 256) {
            int ret;
            ulong pBuf;
            byte[] pBuffer;
            pBuffer = new byte[lngCnt + 1];
            unsafe {
                fixed (byte* p = &pBuffer[0]) {
                    pBuf = (ulong)p;
                    if (IntPtr.Size == 4) {
                        ret = ausb_read32(hDev, (uint)pBuf, lngCnt, ref RdCnt);
                    }
                    else {
                        ret = ausb_read64(hDev, pBuf, lngCnt, ref RdCnt);
                    }

                    ReadDt = "";
                    if (ret == 0) {
                        string tmps = System.Text.Encoding.Default.GetString(pBuffer);
                        ReadDt = tmps.TrimEnd(new char[] { '\r', '\n', '\0' });
                    }
                }
            }
            return ret;
        }
    }
}
