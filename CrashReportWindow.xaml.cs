using System.Windows;
using SkyboundLauncher.Services;

namespace SkyboundLauncher;

public partial class CrashReportWindow : Window
{
    private readonly CrashReportService _crashReportService;

    public CrashReportWindow()
    {
        InitializeComponent();
        _crashReportService = new CrashReportService();
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        string nick = NickTextBox.Text?.Trim() ?? "";
        string errorMessage = ErrorMessageTextBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(nick))
        {
            MessageBox.Show("Please enter your nickname", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            MessageBox.Show("Please enter error description", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SendButton.IsEnabled = false;
        SendButton.Content = "Sending...";

        bool success = await _crashReportService.SendCrashReportAsync(nick, errorMessage);

        if (success)
        {
            MessageBox.Show("Thank you for reporting! Your message has been sent successfully.", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        else
        {
            MessageBox.Show("Error sending message. Please try again later.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            SendButton.IsEnabled = true;
            SendButton.Content = "Send";
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
