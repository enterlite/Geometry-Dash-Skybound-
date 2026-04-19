using System.Windows;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for UninstallConfirmWindow.xaml
    /// </summary>
    public partial class UninstallConfirmWindow : Window
    {
        public bool ConfirmUninstall { get; private set; } = false;
        public bool DeleteGameFiles { get; private set; } = false;

        public UninstallConfirmWindow()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmUninstall = true;
            DeleteGameFiles = DeleteFilesCheckBox.IsChecked ?? false;
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmUninstall = false;
            DeleteGameFiles = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}
