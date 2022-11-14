using PDB2ePubChs.HaoduPdbFiles.Internals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PDB2ePubChs.HaoduPdbFiles
{
    public sealed class PdbArchive : IDisposable
    {
        // todo: add unicode chars here
        private static readonly Dictionary<char, char> s_vericalDict = new Dictionary<char, char>() {
            {'︿','〈'}, {'﹀','〉'}, {'︽','《'}, {'︾','》'}, {'︹','〔'}, {'︺','〕'}, {'︻','【'},
            {'︼','】'}, {'﹃','‘'}, {'﹄','’'}, {'﹁','“'}, {'﹂','”'}, {'︷','｛'}, {'︸','｝'},
            {'︵','（'}, {'︶','）'}, {'｜','—'}, {'│','…'}, {'︙','…'},  {'︱', '—' }
        };

        // <p>
        private static readonly byte[] s_pStart = { 0x3C, 0x70, 0x3E };
        // </p>
        private static readonly byte[] s_pEnd = { 0x3C, 0x2F, 0x70, 0x3E };
        // </p><p>
        private static readonly byte[] s_pEndStart = { 0x3C, 0x2F, 0x70, 0x3E, 0x3C, 0x70, 0x3E };


        //private static readonly byte[] s_hor_char_EF_0 = { 0xE3, 0x80, 0x88 }; // ︿〈
        //private static readonly byte[] s_hor_char_EF_1 = { 0xE3, 0x80, 0x89 }; // ﹀〉
        //private static readonly byte[] s_hor_char_EF_2 = { 0xE3, 0x80, 0x8A }; // ︽《
        //private static readonly byte[] s_hor_char_EF_3 = { 0xE3, 0x80, 0x8B }; // ︾》
        //private static readonly byte[] s_hor_char_EF_4 = { 0xE3, 0x80, 0x94 }; // ︹〔
        //private static readonly byte[] s_hor_char_EF_5 = { 0xE3, 0x80, 0x95 }; // ︺〕
        //private static readonly byte[] s_hor_char_EF_6 = { 0xE3, 0x80, 0x90 }; // ︻【
        //private static readonly byte[] s_hor_char_EF_7 = { 0xE3, 0x80, 0x91 }; // ︼】
        //private static readonly byte[] s_hor_char_EF_8 = { 0xE2, 0x80, 0x98 }; // ﹃‘
        //private static readonly byte[] s_hor_char_EF_9 = { 0xE2, 0x80, 0x99 }; // ﹄’
        //private static readonly byte[] s_hor_char_EF_10 = { 0xE2, 0x80, 0x9C }; // ﹁“
        //private static readonly byte[] s_hor_char_EF_11 = { 0xE2, 0x80, 0x9D }; // ﹂”
        //private static readonly byte[] s_hor_char_EF_12 = { 0xEF, 0xBD, 0x9B }; // ︷｛
        //private static readonly byte[] s_hor_char_EF_13 = { 0xEF, 0xBD, 0x9D }; // ︸｝
        //private static readonly byte[] s_hor_char_EF_14 = { 0xEF, 0xBC, 0x88 }; // ︵（
        //private static readonly byte[] s_hor_char_EF_15 = { 0xEF, 0xBC, 0x89 }; // ︶）
        //private static readonly byte[] s_hor_char_EF_16 = { 0xE2, 0x80, 0x94 }; // ｜—   ︱—
        //private static readonly byte[] s_hor_char_EF_17 = { 0xE2, 0x80, 0xA6 }; // ︙…



        private readonly FileStream _fs;
        internal readonly BytesBuffer AuthorBuffer;
        internal readonly BytesBuffer BookNameBuffer;

        private readonly int _elementCount;

        public PdbArchive(string path)
        {
            BytesBuffer tempBuf = null;
            BytesBuffer tempBuf2 = null;
            try
            {
                _fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                // header
                tempBuf = ReadAndEusureHeader(_fs);
                _elementCount = tempBuf.ReadBigEndianUInt16(76);
                AuthorBuffer = tempBuf.GetUtf8Buffer(0, GetAuthorBytesCount(tempBuf));
                tempBuf.Dispose();
                // indexes
                tempBuf = BytesBuffer.CreateFromStream(_fs, _elementCount << 3);
                // catalogue
                tempBuf2 = BytesBuffer.CreateFromStream(_fs, tempBuf.ReadBigEndianInt32(8) - tempBuf.ReadBigEndianInt32(0));
                int nextIndex = GetBookNameCount(tempBuf2, out int nameCount);
                BookNameBuffer = tempBuf2.GetUtf8Buffer(8, nameCount);
                Chapters = CreateChapters(tempBuf, tempBuf2, nextIndex, _elementCount - 2);
                ID = Guid.NewGuid();
            }
            catch
            {
                _fs?.Dispose();
                AuthorBuffer?.Dispose();
                BookNameBuffer?.Dispose();
                throw;
            }
            finally
            {
                tempBuf?.Dispose();
                tempBuf2?.Dispose();
            }
        }

        public Guid ID { get; }

        public string Author => Encoding.UTF8.GetString(AuthorBuffer, 0, AuthorBuffer.Length);

        public string BookName => Encoding.UTF8.GetString(BookNameBuffer, 0, BookNameBuffer.Length);

        public PdbChapterInfo[] Chapters { get; }

        internal MemoryStream CreateHtmlStream(PdbChapterInfo chapter/*, bool seek*/)
        {
            // todo: add seek for random accessing
            // if (seek)
            //   _fs.Seek(chapter.Position, SeekOrigin.Begin);
            BytesBuffer temp = BytesBuffer.CreateFromStream(_fs, chapter.Length);
            BytesBuffer utf8Buf = temp.GetUtf8Buffer(0, temp.Length);
            temp.Dispose();
            MemoryStream ms = Utils.MSManager.GetStream();
            ms.Write(Utils.Chapter_Start, 0, Utils.Chapter_Start.Length);
            ms.Write(chapter.NameBuffer, 0, chapter.NameBuffer.Length);
            ms.Write(Utils.Chapter_Middle, 0, Utils.Chapter_Middle.Length);
            AppendAndProcess(utf8Buf, ms);
            ms.Write(Utils.Chapter_End, 0, Utils.Chapter_End.Length);
            utf8Buf.Dispose();
            // todo: ms has been seeked to begin here
            _ = ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private void AppendAndProcess(BytesBuffer utf8Buf, MemoryStream ms)
        {
            var length = utf8Buf.Length;
            int i = 0;
            byte ch;
            ushort t;
            ms.Write(s_pStart, 0, 3);
            unsafe
            {
                fixed (byte* pOld = utf8Buf.Buffer)
                    while (i < length)
                        switch (ch = pOld[i])
                        {
                            case 13: // /r/n
                                ms.Write(s_pEndStart, 0, 7);
                                i += 2;
                                break;
                            case 0xEF:
                                t = GetNewValueEF(pOld + i + 1, out ch);
                                ms.WriteByte(ch);
                                ms.WriteByte(*(byte*)&t);
                                ms.WriteByte(*((byte*)&t + 1));
                                i += 3;
                                break;
                            case 0xE2:
                                t = GetNewValueE2(pOld + i + 1);
                                ms.WriteByte(0xE2);
                                ms.WriteByte(*(byte*)&t);
                                ms.WriteByte(*((byte*)&t + 1));
                                i += 3;
                                break;
                            default:
                                ms.WriteByte(ch); // 1
                                ++i;
                                if (ch > 0x7F) // 2
                                    ms.WriteByte(*(pOld + i++));
                                if (ch > 0xDF) // 3
                                    ms.WriteByte(*(pOld + i++));
                                if (ch > 0xEF) // 4
                                    ms.WriteByte(*(pOld + i++));
                                break;
                        }
            }
            ms.Write(s_pEnd, 0, 4);
        }

        // todo: add utf8 chars here
        private unsafe ushort GetNewValueEF(byte* p, out byte first)
        {
            var v = Utils.ReadBigEndianUInt16(p);
            switch (v)
            {
                case 0xB8BF: // ︿〈
                    first = 0xE3;
                    v = 0x8088;
                    break;
                case 0xB980: // ﹀〉
                    first = 0xE3;
                    v = 0x8089;
                    break;
                case 0xB8BD: // ︽《
                    first = 0xE3;
                    v = 0x808A;
                    break;
                case 0xB8BE: // ︾》
                    first = 0xE3;
                    v = 0x808B;
                    break;
                case 0xB8B9: // ︹〔
                    first = 0xE3;
                    v = 0x8094;
                    break;
                case 0xB8BA: // ︺〕
                    first = 0xE3;
                    v = 0x8095;
                    break;
                case 0xB8BB: // ︻【
                    first = 0xE3;
                    v = 0x8090;
                    break;
                case 0xB8BC: // ︼】
                    first = 0xE3;
                    v = 0x8091;
                    break;
                case 0xB983: // ﹃‘
                    first = 0xE2;
                    v = 0x8098;
                    break;
                case 0xB984: // ﹄’
                    first = 0xE2;
                    v = 0x8099;
                    break;
                case 0xB981: // ﹁“
                    first = 0xE2;
                    v = 0x809C;
                    break;
                case 0xB982: // ﹂”
                    first = 0xE2;
                    v = 0x809D;
                    break;
                case 0xB8B7: // ︷｛
                    first = 0xEF;
                    v = 0xBD9B;
                    break;
                case 0xB8B8: // ︸｝
                    first = 0xEF;
                    v = 0xBD9D;
                    break;
                case 0xB8B5: // ︵（
                    first = 0xEF;
                    v = 0xBC88;
                    break;
                case 0xB8B6: // ︶）
                    first = 0xEF;
                    v = 0xBC89;
                    break;
                case 0xBD9C: // ｜—
                case 0xB8B1: // ︱—
                    first = 0xE2;
                    v = 0x8094;
                    break;
                case 0xB899: // ︙…
                    first = 0xE2;
                    v = 0x80A6;
                    break;
                default:
                    first = 0xEF;
                    break;
            }
            return BitConverter.IsLittleEndian ? (ushort)((v >> 8) | (v << 8)) : v;

        }
        private unsafe ushort GetNewValueE2(byte* p)
        {
            var v = Utils.ReadBigEndianUInt16(p);
            if (v == 0x9482) // │…
                v = 0x80A6;
            return BitConverter.IsLittleEndian ? (ushort)((v >> 8) | (v << 8)) : v;
        }

        private BytesBuffer ReadAndEusureHeader(Stream stream)
        {
            BytesBuffer buf = BytesBuffer.CreateFromStream(stream, 78);
            if (buf.ReadBigEndianInt32(64) != 0x4D544955) //MITU
            {
                buf.Dispose();
                throw Utils.Unsupported_Pdb_File;
            }
            return buf;
        }

        private static int GetAuthorBytesCount(byte[] buf)
        {
            unsafe
            {
                fixed (byte* p = buf)
                {
                    int i = 0;
                    while (i < buf.Length)
                    {
                        if (ReadReplace((char*)(p + i)) == '\0')
                            break;
                        i += 2;
                    }
                    return i;
                }
            }
        }

        private static int GetBookNameCount(byte[] buf, out int nameCount)
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
                        if (ReadReplace((char*)(p + i)) == '\u001b')
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

        private static PdbChapterInfo[] CreateChapters(BytesBuffer indexes, BytesBuffer catalogue, int startIndex, int chapterCount)
        {
            var chapters = new PdbChapterInfo[chapterCount];
            int i = 0;
            int idx = 8;
            int a, b;
            while (i < chapterCount)
            {
                a = indexes.ReadBigEndianInt32(idx);
                b = indexes.ReadBigEndianInt32(idx + 8);
                startIndex = FindNextChapterNameBuf(catalogue, startIndex, out BytesBuffer nameBuf);
                chapters[i] = new PdbChapterInfo(nameBuf, a, b - a);
                ++i;
                idx += 8;
            }
            return chapters;
        }

        private static int FindNextChapterNameBuf(BytesBuffer buf, int startIndex, out BytesBuffer nameBuf)
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
                        word = ReadReplace((char*)(p + i));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe char ReadReplace(char* p) => s_vericalDict.TryGetValue(*p, out char newCh) ? *p = newCh : *p;


        #region dispose

        private bool disposedValue;

        public void Dispose()
        {
            if (!disposedValue)
            {
                _fs.Dispose();
                BookNameBuffer.Dispose();
                AuthorBuffer.Dispose();
                for (int i = 0; i < Chapters.Length; i++)
                    (Chapters[i] as IDisposable).Dispose();
                disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }

        public override string ToString() => $"《{BookName}》{Author}";


#if DEBUG
        ~PdbArchive() => Debug.Assert(true, "have not been disposed.");
#endif
        #endregion

    }
}
