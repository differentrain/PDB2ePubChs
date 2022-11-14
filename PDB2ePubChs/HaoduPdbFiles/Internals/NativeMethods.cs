using System;
using System.Runtime.InteropServices;

namespace PDB2ePubChs.HaoduPdbFiles.Internals
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern unsafe int LCMapStringEx(string lpLocaleName, uint dwMapFlags, byte* lpSrcStr, int cchSrc, [Out] byte[] lpDestStr, int cchDest, IntPtr lpVersionInformation, IntPtr lpReserved, IntPtr sortHandle);

        public const string LOCALE_NAME_INVARIANT = "";
        public const uint LCMAP_SIMPLIFIED_CHINESE = 0x02000000;

    }
}
