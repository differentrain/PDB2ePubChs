using System.IO;

namespace PDB2ePubChs.HaoduPdbFiles.Internals
{
    internal sealed class PdbUnicode : PdbArchive
    {
        protected internal override int ChapterLengthFixed => 0;

        public PdbUnicode(BytesBuffer header, FileStream fs) : base(header, fs)
        {
        }

        internal override int FindNextChapterNameBuf(BytesBuffer buf, int startIndex, out BytesBuffer nameBuf)
        {
            unsafe
            {
                fixed (byte* p = buf.Buffer)
                {
                    bool foundCR = false;
                    char word;
                    int i = startIndex;
                    while (i < buf.Length)
                    {
                        word = GetReplacedChar((char*)(p + i));
                        if (foundCR)
                        {
                            if (word == '\n')
                                break;
                            foundCR = false;
                        }
                        else if (word == '\r')
                        {
                            foundCR = true;
                        }
                        i += 2;
                    }
                    nameBuf = buf.GetUtf8Buffer(startIndex, i - startIndex - 2);
                    return i + 2;
                }
            }
        }

        protected internal override int GetAuthorBytesCount(byte[] buf)
        {
            unsafe
            {
                fixed (byte* p = buf)
                {
                    int i = 0;
                    while (i < buf.Length)
                    {
                        if (GetReplacedChar((char*)(p + i)) == '\0')
                            break;
                        i += 2;
                    }
                    return i;
                }
            }
        }

        protected internal override int GetBookNameCount(byte[] buf, out int nameCount)
        {
            int i = 8;
            int len = buf.Length;
            int flag = 0;
            unsafe
            {
                fixed (byte* p = buf)
                {
                    while (i < len && flag < 3)
                    {
                        if (GetReplacedChar((char*)(p + i)) == '\u001b')
                            ++flag;
                        else
                            flag = 0;
                        i += 2;
                    }
                    nameCount = i - 8 - 6;
                    flag = 0;
                    while (i < len)
                    {
                        if (flag == 1)
                        {
                            if (p[i++] == 0)
                                break;
                            flag = 0;
                        }
                        else if (p[i++] == 27)
                        {
                            flag = 1;
                        }
                    }
                }
            }
            return i;
        }

        internal override BytesBuffer GetUtf8Buffer(BytesBuffer buf, int start, int length) => buf.GetUtf8Buffer(start, length);






    }
}
