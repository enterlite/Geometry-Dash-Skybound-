using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for DiscordServerView.xaml
    /// </summary>
    public partial class DiscordServerView : UserControl
    {
        private WebView2? _webView;

        public DiscordServerView()
        {
            InitializeComponent();
            Loaded += DiscordServerView_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        private async void DiscordServerView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var container = this.FindName("WebViewContainer") as Grid;
                if (container == null)
                {
                    Debug.WriteLine("[DiscordServerView] WebViewContainer not found");
                    return;
                }

                // Create WebView2
                _webView = new WebView2();
                container.Children.Add(_webView);

                // Initialize WebView2 environment
                await _webView.EnsureCoreWebView2Async(null);

                // Create HTML content with Discord widget
                string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            margin: 0;
            padding: 20px;
            background-color: #0f0f1e;
            display: flex;
            justify-content: center;
            align-items: flex-start;
            font-family: 'Segoe UI', Arial, sans-serif;
        }
        .container {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 20px;
        }
        h1 {
            color: #00d9ff;
            margin: 0;
            text-align: center;
        }
        .discord-widget {
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0, 217, 255, 0.2);
        }
        .join-button {
            background: linear-gradient(135deg, #00d9ff 0%, #0099cc 100%);
            color: white;
            border: none;
            padding: 12px 32px;
            font-size: 16px;
            border-radius: 6px;
            cursor: pointer;
            font-weight: bold;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0, 217, 255, 0.3);
        }
        .join-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 217, 255, 0.5);
        }
        .join-button:active {
            transform: translateY(0);
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🎮 Discord Server</h1>
        <div class='discord-widget'>
            <iframe src='https://discord.com/widget?id=1450787721931919382&theme=dark' width='350' height='500' allowtransparency='true' frameborder='0' sandbox='allow-popups allow-popups-to-escape-sandbox allow-same-origin allow-scripts'></iframe>
        </div>
        <a href='https://discord.gg/dYVMnAEMct' target='_blank' style='text-decoration: none;'>
            <button class='join-button'>🚀 Join Discord Server</button>
        </a>
    </div>
</body>
</html>";

                // Set the HTML content
                _webView.CoreWebView2.NavigateToString(htmlContent);

                Debug.WriteLine("[DiscordServerView] WebView2 initialized with Discord widget");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DiscordServerView] Error: {ex.Message}");
                MessageBox.Show($"Error loading Discord Server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Debug.WriteLine($"[DiscordServerView] Error cleaning up: {ex.Message}");
            }
        }
    }
}
