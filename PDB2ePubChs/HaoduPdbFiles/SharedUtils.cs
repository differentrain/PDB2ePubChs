using System.IO;

namespace PDB2ePubChs.HaoduPdbFiles
{
    public static class SharedUtils
    {
        public static string GetDirPath(this string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            return fi.DirectoryName;
        }

        public static PdbArchive[] GetUPDBList(this string dir)
        {
            DirectoryInfo directory = new DirectoryInfo(dir);
            FileInfo[] fis = directory.GetFiles("*.updb", SearchOption.AllDirectories);
            if (fis.Length == 0)
                throw new FileNotFoundException("没有找到updb文件。");

            PdbArchive[] archives = new PdbArchive[fis.Length];
            for (int i = 0; i < fis.Length; i++)
                archives[i] = PdbArchive.Open(fis[i].FullName);
            return archives;
        }

    }
}
