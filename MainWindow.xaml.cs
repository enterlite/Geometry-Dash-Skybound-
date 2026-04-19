using SkyboundLauncher.Models;
using SkyboundLauncher.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Media;
using SkyboundLauncher.Services;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using SDImage = System.Drawing.Image;

namespace SkyboundLauncher
{
    /// <summary>
    /// Converter from LauncherState to Visibility for XAML binding.
    /// </summary>
    public class StateToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is LauncherState state && parameter is string paramState)
            {
                var expectedState = Enum.Parse<LauncherState>(paramState);
                return state == expectedState ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for lookup in language dictionary.
    /// </summary>
    public class DictionaryValueConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Dictionary<string, string> dict && parameter is string key)
            {
                return dict.ContainsKey(key) ? dict[key] : key;
            }
            return parameter?.ToString() ?? "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;
        private VideoBackgroundManager? _videoManager;
        private UIElement? _currentView;
        private string? _currentSelectedTab;

        public MainWindow()
        {
            InitializeComponent();

            // Set ViewModel
            _viewModel = new MainWindowViewModel();
            this.DataContext = _viewModel;

            // Initialize video manager
            InitializeVideoBackground();

            // Initialize system tray
            InitializeSystemTray();
            
            // Hook up animations start
            this.Loaded += (s, e) => StartAnimations();
        }

        private void StartAnimations()
        {
            try
            {
                // Start gradient animation
                var gradientAnimation = this.FindResource("GradientAnimation") as Storyboard;
                if (gradientAnimation != null)
                {
                    gradientAnimation.Begin();
                    Debug.WriteLine("[MainWindow] GradientAnimation started");
                }

                // Start spinner animation
                var spinnerAnimation = this.FindResource("SpinnerRotation") as Storyboard;
                if (spinnerAnimation != null)
                {
                    spinnerAnimation.Begin();
                    Debug.WriteLine("[MainWindow] SpinnerRotation started");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error starting animations: {ex.Message}");
            }
        }



    /// <summary>
    /// Initializes video playback in the background.
    /// </summary>
    private void InitializeVideoBackground()
    {
        try
        {
            var videoBackground = this.FindName("VideoBackground") as MediaElement;

            if (videoBackground == null)
            {
                Debug.WriteLine("[MainWindow] VideoBackground element not found in XAML");
                return;
            }

            _videoManager = new VideoBackgroundManager();
            _videoManager.Initialize(videoBackground);

            // Loading video - relative path
            string videoPath = "image/BC.mp4";
            _videoManager.LoadVideo(videoPath);
            
            Debug.WriteLine($"[MainWindow] Video initialized: {videoPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MainWindow] Error during video initialization: {ex.Message}");
            var videoBackground = this.FindName("VideoBackground") as MediaElement;
            if (videoBackground != null)
                videoBackground.Visibility = Visibility.Collapsed;
        }
    }

    private void InitializeSystemTray()
    {
        try
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Open", null, (s, e) =>
            {
                try
                {
                    Dispatcher.Invoke(() => ShowWindow());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error while opening: {ex.Message}");
                }
            });
            _contextMenu.Items.Add("Close Application", null, (s, e) => 
            {
                try
                {
                    Dispatcher.Invoke(() => ExitApplication());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error while closing: {ex.Message}");
                }
            });

            // Create NotifyIcon with default icon
            _notifyIcon = new NotifyIcon
            {
                Text = "Skybound Launcher",
                ContextMenuStrip = _contextMenu
            };

            // Try loading icon from file
            bool iconLoaded = false;
            try
            {
                // Try several paths
                string[] possiblePaths = new[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image", "sb.ico"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sb.ico"),
                    System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "image", "sb.ico"),
                    System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "sb.ico"),
                };

                foreach (string iconPath in possiblePaths)
                {
                    System.Diagnostics.Debug.WriteLine($"Attempting to load icon: {iconPath}");
                    if (System.IO.File.Exists(iconPath))
                    {
                        try
                        {
                            var customIcon = new Icon(iconPath);
                            _notifyIcon.Icon = customIcon;
                            iconLoaded = true;
                            System.Diagnostics.Debug.WriteLine($"Icon successfully loaded: {iconPath}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading icon from {iconPath}: {ex.Message}");
                        }
                    }
                }

                if (!iconLoaded)
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                    System.Diagnostics.Debug.WriteLine("Default icon was used");
                }
            }
            catch (Exception ex)
            {
                _notifyIcon.Icon = SystemIcons.Application;
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }

            _notifyIcon.Visible = false;

            // Event handler for double-click to open
            _notifyIcon.DoubleClick += (s, e) => 
            {
                try
                {
                    Dispatcher.Invoke(() => ShowWindow());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error opening via double-click: {ex.Message}");
                }
            };

            // Handler na Closing event
            this.Closing += MainWindow_Closing;
            this.StateChanged += MainWindow_StateChanged;
        }
        catch (Exception ex)
        {
            // If system tray doesn't work, the application will function normally
            System.Diagnostics.Debug.WriteLine($"Error initializing system tray: {ex.Message}");
            _notifyIcon = null;
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Mute sound when minimizing
        _videoManager?.Mute();
        
        // If system tray is available, hide window instead of closing
        if (_notifyIcon != null)
        {
            e.Cancel = true;
            HideWindow();
        }
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        // If window is minimized, hide it and mute sound
        if (this.WindowState == WindowState.Minimized)
        {
            _videoManager?.Mute();
            HideWindow();
        }
    }

    private void HideWindow()
    {
        try
        {
            // Mute sound when minimizing
            _videoManager?.Mute();
            
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
            }
            this.WindowState = System.Windows.WindowState.Normal;
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error while hiding window: {ex.Message}");
        }
    }

    private void ShowWindow()
    {
        try
        {
            // Unmute sound when opening window
            _videoManager?.Unmute();
            
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
            
            // Make window visible and normal - in correct order
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
            
            // Center window on screen
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
            
            // Now make it visible
            this.Visibility = System.Windows.Visibility.Visible;
            this.Show();
            
            // Force window to foreground
            this.Activate();
            this.Focus();
            this.Topmost = true;
            this.Topmost = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error while showing window: {ex.Message}");
        }
    }

    private void ExitApplication()
    {
        try
        {
            // Clean up video manager
            _videoManager?.Dispose();
            _videoManager = null;

            // Opravdu zavřít aplikaci
            this.Closing -= MainWindow_Closing;
            this.StateChanged -= MainWindow_StateChanged;
            
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.DoubleClick -= (s, e) => { };
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            
            _contextMenu?.Dispose();
            _contextMenu = null;
            
            this.Close();
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error while closing application: {ex.Message}");
            System.Windows.Application.Current.Shutdown(1);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.UpdateLauncherState();
    }

    /// <summary>
    /// Toggle between news (Game News / Launcher Updates)
    /// </summary>
    private void NewsTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string newsType)
        {
            var newsOverlay = this.FindName("NewsOverlay") as Grid;
            var newsModalTitle = this.FindName("NewsModalTitle") as TextBlock;
            var newsModalContent = this.FindName("NewsModalContent") as TextBlock;
            var gameNewsBtn = this.FindName("GameNewsBtn") as System.Windows.Controls.Button;
            var launcherNewsBtn = this.FindName("LauncherNewsBtn") as System.Windows.Controls.Button;

            if (newsType == "GameNews")
            {
                // Display Game News
                if (newsModalTitle != null) newsModalTitle.Text = _viewModel.GameNewsTitle;
                if (newsModalContent != null) newsModalContent.Text = _viewModel.GameNewsContent;
                
                if (gameNewsBtn != null)
                {
                    gameNewsBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#252525"));
                    gameNewsBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0066cc"));
                    gameNewsBtn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0066cc"));
                }
                if (launcherNewsBtn != null)
                {
                    launcherNewsBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1a1a1a"));
                    launcherNewsBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999999"));
                    launcherNewsBtn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1a1a1a"));
                }
            }
            else if (newsType == "LauncherNews")
            {
                // Display Launcher News
                if (newsModalTitle != null) newsModalTitle.Text = _viewModel.LauncherNewsTitle;
                if (newsModalContent != null) newsModalContent.Text = _viewModel.LauncherNewsContent;
                
                if (gameNewsBtn != null)
                {
                    gameNewsBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1a1a1a"));
                    gameNewsBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999999"));
                    gameNewsBtn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1a1a1a"));
                }
                if (launcherNewsBtn != null)
                {
                    launcherNewsBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#252525"));
                    launcherNewsBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0066cc"));
                    launcherNewsBtn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0066cc"));
                }
            }

            // Zobrazit overlay
            if (newsOverlay != null)
            {
                newsOverlay.Visibility = Visibility.Visible;
            }
        }
    }

    /// <summary>
    /// Closing news overlay
    /// </summary>
    private void CloseNews_Click(object sender, RoutedEventArgs e)
    {
        var newsOverlay = this.FindName("NewsOverlay") as Grid;
        if (newsOverlay != null)
        {
            newsOverlay.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Volume slider change
    /// </summary>
    private void VolumeSlider_ValueChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_videoManager != null && sender is Slider slider)
            {
                double volume = slider.Value;
                _videoManager.SetVolume(volume);
                
                // Aktualizace procent zobrazení
                var volumePercent = this.FindName("VolumePercent") as TextBlock;
                if (volumePercent != null)
                {
                    volumePercent.Text = $"{volume * 100:F0}%";
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error while changing volume: {ex.Message}");
        }
    }

    /// <summary>
    /// Handler for dragging window by top bar
    /// </summary>
    private void TopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    /// <summary>
    /// Handler pro minimalizaci okna
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Launch game button click handler
    /// </summary>
    private void LaunchGameButton_Click(object sender, RoutedEventArgs e)
    {
        // Command is already bound in XAML, this handler is just for additional logic if needed
        Debug.WriteLine("[MainWindow] Launch Game Button clicked");
    }

    /// <summary>
    /// Settings button click handler
    /// </summary>
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        settingsWindow.ShowDialog();
    }

    /// <summary>
    /// Handler for closing application
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Mute sound when closing application
        _videoManager?.Mute();
        this.Close();
    }

        /// <summary>
        /// Opens music upload window with embedded web link
        /// </summary>
        private void UploadMusic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowTabContent(new UploadMusicView(), "UploadMusic");
                Debug.WriteLine("[MainWindow] Taab Upload Music shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing Upload Music: {ex.Message}");
                System.Windows.MessageBox.Show($"Error showing Upload Music: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens account registration window with embedded web link
        /// </summary>
        private void RegisterAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowTabContent(new RegisterAccountView(), "RegisterAccount");
                Debug.WriteLine("[MainWindow] Tab Register Account shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing Register Account: {ex.Message}");
                System.Windows.MessageBox.Show($"Error showing Register Account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Otevře okno pro Dashboard s embeddovaným webovým odkázem
        /// </summary>
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowTabContent(new DashboardView(), "Dashboard");
                Debug.WriteLine("[MainWindow] Tab Dashboard shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing Dashboard: {ex.Message}");
                System.Windows.MessageBox.Show($"Error showing Dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Otevře okno pro změnu hesla
        /// </summary>
        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowTabContent(new ForgotPasswordView(), "ForgotPassword");
                Debug.WriteLine("[MainWindow] Tab Forgot Password shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing Forgot Password: {ex.Message}");
                System.Windows.MessageBox.Show($"Error showing Forgot Password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Otevře Debug Console okno
        /// </summary>
        private void DebugConsole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowTabContent(new DebugConsoleView(), "DebugConsole");
                Debug.WriteLine("[MainWindow] Tab Debug Console shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing Debug Console: {ex.Message}");
                System.Windows.MessageBox.Show($"Error showing Debug Console: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens Discord Server tab
        /// </summary>
        private void DiscordServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowTabContent(new DiscordServerView(), "DiscordServer");
                Debug.WriteLine("[MainWindow] Tab Discord Server shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing Discord Server: {ex.Message}");
                System.Windows.MessageBox.Show($"Error showing Discord Server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Vyčistit prostředky
            _videoManager?.Dispose();
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
            base.OnClosed(e);
        }

        /// <summary>
        /// Zobrazí/skryje obsah taby v ContentControl a aktualizuje styly tlačítek (Toggle)
        /// </summary>
        private void ShowTabContent(UIElement view, string tabName)
        {
            try
            {
                var tabControl = this.FindName("TabContentControl") as ContentControl;
                if (tabControl == null)
                {
                    Debug.WriteLine("[MainWindow] TabContentControl not found");
                    return;
                }

                // If tab is already visible, hide it (toggle)
                if (_currentSelectedTab == tabName)
                {
                    tabControl.Content = null;
                    _currentView = null;
                    _currentSelectedTab = null;
                    
                    // Reset all button styles
                    ResetAllTabButtonStyles();

                    Debug.WriteLine($"[MainWindow] Tab {tabName} hidden");
                    return;
                }

                // Otherwise show new tab
                tabControl.Content = view;
                _currentView = view;
                _currentSelectedTab = tabName;

                // Update button styles
                UpdateTabButtonStyles(tabName);

                Debug.WriteLine($"[MainWindow] Tab {tabName} shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error showing tabs: {ex.Message}");
            }
        }

        /// <summary>
        /// Aktualizuje styly tlačítek na základě vybraného taby
        /// </summary>
        private void UpdateTabButtonStyles(string selectedTab)
        {
            try
            {
                var buttonPairs = new (string tag, System.Windows.Controls.Button? btn)[]
                {
                    ("GameNews", this.FindName("GameNewsBtn") as System.Windows.Controls.Button),
                    ("LauncherNews", this.FindName("LauncherNewsBtn") as System.Windows.Controls.Button),
                    ("UploadMusic", this.FindName("UploadMusicBtn") as System.Windows.Controls.Button),
                    ("RegisterAccount", this.FindName("RegisterAccountBtn") as System.Windows.Controls.Button),
                    ("Dashboard", this.FindName("DashboardBtn") as System.Windows.Controls.Button),
                    ("ForgotPassword", this.FindName("ForgotPasswordBtn") as System.Windows.Controls.Button),
                    ("DebugConsole", this.FindName("DebugConsoleBtn") as System.Windows.Controls.Button),
                    ("DiscordServer", this.FindName("DiscordServerBtn") as System.Windows.Controls.Button)
                };

                foreach (var (tag, btn) in buttonPairs)
                {
                    if (btn != null)
                    {
                        if (tag == selectedTab)
                        {
                            // Active state
                            btn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00d9ff99"));
                            btn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00d9ff"));
                            btn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00d9ff"));
                            btn.BorderThickness = new Thickness(0, 0, 0, 2);
                        }
                        else
                        {
                            // Inactive state
                            btn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00000000"));
                            btn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#b0b0c0"));
                            btn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00000000"));
                            btn.BorderThickness = new Thickness(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error updating styles: {ex.Message}");
            }
        }

        /// <summary>
        /// Resetne všechna tlačítka do výchozího nezvýrazněného stavu
        /// </summary>
        private void ResetAllTabButtonStyles()
        {
            try
            {
                var buttonPairs = new (string tag, System.Windows.Controls.Button? btn)[]
                {
                    ("GameNews", this.FindName("GameNewsBtn") as System.Windows.Controls.Button),
                    ("LauncherNews", this.FindName("LauncherNewsBtn") as System.Windows.Controls.Button),
                    ("UploadMusic", this.FindName("UploadMusicBtn") as System.Windows.Controls.Button),
                    ("RegisterAccount", this.FindName("RegisterAccountBtn") as System.Windows.Controls.Button),
                    ("Dashboard", this.FindName("DashboardBtn") as System.Windows.Controls.Button),
                    ("ForgotPassword", this.FindName("ForgotPasswordBtn") as System.Windows.Controls.Button),
                    ("DebugConsole", this.FindName("DebugConsoleBtn") as System.Windows.Controls.Button),
                    ("DiscordServer", this.FindName("DiscordServerBtn") as System.Windows.Controls.Button)
                };

                foreach (var (tag, btn) in buttonPairs)
                {
                    if (btn != null)
                    {
                        // Inactive state
                        btn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00000000"));
                        btn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#b0b0c0"));
                        btn.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00000000"));
                        btn.BorderThickness = new Thickness(0);
                    }
                }

                Debug.WriteLine("[MainWindow] Všechna tlačítka resetnuta");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Chyba při resetování stylů: {ex.Message}");
            }
        }
    }
}