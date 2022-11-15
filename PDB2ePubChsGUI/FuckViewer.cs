using PDB2ePubChs.HaoduPdbFiles;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PDB2ePubChsGUI
{
    internal class FuckViewer : INotifyPropertyChanged
    {
        public static FuckViewer Instance { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;


        private bool _listEnabled = true;
        private bool _selEnabled = false;
        private bool _allEnabled = false;
        private bool _packChecked = false;
        private string _bookName = string.Empty;
        private string _author = string.Empty;

        public FuckViewer()
        {
            Archives.CollectionChanged += Archives_CollectionChanged;
            Instance = this;
        }

        private void Archives_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => AllEnabled = Archives.Count > 0;

        public FuckCommand CmdFucker { get; } = new FuckCommand();

        public bool SelEnabled
        {
            get => _selEnabled; set
            {
                if (_selEnabled != value)
                {
                    _selEnabled = value;
                    Invoke();
                    CmdFucker.Update();
                }
            }
        }

        public bool AllEnabled
        {
            get => _allEnabled; set
            {
                if (_allEnabled != value)
                {
                    _allEnabled = value;
                    Invoke();
                }
            }
        }

        public bool ListEnabled
        {
            get => _listEnabled; set
            {
                if (_listEnabled != value)
                {
                    _listEnabled = value;
                    Invoke();
                }
            }
        }

        public bool PackEnabled
        {
            get => _packChecked; set
            {
                if (_packChecked != value)
                {
                    _packChecked = value;
                    Invoke();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonOKEnabled)));
                }
            }
        }

        public string BookName
        {
            get => _bookName; set
            {
                if (_bookName != value)
                {
                    _bookName = value;
                    Invoke();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonOKEnabled)));
                }
            }
        }

        public bool ButtonOKEnabled => !PackEnabled || !string.IsNullOrWhiteSpace(BookName) || !string.IsNullOrWhiteSpace(Author);



        public string Author
        {
            get => _author; set
            {
                if (_author != value)
                {
                    _author = value;
                    Invoke();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonOKEnabled)));
                }
            }
        }

        public ObservableCollection<PdbArchive> Archives { get; } = new ObservableCollection<PdbArchive>();

        private void Invoke([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
