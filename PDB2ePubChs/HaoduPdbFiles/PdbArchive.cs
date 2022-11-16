using PDB2ePubChs.HaoduPdbFiles.Internals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace PDB2ePubChs.HaoduPdbFiles
{
    public abstract class PdbArchive : IDisposable
    {
        private static readonly Dictionary<char, char> s_unicodeDict;
        private static readonly Dictionary<byte, byte[]> s_utf8Table_1;
        private static readonly Dictionary<short, byte[]> s_utf8Table_2;
        private static readonly Dictionary<byte, Dictionary<short, byte[]>> s_utf8Table_3;
        private static readonly Dictionary<int, byte[]> s_utf8Table_4;
        private static readonly ReplacedWord[] s_replaceWords;

        // <p>
        private static readonly byte[] s_pStart = { 0x3C, 0x70, 0x3E };
        // </p>
        private static readonly byte[] s_pEnd = { 0x3C, 0x2F, 0x70, 0x3E };
        // </p><p>
        private static readonly byte[] s_pEndStart = { 0x3C, 0x2F, 0x70, 0x3E, 0x3C, 0x70, 0x3E };

        static unsafe PdbArchive()
        {
            // auto gen
            ReplacedChar pattern;
            using (XmlReader reader = XmlReader.Create("ReplacedChars.xml"))
            {
                var xml = new XmlSerializer(typeof(ReplacedChar[]));
                var rc = xml.Deserialize(reader) as ReplacedChar[];
                s_unicodeDict = new Dictionary<char, char>(rc.Length);
                var l = new List<ReplacedWord>(rc.Length);
                for (int i = 0; i < rc.Length; i++)
                {
                    pattern = rc[i];
                    if (pattern.Org.Length == 1)
                        s_unicodeDict.Add(pattern.Org[0], pattern.Rep[0]);
                    else
                        l.Add(new ReplacedWord(pattern));
                }
                s_replaceWords = l.ToArray();
            }
            s_utf8Table_1 = new Dictionary<byte, byte[]>(s_unicodeDict.Count);
            s_utf8Table_2 = new Dictionary<short, byte[]>(s_unicodeDict.Count);
            s_utf8Table_3 = new Dictionary<byte, Dictionary<short, byte[]>>(s_unicodeDict.Count);
            s_utf8Table_4 = new Dictionary<int, byte[]>(s_unicodeDict.Count);
            char ukey, uvalue;
            int keyBytesCount, valueBytesCount;
            Dictionary<short, byte[]> dict;
            byte* pbuffer = stackalloc byte[8];
            foreach (KeyValuePair<char, char> item in s_unicodeDict)
            {
                ukey = item.Key;
                uvalue = item.Value;
                keyBytesCount = Encoding.UTF8.GetBytes(&ukey, 1, pbuffer, 4);
                valueBytesCount = Encoding.UTF8.GetBytes(&uvalue, 1, pbuffer + 4, 4);
                switch (keyBytesCount)
                {
                    case 1:
                        s_utf8Table_1.Add(pbuffer[0], Utils.CreateBytes(pbuffer + 4, valueBytesCount));
                        break;
                    case 2:
                        s_utf8Table_2.Add(*(short*)pbuffer, Utils.CreateBytes(pbuffer + 4, valueBytesCount));
                        break;
                    case 3:
                        if (!s_utf8Table_3.TryGetValue(pbuffer[0], out dict))
                        {
                            dict = new Dictionary<short, byte[]>(s_unicodeDict.Count);
                            s_utf8Table_3.Add(pbuffer[0], dict);
                        }
                        dict.Add(*(short*)(pbuffer + 1), Utils.CreateBytes(pbuffer + 4, valueBytesCount));
                        break;
                    default: // 4
                        s_utf8Table_4.Add(*(int*)pbuffer, Utils.CreateBytes(pbuffer + 4, valueBytesCount));
                        break;
                }
            }
        }


        private readonly FileStream _fs;
        internal readonly BytesBuffer AuthorBuffer;
        internal readonly BytesBuffer BookNameBuffer;

        private readonly int _elementCount;


        internal PdbArchive(BytesBuffer header, FileStream fs)
        {
            BytesBuffer tempBuf = null;
            BytesBuffer tempBuf2 = null;
            try
            {
                _fs = fs;
                // header
                tempBuf = header;
                _elementCount = tempBuf.ReadBigEndianUInt16(76);
                AuthorBuffer = GetUtf8Buffer(tempBuf, 0, GetAuthorBytesCount(tempBuf));
                tempBuf.Dispose();
                // indexes
                tempBuf = BytesBuffer.CreateFromStream(_fs, _elementCount << 3);
                // catalogue
                tempBuf2 = BytesBuffer.CreateFromStream(_fs, tempBuf.ReadBigEndianInt32(8) - tempBuf.ReadBigEndianInt32(0));
                int nextIndex = GetBookNameCount(tempBuf2, out int nameCount);
                BookNameBuffer = GetUtf8Buffer(tempBuf2, 8, nameCount);
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


        protected internal abstract int ChapterLengthFixed { get; }

        internal abstract BytesBuffer GetUtf8Buffer(BytesBuffer buf, int start, int length);

        protected internal abstract int GetAuthorBytesCount(byte[] buf);

        protected internal abstract int GetBookNameCount(byte[] buf, out int nameCount);

        internal abstract int FindNextChapterNameBuf(BytesBuffer buf, int startIndex, out BytesBuffer nameBuf);

        internal MemoryStream CreateHtmlStream(PdbChapterInfo chapter/*, bool seek*/)
        {
            // todo: add seek for random accessing
            // if (seek)
            //   _fs.Seek(chapter.Position, SeekOrigin.Begin);
            BytesBuffer temp = BytesBuffer.CreateFromStream(_fs, chapter.Length);
            BytesBuffer utf8Buf = GetUtf8Buffer(temp, 0, temp.Length + ChapterLengthFixed);
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


        public static PdbArchive Open(string path)
        {
            FileStream fs = null;
            BytesBuffer header = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                header = BytesBuffer.CreateFromStream(fs, 78);
                return header.ReadBigEndianInt32(64) == 0x4D544954
                    ? new PdbBig5(header, fs)
                    : header.ReadBigEndianInt32(64) == 0x4D544955 ? (PdbArchive)new PdbUnicode(header, fs) : throw Utils.Unsupported_Pdb_File;
            }
            catch
            {
                fs?.Dispose();
                header?.Dispose();
                throw;
            }
        }


        private void AppendAndProcess(BytesBuffer utf8Buf, MemoryStream ms)
        {
            var length = utf8Buf.Length;
            int i = 0;
            byte ch;
            byte[] bs;
            ms.Write(s_pStart, 0, 3);
            unsafe
            {
                fixed (byte* pOld = utf8Buf.Buffer)
                    while (i < length)
                        if ((ch = pOld[i]) == 13) // /r/n
                        {
                            ms.Write(s_pEndStart, 0, 7);
                            i += 2;
                        }
                        else if (ch <= 0x7F) // 1
                        {
                            if (s_utf8Table_1.TryGetValue(ch, out bs))
                                ms.Write(bs, 0, bs.Length);
                            else
                                ms.WriteByte(ch);
                            ++i;
                        }
                        else if (ch <= 0xDF) // 2
                        {
                            if (s_utf8Table_2.TryGetValue(*(short*)(pOld + i), out bs))
                                ms.Write(bs, 0, bs.Length);
                            else
                            {
                                ms.WriteByte(ch);
                                ms.WriteByte(*(pOld + 1));
                            }
                            i += 2;
                        }
                        else if (ch <= 0xEF) // 3
                        {
                            if (s_utf8Table_3.TryGetValue(ch, out Dictionary<short, byte[]> dict) &&
                                dict.TryGetValue(*(short*)(pOld + i + 1), out bs))
                                ms.Write(bs, 0, bs.Length);
                            else
                            {
                                ms.WriteByte(ch);
                                ms.WriteByte(*(pOld + i + 1));
                                ms.WriteByte(*(pOld + i + 2));
                            }
                            i += 3;
                        }
                        else   // 4
                        {
                            if (s_utf8Table_4.TryGetValue(*(int*)(pOld + i), out bs))
                                ms.Write(bs, 0, bs.Length);
                            else
                            {
                                ms.WriteByte(ch);
                                ms.WriteByte(*(pOld + i + 1));
                                ms.WriteByte(*(pOld + i + 2));
                                ms.WriteByte(*(pOld + i + 3));
                            }
                            i += 4;
                        }
            }
            ms.Write(s_pEnd, 0, 4);
        }


        private PdbChapterInfo[] CreateChapters(BytesBuffer indexes, BytesBuffer catalogue, int startIndex, int chapterCount)
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

        protected static unsafe char GetReplacedChar(char* p) => s_unicodeDict.TryGetValue(*p, out char nw) ? *p = nw : *p;

        internal static bool GetReplacedString(BytesBuffer unicode, int start, int length, out BytesBuffer replaced)
        {
            replaced = null;
            if (s_replaceWords.Length == 0)
                return false;
            unsafe
            {
                fixed (byte* p = unicode.Buffer)
                {
                    var str = new string((char*)(p + start), 0, length >> 1);
                    ReplacedWord rw;
                    for (int i = 0; i < s_replaceWords.Length; i++)
                    {
                        rw = s_replaceWords[i];
                        str = rw.Reg.Replace(str, rw.Pattern);
                    }
                    replaced = BytesBuffer.CreateFromString(str);
                    return true;
                }
            }

        }

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
