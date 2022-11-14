using PDB2ePubChs.HaoduPdbFiles.Internals;
using System;
using System.Text;

namespace PDB2ePubChs.HaoduPdbFiles
{
    public sealed class PdbChapterInfo : IDisposable
    {
        internal PdbChapterInfo(BytesBuffer nameBuffer, int position, int length)
        {
            NameBuffer = nameBuffer;
            Position = position;
            Length = length;
            ID = Guid.NewGuid();
        }

        internal readonly BytesBuffer NameBuffer;


        public Guid ID { get; }
        public string Name => Encoding.UTF8.GetString(NameBuffer, 0, NameBuffer.Length);
        public int Position { get; }
        public int Length { get; }


        #region dispose
        private bool disposedValue;
        void IDisposable.Dispose()
        {
            if (!disposedValue)
            {
                NameBuffer.Dispose();
                disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
