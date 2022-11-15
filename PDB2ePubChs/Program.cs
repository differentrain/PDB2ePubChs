using PDB2ePubChs.HaoduPdbFiles;
using PDB2ePubChs.HaoduPdbFiles.Internals;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PDB2ePubChs
{
    internal class Program
    {
        // var x = new PdbArchive(@"C:\Users\diffe\Desktop\17S4.updb");
        private static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine(Utils.ConsoleHelper);
                return;
            }

            switch (args[0].Trim().ToLower())
            {
                case "-c":
                    ConvertFile(args);
                    return;
                case "-l":
                    ConvertFileList(args);
                    return;
                case "-p":
                    PackFileList(args);
                    return;
                default:
                    Console.WriteLine(Utils.ConsoleHelper);
                    return;
            }

        }

        private static void ConvertFile(string[] args)
        {
            PdbArchive archive = null;
            try
            {
                string customAuthorName = null;
                string path;
                if (args[1].Trim().ToLower() == "-a")
                {
                    customAuthorName = args[2].Trim().ToLower();
                    archive = PdbArchive.Open(args[3]);
                    path = args.Length > 4 ? args[4] : $@"{args[3].GetDirPath()}\《{archive.BookName}》{customAuthorName}.epub";
                }
                else
                {
                    archive = PdbArchive.Open(args[1]);
                    path = args.Length > 2 ? args[2] : $@"{args[1].GetDirPath()}\《{archive.BookName}》{archive.Author}.epub";

                }
                Console.WriteLine("转换中...");
                Pdb2Epub.CreateEpub(archive, path, customAuthorName);
                Console.WriteLine("完成。");
            }
            catch (Exception e)
            {
                Console.WriteLine("错误：");
                Console.WriteLine(e.Message);
            }
            finally
            {
                archive?.Dispose();
            }
        }

        private static void ConvertFileList(string[] args)
        {
            PdbArchive[] archives = null;
            string dir = args[1];
            try
            {
                archives = dir.GetUPDBList();
                if (args.Length > 2)
                    dir = args[2];

                _ = Directory.CreateDirectory(dir);


                Console.WriteLine("转换中...");
                _ = Parallel.ForEach(archives, archive =>
                {
                    Pdb2Epub.CreateEpub(archive, $@"{dir}\《{archive.BookName}》{archive.Author}.epub");
                });
                Console.WriteLine("完成。");
            }
            catch (Exception e)
            {
                Console.WriteLine("错误：");
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (archives != null)
                    for (int i = 0; i < archives.Length; i++)
                        archives[i].Dispose();
            }
        }

        private static void PackFileList(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine(Utils.ConsoleHelper);
                return;
            }

            PdbArchive[] archives = null;
            string bookName = args[1];
            string t = args[2];
            string author = null;
            string dir, output;
            if (t.Trim().ToLower() == "-a")
            {
                if (args.Length < 5)
                {
                    Console.WriteLine(Utils.ConsoleHelper);
                    return;
                }
                author = args[3];
                dir = args[4];
                output = args.Length > 5 ? args[5] : $@"{dir}\《{bookName}》{author}.epub";
            }
            else
            {
                dir = t;
                output = args.Length > 3 ? args[3] : $@"{dir}\《{bookName}》{author}.epub";
            }
            try
            {
                archives = dir.GetUPDBList();
                Console.WriteLine("转换中...");
                Pdb2Epub.CreateEpub(output, bookName, archives, author);
                Console.WriteLine("完成。");
            }
            catch (Exception e)
            {
                Console.WriteLine("错误：");
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (archives != null)
                    for (int i = 0; i < archives.Length; i++)
                        archives[i].Dispose();
            }
        }
    }
}
