using System.Windows;
using SkyboundLauncher.Services;
using System.Windows.Forms;

namespace SkyboundLauncher
{
    /// <summary>
    /// Okno s nastavením launcheru
    /// </summary>
    public partial class SettingsWindow : Window
{
    private SettingsManager _settingsManager;

    public SettingsWindow()
    {
        InitializeComponent();
        _settingsManager = new SettingsManager();
        LoadSettings();
    }

    /// <summary>
    /// Načte nastavení z souboru do UI
    /// </summary>
    private void LoadSettings()
    {
        var settings = _settingsManager.GetSettings();

        AutoUpdateCheckBox.IsChecked = settings.AutomaticUpdates;
        MinRamSlider.Value = settings.MinRamMB;
        MaxRamSlider.Value = settings.MaxRamMB;
        MinCpuSlider.Value = settings.MinCpuPercent;
        MaxCpuSlider.Value = settings.MaxCpuPercent;
        GamePathTextBox.Text = settings.GamePath;

        UpdateTexts();
    }

    /// <summary>
    /// Aktualizuje zobrazení hodnot
    /// </summary>
    private void UpdateTexts()
    {
        MinRamText.Text = $"{(int)MinRamSlider.Value} MB";
        MaxRamText.Text = $"{(int)MaxRamSlider.Value} MB";
        MinCpuText.Text = $"{(int)MinCpuSlider.Value} %";
        MaxCpuText.Text = $"{(int)MaxCpuSlider.Value} %";
    }

    private void MinRamSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateTexts();
    }

    private void MaxRamSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateTexts();
    }

    private void MinCpuSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateTexts();
    }

    private void MaxCpuSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateTexts();
    }

    private void AutoUpdateCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Bez akce na UI, jen sledování pro uložení
    }

    /// <summary>
    /// Procházení složek pro výběr cesty
    /// </summary>
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            dialog.Description = "Vyberte složku pro instalaci hry";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                GamePathTextBox.Text = dialog.SelectedPath;
            }
        }
    }

    /// <summary>
    /// Uloží nastavení do souboru
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsManager.AutomaticUpdates = AutoUpdateCheckBox.IsChecked ?? true;
        _settingsManager.MinRamMB = (int)MinRamSlider.Value;
        _settingsManager.MaxRamMB = (int)MaxRamSlider.Value;
        _settingsManager.MinCpuPercent = (int)MinCpuSlider.Value;
        _settingsManager.MaxCpuPercent = (int)MaxCpuSlider.Value;
        _settingsManager.GamePath = GamePathTextBox.Text;

        System.Diagnostics.Debug.WriteLine("[SettingsWindow] Nastavení uloženo");
        this.Close();
    }

    /// <summary>
    /// Zruší změny bez uložení
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// Close tlačítko
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
}
