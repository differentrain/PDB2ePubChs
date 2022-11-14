using System.Windows;

namespace PDB2ePubChsGUI
{
    /// <summary>
    /// WindowSaveConfig.xaml 的交互逻辑
    /// </summary>
    public partial class WindowSaveConfig : Window
    {
        public static readonly WindowSaveConfig Instance = new WindowSaveConfig();

        public WindowSaveConfig()
        {
            InitializeComponent();
        }

        public bool Closeable = false;

        public bool Result = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Closeable;
            Result = false;
            Hide();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Hide();
        }


    }
}
