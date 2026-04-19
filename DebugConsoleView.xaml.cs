using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for DebugConsoleView.xaml
    /// </summary>
    public partial class DebugConsoleView : UserControl
    {
        private bool _autoScroll = true;

        public DebugConsoleView()
        {
            InitializeComponent();
            Loaded += DebugConsoleView_Loaded;
            Unloaded += DebugConsoleView_Unloaded;
        }

        private void DebugConsoleView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var debugTextBox = this.FindName("DebugTextBox") as TextBox;
                if (debugTextBox != null)
                {
                    // Subscribe to debug output
                    DebugOutputListener.OnDebugMessage += OnDebugMessageReceived;

                    // Display initial message
                    debugTextBox.AppendText("[DebugConsole] Monitoring system initialized\n");
                }

                Debug.WriteLine("[DebugConsoleView] Initialized");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleView] Error during initialization: {ex.Message}");
            }
        }

        private void DebugConsoleView_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DebugOutputListener.OnDebugMessage -= OnDebugMessageReceived;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleView] Error during unload: {ex.Message}");
            }
        }

        private void OnDebugMessageReceived(string message)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var debugTextBox = this.FindName("DebugTextBox") as TextBox;
                    if (debugTextBox != null)
                    {
                        debugTextBox.AppendText(message);

                        if (_autoScroll)
                        {
                            debugTextBox.ScrollToEnd();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleView] Error adding message: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var debugTextBox = this.FindName("DebugTextBox") as TextBox;
                if (debugTextBox != null)
                {
                    debugTextBox.Clear();
                    debugTextBox.AppendText("[DebugConsole] Content cleared\n");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleView] Error clearing: {ex.Message}");
            }
        }

        private void AutoScrollButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var autoScrollButton = this.FindName("AutoScrollButton") as Button;
                _autoScroll = !_autoScroll;

                if (autoScrollButton != null)
                {
                    autoScrollButton.Content = _autoScroll ? "Auto-scroll: ON" : "Auto-scroll: OFF";
                    autoScrollButton.Background = _autoScroll 
                        ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00d9ff"))
                        : new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#404040"));
                }

                Debug.WriteLine($"[DebugConsoleView] Auto-scroll: {_autoScroll}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleView] Error toggling auto-scroll: {ex.Message}");
            }
        }
    }
}
