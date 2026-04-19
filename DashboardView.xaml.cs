using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private WebView2? _webView;

        public DashboardView()
        {
            InitializeComponent();
            Loaded += DashboardView_Loaded;
        }

        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var container = this.FindName("WebViewContainer") as Grid;
                if (container == null)
                {
                    Debug.WriteLine("[DashboardView] WebViewContainer not found");
                    return;
                }

                // Create WebView2
                _webView = new WebView2();
                container.Children.Add(_webView);

                // Initialize WebView2 environment
                await _webView.EnsureCoreWebView2Async(null);

                // Navigate to URL
                _webView.CoreWebView2.Navigate("https://enterlite.ps.fhgdps.com/dashboard/");

                Debug.WriteLine("[DashboardView] WebView2 initialized and navigated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardView] Error: {ex.Message}");
                MessageBox.Show($"Error loading Dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Debug.WriteLine($"[DashboardView] Error cleaning up: {ex.Message}");
            }
        }
    }
}
