using System.IO;

namespace PDB2ePubChs.HaoduPdbFiles.Internals
{
    internal sealed class PdbBig5 : PdbArchive
    {
        protected internal override int ChapterLengthFixed => -1;

        public PdbBig5(BytesBuffer header, FileStream fs) : base(header, fs)
        {
        }

        internal override int FindNextChapterNameBuf(BytesBuffer buf, int startIndex, out BytesBuffer nameBuf)
        {
            unsafe
            {
                fixed (byte* p = buf.Buffer)
                {
                    byte t;
                    int i = startIndex;
                    while (i < buf.Length)
                    {
                        t = p[i++];
                        if (t == 27)
                            break;
                        else if (t > 0x7F)
                            ++i;
                    }
                    nameBuf = GetUtf8Buffer(buf, startIndex, i - startIndex - 1);
                    return i;
                }
            }
        }

        protected internal override int GetAuthorBytesCount(byte[] buf) => 0;


        protected internal override int GetBookNameCount(byte[] buf, out int nameCount)
        {
            int i = 8;
            int len = buf.Length;
            int flag = 0;
            byte t;
            unsafe
            {
                fixed (byte* p = buf)
                {
                    while (i < len && flag < 3)
                    {
                        t = p[i++];
                        if (t == 27)
                            ++flag;
                        else
                        {
                            flag = 0;
                            if (t > 0x7F)
                                ++i;
                        }
                    }
                    nameCount = i - 8 - 3;
                    while (i < len)
                    {
                        t = p[i++];
                        if (t == 27)
                            break;
                        else if (t > 0x7F)
                            ++i;
                    }
                }
            }
            return i;
        }

        internal override BytesBuffer GetUtf8Buffer(BytesBuffer buf, int start, int length)
        {
            using (BytesBuffer unicode = buf.GetUnicodeBuffer(start, length))
            {
                unsafe
                {
                    fixed (byte* p = unicode.Buffer)
                    {
                        int i = 0;
                        char* pch = (char*)p;
                        int len = unicode.Length >> 1;
                        while (i < len)
                            _ = GetReplacedChar(pch + i++);
                    }
                }
                return unicode.GetUtf8Buffer(0, unicode.Length);
            }
        }

    }
}
