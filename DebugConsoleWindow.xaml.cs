using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for DebugConsoleWindow.xaml
    /// </summary>
    public partial class DebugConsoleWindow : Window
    {
        private bool _autoScroll = true;
        private TextBox? _debugTextBox;

        public DebugConsoleWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _debugTextBox = this.FindName("DebugTextBox") as TextBox;

                if (_debugTextBox == null)
                {
                    Debug.WriteLine("[DebugConsoleWindow] DebugTextBox not found");
                    MessageBox.Show("Error: TextBox not found.", "Error");
                    this.Close();
                    return;
                }

                // Subscribe to debug output listener
                DebugOutputListener.OnDebugMessage += OnDebugMessageReceived;

                Debug.WriteLine("[DebugConsoleWindow] Debug console initialized");
                _debugTextBox.AppendText("[DebugConsoleWindow] Debug console started\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleWindow] Error during loading: {ex.Message}");
            }
        }

        private void OnDebugMessageReceived(string message)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (_debugTextBox != null)
                    {
                        _debugTextBox.AppendText(message);

                        if (_autoScroll)
                        {
                            _debugTextBox.ScrollToEnd();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleWindow] Error adding message: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_debugTextBox != null)
            {
                _debugTextBox.Clear();
                Debug.WriteLine("[DebugConsoleWindow] Debug console cleared");
            }
        }

        private void AutoScrollButton_Click(object sender, RoutedEventArgs e)
        {
            _autoScroll = !_autoScroll;

            if (sender is Button button)
            {
                button.Content = _autoScroll ? "Auto-scroll: ON" : "Auto-scroll: OFF";
                button.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(
                        _autoScroll ? "#00d9ff" : "#ff6b6b"));
            }

            Debug.WriteLine($"[DebugConsoleWindow] Auto-scroll: {(_autoScroll ? "ON" : "OFF")}");
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
                // Unsubscribe from debug listener
                DebugOutputListener.OnDebugMessage -= OnDebugMessageReceived;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugConsoleWindow] Error during closing: {ex.Message}");
            }
        }
    }
}
