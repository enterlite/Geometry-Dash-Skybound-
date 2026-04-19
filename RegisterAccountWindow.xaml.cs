using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for RegisterAccountWindow.xaml
    /// </summary>
    public partial class RegisterAccountWindow : Window
    {
        private WebView2? _webView;
        private const string REGISTER_URL = "https://enterlite.ps.fhgdps.com/dashboard/login/register.php";

        public RegisterAccountWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var container = this.FindName("WebViewContainer") as Grid;
                
                if (container == null)
                {
                    Debug.WriteLine("[RegisterAccountWindow] WebViewContainer not found");
                    System.Windows.MessageBox.Show("Chyba: WebViewContainer se nepodařilo nalézt.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Create WebView2 instance programmatically
                _webView = new WebView2();
                container.Children.Add(_webView);
                
                Debug.WriteLine("[RegisterAccountWindow] WebView2 created programmatically");

                // Initialize WebView2
                await _webView.EnsureCoreWebView2Async(null);
                
                // Navigate to the URL
                _webView.CoreWebView2.Navigate(REGISTER_URL);
                
                Debug.WriteLine($"[RegisterAccountWindow] Navigated to: {REGISTER_URL}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterAccountWindow] Error loading WebView2: {ex.Message}");
                Debug.WriteLine($"[RegisterAccountWindow] Stack trace: {ex.StackTrace}");
                System.Windows.MessageBox.Show($"Chyba při načítání WebView2: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_webView != null)
                {
                    _webView.Dispose();
                    _webView = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterAccountWindow] Error during cleanup: {ex.Message}");
            }
        }
    }
}
