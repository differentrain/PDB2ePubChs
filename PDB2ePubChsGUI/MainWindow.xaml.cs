using Microsoft.Win32;
using PDB2ePubChs.HaoduPdbFiles;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace PDB2ePubChsGUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ListBox MainListBox { get; private set; }


        private static readonly OpenFileDialog s_op = new OpenFileDialog()
        {
            CheckPathExists = true,
            CheckFileExists = true,
            Multiselect = true,
            Filter = "好读Updb文件|*.updb",
            Title = "打开uPdb文件"
        };

        private static readonly SaveFileDialog s_sp = new SaveFileDialog()
        {
            CheckPathExists = true,
            Filter = "好读Updb文件|*.updb",
            Title = "保存到"
        };

        private static readonly System.Windows.Forms.FolderBrowserDialog s_fp = new System.Windows.Forms.FolderBrowserDialog();


        public MainWindow()
        {
            InitializeComponent();
            MainListBox = ListBoxMain;


        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (s_op.ShowDialog() == true)
            {
                var list = ListBoxMain.ItemsSource as ObservableCollection<PdbArchive>;
                for (int i = 0; i < s_op.FileNames.Length; i++)
                {
                    try
                    {
                        list.Add(new PdbArchive(s_op.FileNames[i]));
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void ListBoxMain_SelectionChanged(object sender, SelectionChangedEventArgs e) => FuckViewer.Instance.SelEnabled = ListBoxMain.SelectedItems.Count > 0;

        private void ButtonConvSel_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxMain.SelectedItems.Count == 1)
            {
                var ar = ListBoxMain.SelectedItems[0] as PdbArchive;
                s_sp.FileName = $@"《{ar.BookName}》{ar.Author}.epub";
                if (s_sp.ShowDialog() == true)
                {
                    try
                    {
                        Pdb2Epub.CreateEpub(ar, s_sp.FileName);
                        _ = MessageBox.Show("转换成功");
                    }
                    catch (Exception er)
                    {
                        _ = MessageBox.Show(er.Message);
                    }
                    finally
                    {
                        _ = FuckViewer.Instance.Archives.Remove(ar);
                        ar.Dispose();
                    }

                }
            }
            else
            {
                ConvMulti(ListBoxMain.SelectedItems);
            }
        }

        private void ButtonConvAll_Click(object sender, RoutedEventArgs e)
        {
            ConvMulti(ListBoxMain.Items);
        }

        private void ConvMulti(System.Collections.IList archives)
        {
            _ = WindowSaveConfig.Instance.ShowDialog();
            if (WindowSaveConfig.Instance.Result)
            {
                if (FuckViewer.Instance.PackEnabled)
                {
                    var bn = FuckViewer.Instance.BookName.Trim();
                    var an = FuckViewer.Instance.Author.Trim();
                    var t = $@"《{bn}》{an}.epub";
                    if (t.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0)
                        s_sp.FileName = t;
                    if (s_sp.ShowDialog() == true)
                    {
                        try
                        {
                            Pdb2Epub.CreateEpub(s_sp.FileName, bn, archives, string.IsNullOrWhiteSpace(an) ? null : an);
                            _ = MessageBox.Show("转换成功");
                        }
                        catch (Exception er)
                        {
                            _ = MessageBox.Show(er.Message);
                        }
                        finally
                        {
                            FuckCommand.RemoveArchive(archives);
                        }

                    }
                }
                else if (s_fp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var dir = s_fp.SelectedPath;
                        _ = Parallel.For(0, archives.Count, i =>
                        {
                            PdbArchive t = archives[i] as PdbArchive;
                            Pdb2Epub.CreateEpub(t, $@"{dir}\《{t.BookName}》{t.Author}.epub");
                        });
                        _ = MessageBox.Show("转换成功");
                    }
                    catch (Exception er)
                    {
                        _ = MessageBox.Show(er.Message);
                    }
                    finally
                    {
                        FuckCommand.RemoveArchive(archives);
                    }
                }
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowSaveConfig.Instance.Closeable = true;
            WindowSaveConfig.Instance.Close();
        }
    }
}
