using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for RegisterAccountView.xaml
    /// </summary>
    public partial class RegisterAccountView : UserControl
    {
        private WebView2? _webView;

        public RegisterAccountView()
        {
            InitializeComponent();
            Loaded += RegisterAccountView_Loaded;
        }

        private async void RegisterAccountView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var container = this.FindName("WebViewContainer") as Grid;
                if (container == null)
                {
                    Debug.WriteLine("[RegisterAccountView] WebViewContainer nenalezeno");
                    return;
                }

                // Create WebView2
                _webView = new WebView2();
                container.Children.Add(_webView);

                // Initialize WebView2 environment
                await _webView.EnsureCoreWebView2Async(null);

                // Navigate to URL
                _webView.CoreWebView2.Navigate("https://enterlite.ps.fhgdps.com/dashboard/login/register.php");

                Debug.WriteLine("[RegisterAccountView] WebView2 inicializováno a navigováno");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterAccountView] Chyba: {ex.Message}");
                MessageBox.Show($"Chyba při načítání Register Account: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
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
                Debug.WriteLine($"[RegisterAccountView] Chyba při vyčištění: {ex.Message}");
            }
        }
    }
}
