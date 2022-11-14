﻿using System;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PDB2ePubChs.HaoduPdbFiles.Internals
{
    internal sealed class BytesBuffer : ArrayBuffer<byte>
    {
        public BytesBuffer(int count) : base(count) { }

        private BytesBuffer(byte[] buffer, int length) : base(buffer, length) { }

        public BytesBuffer GetUtf8Buffer(int startIndex, int count)
        {
            using (BytesBuffer nb = GetUnicodeBuffer(startIndex, count))
            {
                int newLen = nb.Length << 1;
                byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newLen);
                if (newLen > 0)
                {
                    unsafe
                    {
                        fixed (byte* pOrg = nb.Buffer, pNew = newBuffer)
                            newLen = Encoding.UTF8.GetBytes((char*)pOrg, nb.Length >> 1, pNew, newLen);
                    }
                }
                return new BytesBuffer(newBuffer, newLen);
            }
        }

        private BytesBuffer GetUnicodeBuffer(int startIndex, int count)
        {
            var length = count >> 1;

            unsafe
            {
                fixed (byte* p = Buffer)
                {
                    var destlen = NativeMethods.LCMapStringEx(
                         NativeMethods.LOCALE_NAME_INVARIANT,
                         NativeMethods.LCMAP_SIMPLIFIED_CHINESE,
                         p + startIndex,
                         length,
                         null,
                         0,
                         IntPtr.Zero,
                         IntPtr.Zero,
                         IntPtr.Zero);

                    byte[] chsBuf = ArrayPool<byte>.Shared.Rent(destlen << 1);

                    if (NativeMethods.LCMapStringEx(
                         NativeMethods.LOCALE_NAME_INVARIANT,
                         NativeMethods.LCMAP_SIMPLIFIED_CHINESE,
                         p + startIndex,
                         length,
                         chsBuf,
                         destlen,
                         IntPtr.Zero,
                         IntPtr.Zero,
                         IntPtr.Zero) == 0 && length != 0)
                    {
                        ArrayPool<byte>.Shared.Return(chsBuf);
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    return new BytesBuffer(chsBuf, destlen << 1);
                }
            }
        }

        public static BytesBuffer CreateFromStream(Stream stream, int length)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            int len;
            try
            {
                len = stream.Read(buffer, 0, length);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }
            if (len != length)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw Utils.Invalid_Pdb_File;
            }
            return new BytesBuffer(buffer, length);
        }

        public static BytesBuffer CreateFromString(string str)
        {
            int length = str.Length;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length << 2);
            length = Encoding.UTF8.GetBytes(str, 0, length, buffer, 0);
            return new BytesBuffer(buffer, length);
        }

    }
}
