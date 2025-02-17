using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xemuh2stats
{
    public class MemoryHandler
    {
        #region Flags
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        [Flags]
        public enum MemoryProtection : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        #endregion
        #region Externs
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public ulong RegionSize;
            public uint State;
            public MemoryProtection Protect;
            public uint Type;
        }


        [DllImport("user32.dll")]
        public static extern short GetKeyState(Keys nVirtKey);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        //public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, Uint nSize, out int lpNumberOfBytesWritten);
        public static extern bool WriteProcessMemory(int hProcess, long lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(int hObject);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(int hProcess, int lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(int hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        // Find window by Caption only. Note you must pass 0 as the first parameter.
        public static extern int FindWindowByCaption(int ZeroOnly, string lpWindowName);
        //public static extern short GetKeyState(VirtualKeyStates nVirtKey);
        #endregion
        #region Const

        const uint PAGE_NOACCESS = 1;
        const uint PAGE_READONLY = 2;
        const uint PAGE_READWRITE = 4;
        const uint PAGE_WRITECOPY = 8;
        const uint PAGE_EXECUTE = 16;
        const uint PAGE_EXECUTE_READ = 32;
        const uint PAGE_EXECUTE_READWRITE = 64;
        const uint PAGE_EXECUTE_WRITECOPY = 128;
        const uint PAGE_GUARD = 256;
        const uint PAGE_NOCACHE = 512;
        const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        #endregion

        private Process mainProcess { get; set; }
        public int processID { get; set; }
        public int processHandle { get; set; }
        public long processBase { get; set; }

        public MemoryHandler(Process process)
        {
            mainProcess = process;
            processID = process.Id;
            processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, mainProcess.Id);
            processBase = (long)process.MainModule.BaseAddress;
        }

        public bool ProcessIsRunning()
        {
            mainProcess.Refresh();
            return mainProcess.HasExited;
        }
        public int DllImageAddress(int handle, string dllname)
        {
            foreach (ProcessModule module in mainProcess.Modules)
                if (module.FileName.EndsWith(dllname))
                    return (int)module.BaseAddress;
            return -1;
        }

        public string NullTerminateString(string mystring)
        {
            try
            {
                char[] mychar = mystring.ToCharArray();
                string returnstring = "";
                for (int i = 0; i < mystring.Length; i++)
                    if (mychar[i] == '\0') return returnstring;
                    else if (mychar.Length == i) return returnstring;
                    else returnstring += mychar[i].ToString();
                return returnstring;
            }
            catch
            {
                return mystring.TrimEnd('0');
            }
        }

        private List<MEMORY_BASIC_INFORMATION> _QueriedMemoryRegions = new List<MEMORY_BASIC_INFORMATION>();

        public List<MEMORY_BASIC_INFORMATION> QueryMemoryRegions()
        {
            if (_QueriedMemoryRegions.Count == 0)
            {
                IntPtr _base = IntPtr.Zero;
                MEMORY_BASIC_INFORMATION memInfo;
                while (VirtualQueryEx(this.processHandle, _base, out memInfo,
                           (uint) Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
                {
                    _QueriedMemoryRegions.Add(memInfo);
                    _base = (IntPtr) ((ulong) _base + memInfo.RegionSize);
                }
            }

            return _QueriedMemoryRegions;
        }

        public long ImageAddress(long pOffset)
        {
            return processBase + pOffset;
        }


        public byte[] ReadMemory(bool addToBaseAddress, long pOffset, int pSize)
        {
            byte[] buffer = new byte[pSize];
            ReadProcessMemory(processHandle, addToBaseAddress ? ImageAddress(pOffset) : pOffset, buffer, pSize, 0);
            return buffer;
        }

        public void WriteMemory(bool addToBaseAddress, long pOffset, byte[] pBytes)
        {
            WriteProcessMemory(processHandle, addToBaseAddress ? ImageAddress(pOffset) : pOffset, pBytes, pBytes.Length,
                0);
        }

        public int Pointer(bool addToBase, params int[] pOffsets)
        {
            var startPointer = ReadInt(pOffsets[0], true);
            for (var i = 1; i < pOffsets.Length; i++)
                if (i == pOffsets.Length - 1)
                    startPointer += pOffsets[i];
                else
                    startPointer = ReadInt(startPointer + pOffsets[i]);
            return startPointer;
        }

        public int BlamCachePointer(int cacheOffset)
        {
            return Pointer(true, 0x4A29BC, cacheOffset);
        }
        #region Read Memory

        public byte ReadByte(long pOffset, bool addToBase = false) =>
            ReadMemory(addToBase, pOffset, 1)[0];

        public bool ReadBool(long pOffset, bool addToBase = false) =>
            ReadByte(pOffset, addToBase) == 1;

        public short ReadShort(long pOffset, bool addToBase = false) =>
            BitConverter.ToInt16(ReadMemory(addToBase, pOffset, 2), 0);

        public ushort ReadUShort(long pOffset, bool addToBase = false) =>
            BitConverter.ToUInt16(ReadMemory(addToBase, pOffset, 2), 0);

        public int ReadInt(long pOffset, bool addToBase = false) =>
            BitConverter.ToInt32(ReadMemory(addToBase, pOffset, 4), 0);

        public uint ReadUInt(long pOffset, bool addToBase = false) =>
            BitConverter.ToUInt32(ReadMemory(addToBase, pOffset, 4), 0);

        public long ReadLong(long pOffset, bool addToBase = false) =>
            BitConverter.ToInt64(ReadMemory(addToBase, pOffset, 8), 0);

        public ulong ReadULong(long pOffset, bool addToBase = false) =>
            BitConverter.ToUInt64(ReadMemory(addToBase, pOffset, 8), 0);

        public float ReadFloat(long pOffset, bool addToBase = false) =>
            BitConverter.ToSingle(ReadMemory(addToBase, pOffset, 4), 0);

        public double ReadDouble(long pOffset, bool addToBase = false) =>
            BitConverter.ToDouble(ReadMemory(addToBase, pOffset, 8), 0);

        public string ReadStringAscii(long pOffset, int length, bool addToBase = false) =>
            NullTerminateString(Encoding.ASCII.GetString(ReadMemory(addToBase, pOffset, length)));

        public string ReadStringUnicode(long pOffset, int length, bool addToBase = false) =>
            NullTerminateString(Encoding.Unicode.GetString(ReadMemory(addToBase, pOffset, length * 2)));

        #endregion
        #region Write Memory

        public void WriteByte(long pOffset, byte pByte, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, new byte[] { pByte });

        public void WriteBool(long pOffset, bool pByte, bool addToBase = false) =>
            WriteByte(pOffset, (pByte) ? (byte)1 : (byte)0, addToBase);

        public void WriteShort(long pOffset, short pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteUShort(long pOffset, ushort pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteInt(long pOffset, int pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteUInt(long pOffset, uint pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteLong(long pOffset, long pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteULong(long pOffset, ulong pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteFloat(long pOffset, float pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteDouble(long pOffset, double pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, BitConverter.GetBytes(pBytes));

        public void WriteStringAscii(long pOffset, string pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, Encoding.ASCII.GetBytes(pBytes));

        public void WriteStringUnicode(long pOffset, string pBytes, bool addToBase = false) =>
            WriteMemory(addToBase, pOffset, Encoding.Unicode.GetBytes(pBytes));

        #endregion

        #region pattern
        public static unsafe List<IntPtr> PatternScan(byte[] buffer, byte[] pattern, byte[] mask, IntPtr baseAddress, int offset)
        {
            var addresses = new List<IntPtr>();

            fixed (byte* bufferPtr = buffer)
            {
                for (int i = 0; i <= buffer.Length - pattern.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (mask[j] == 1 && bufferPtr[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        addresses.Add(baseAddress + i + offset);
                    }
                }
            }

            return addresses;
        }

        public async Task<List<IntPtr>> ScanProcessAsync(byte[] pattern, byte[] mask)
        {
            var matchAddresses = new List<IntPtr>();

            Process process = this.mainProcess;
            IntPtr baseAddress = IntPtr.Zero;
            MEMORY_BASIC_INFORMATION memInfo;
            List<MEMORY_BASIC_INFORMATION> regions = QueryMemoryRegions().Where(x => x.RegionSize == 0x4000000 || x.RegionSize == 0x8000000).ToList();
            int chunkSize = 8192;
            

            foreach (MEMORY_BASIC_INFORMATION region in regions)
            {
                byte[] buffer = new byte[chunkSize];
                for (ulong offset = 0; offset < region.RegionSize; offset += (ulong) chunkSize)
                {
                    if (ReadProcessMemory(this.processHandle,  (long)((ulong)region.BaseAddress + offset), buffer, chunkSize, 0))
                    {
                        var addresses = await Task.Run(() => PatternScan(buffer, pattern, mask, region.BaseAddress, (int)offset));
                        matchAddresses.AddRange(addresses);
                    }
                }
            }
            return matchAddresses;
        }

        #endregion

        public T ReadStruct<T>(long pOffset, bool addToBase = false)
        {
            //// Pin the managed memory while, copy it out the data, then unpin it
            //GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            //handle.Free();
            //return theStructure;
            int size = Marshal.SizeOf<T>();
            byte[] buffer = ReadMemory(addToBase, pOffset, size);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
