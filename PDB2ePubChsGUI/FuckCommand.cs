using PDB2ePubChs.HaoduPdbFiles;
using System;
using System.Windows.Input;

namespace PDB2ePubChsGUI
{
    internal class FuckCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Update()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter) => FuckViewer.Instance.SelEnabled;

        public void Execute(object parameter)
        {
            RemoveArchive(MainWindow.MainListBox.SelectedItems);
        }


        public static void RemoveArchive(System.Collections.IList archives)
        {
            PdbArchive ar;
            while (archives.Count > 0)
            {
                ar = archives[0] as PdbArchive;
                _ = FuckViewer.Instance.Archives.Remove(ar);
                ar.Dispose();
            }
        }
    }
}
