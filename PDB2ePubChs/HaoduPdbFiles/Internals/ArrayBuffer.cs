using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PDB2ePubChs.HaoduPdbFiles.Internals
{
    internal class ArrayBuffer<T> : IDisposable
        where T : unmanaged
    {

        public ArrayBuffer(int count)
        {
            Buffer = ArrayPool<T>.Shared.Rent(count);
            Length = count;
        }

        protected ArrayBuffer(T[] buffer, int length)
        {
            Buffer = buffer;
            Length = length;
        }


        public T[] Buffer { get; protected set; }
        public int Length { get; internal set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadBigEndianUInt16(int startIndex)
        {
            unsafe
            {
                fixed (T* ptr = Buffer)
                    return Utils.ReadBigEndianUInt16((byte*)(ptr + startIndex));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadBigEndianInt32(int startIndex)
        {
            unsafe
            {
                fixed (T* ptr = Buffer)
                    return Utils.ReadBigEndianInt32((byte*)(ptr + startIndex));
            }
        }

        public static implicit operator T[](ArrayBuffer<T> v) => v.Buffer;


        public void Dispose()
        {
            if (Buffer != null)
            {
                ArrayPool<T>.Shared.Return(Buffer);
                Buffer = null;
                GC.SuppressFinalize(this);
            }
        }

#if DEBUG
        ~ArrayBuffer() => Debug.Assert(true, "have not been disposed.");
#endif


    }


}
