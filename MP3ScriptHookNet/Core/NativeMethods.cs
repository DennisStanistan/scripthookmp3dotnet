using System;
using System.Runtime.InteropServices;

namespace MP3ScriptHookNet.Core
{
    internal class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten); // Used for shellcode injection.

        [DllImport("kernel32.dll")]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect); // Allocate memory.

        [DllImport("kernel32.dll")]
        internal static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum AllocationType
    {
        /// <summary>
        /// 
        /// </summary>
        Commit = 0x00001000,
        /// <summary>
        /// 
        /// </summary>
        Reserve = 0x00002000,
        /// <summary>
        /// 
        /// </summary>
        Decommit = 0x00004000,
        /// <summary>
        /// 
        /// </summary>
        Release = 0x00008000,
        /// <summary>
        /// 
        /// </summary>
        Reset = 0x00080000,
        /// <summary>
        /// 
        /// </summary>
        TopDown = 0x00100000,
        /// <summary>
        /// 
        /// </summary>
        WriteWatch = 0x00200000,
        /// <summary>
        /// 
        /// </summary>
        Physical = 0x00400000,
        /// <summary>
        /// 
        /// </summary>
        LargePages = 0x20000000
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum MemoryProtection
    {
        /// <summary>
        /// 
        /// </summary>
        NoAccess = 0x0001,
        /// <summary>
        /// 
        /// </summary>
        ReadOnly = 0x0002,
        /// <summary>
        /// 
        /// </summary>
        ReadWrite = 0x0004,
        /// <summary>
        /// 
        /// </summary>
        WriteCopy = 0x0008,
        /// <summary>
        /// 
        /// </summary>
        Execute = 0x0010,
        /// <summary>
        /// 
        /// </summary>
        ExecuteRead = 0x0020,
        /// <summary>
        /// 
        /// </summary>
        ExecuteReadWrite = 0x0040,
        /// <summary>
        /// 
        /// </summary>
        ExecuteWriteCopy = 0x0080,
        /// <summary>
        /// 
        /// </summary>
        GuardModifierflag = 0x0100,
        /// <summary>
        /// 
        /// </summary>
        NoCacheModifierflag = 0x0200,
        /// <summary>
        /// 
        /// </summary>
        WriteCombineModifierflag = 0x0400
    }
}
