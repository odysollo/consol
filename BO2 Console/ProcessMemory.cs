using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace BO2_Console
{
    /// <summary>
    /// Process Memory management class written by meth for consol.cf.
    /// Attaching and detaching should be handled externally in your project.
    /// Ensure that the process is opened with access flag 0x001F0FFF.
    /// </summary>
    class ProcessMemory
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        public IntPtr ProcessHandle { get; private set; }

        public struct Float3
        {
            public float x;
            public float y;
            public float z;

            public Float3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        public struct Float4
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public Float4(float r, float g, float b, float a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
        }

        public ProcessMemory(IntPtr processHandle)
        {
            ProcessHandle = processHandle;
        }

        public bool WriteString(int lpBaseAddress, string mString)
        {
            byte[] lpBuffer = Encoding.ASCII.GetBytes(mString);
            return WriteBytes(lpBaseAddress, lpBuffer);
        }

        public bool WriteFloat(int lpBaseAddress, float mFloat)
        {
            byte[] lpBuffer = BitConverter.GetBytes(mFloat);
            return WriteBytes(lpBaseAddress, lpBuffer);
        }

        public bool WriteFloat3(int lpBaseAddress, Float3 mFloat3)
        {
            byte[] lpBuffer1 = BitConverter.GetBytes(mFloat3.x);
            byte[] lpBuffer2 = BitConverter.GetBytes(mFloat3.y);
            byte[] lpBuffer3 = BitConverter.GetBytes(mFloat3.z);
            byte[] lpBuffer = lpBuffer1.Concat(lpBuffer2).Concat(lpBuffer3).ToArray();
            return WriteBytes(lpBaseAddress, lpBuffer);
        }

        public bool WriteFloat4(int lpBaseAddress, Float4 mFloat4)
        {
            byte[] lpBuffer1 = BitConverter.GetBytes(mFloat4.r);
            byte[] lpBuffer2 = BitConverter.GetBytes(mFloat4.g);
            byte[] lpBuffer3 = BitConverter.GetBytes(mFloat4.b);
            byte[] lpBuffer4 = BitConverter.GetBytes(mFloat4.a);
            byte[] lpBuffer = lpBuffer1.Concat(lpBuffer2).Concat(lpBuffer3).Concat(lpBuffer4).ToArray();
            return WriteBytes(lpBaseAddress, lpBuffer);
        }

        public bool WriteInt(int lpBaseAddress, int mInt)
        {
            byte[] lpBuffer = BitConverter.GetBytes(mInt);
            return WriteBytes(lpBaseAddress, lpBuffer);
        }

        public bool WriteBool(int lpBaseAddress, bool mBool)
        {
            byte[] lpBuffer = BitConverter.GetBytes(mBool);
            return WriteBytes(lpBaseAddress, lpBuffer);
        }

        private bool WriteBytes(int lpBaseAddress, byte[] lpBuffer)
        {
            int lpNumberOfBytesWritten = 0;
            WriteProcessMemory((int)ProcessHandle, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesWritten);
            return lpNumberOfBytesWritten == lpBuffer.Length;
        }
    }
}