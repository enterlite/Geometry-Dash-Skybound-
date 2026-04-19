using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        private WebView2? _webView;
        private const string DASHBOARD_URL = "https://enterlite.ps.fhgdps.com/dashboard/";

        public DashboardWindow()
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
                    Debug.WriteLine("[DashboardWindow] WebViewContainer not found");
                    System.Windows.MessageBox.Show("Error: WebViewContainer not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Create WebView2 instance programmatically
                _webView = new WebView2();
                container.Children.Add(_webView);
                
                Debug.WriteLine("[DashboardWindow] WebView2 created programmatically");

                // Initialize WebView2
                await _webView.EnsureCoreWebView2Async(null);
                
                // Navigate to the URL
                _webView.CoreWebView2.Navigate(DASHBOARD_URL);
                
                Debug.WriteLine($"[DashboardWindow] Navigated to: {DASHBOARD_URL}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardWindow] Error loading WebView2: {ex.Message}");
                Debug.WriteLine($"[DashboardWindow] Stack trace: {ex.StackTrace}");
                System.Windows.MessageBox.Show($"Error loading WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Debug.WriteLine($"[DashboardWindow] Error during cleanup: {ex.Message}");
            }
        }
    }
}
