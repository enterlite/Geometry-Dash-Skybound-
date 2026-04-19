using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for ForgotPasswordView.xaml
    /// </summary>
    public partial class ForgotPasswordView : UserControl
    {
        private const string DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1451316947512983655/flwlbmC8QunpSC0wVxziEjrJYxhU8QG17HOxmVxq0rEI7i8E1TfkhndLlW-NwkctrA85";

        public ForgotPasswordView()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var usernameTextBox = this.FindName("UsernameTextBox") as TextBox;
                var emailTextBox = this.FindName("EmailTextBox") as TextBox;
                var discordUsernameTextBox = this.FindName("DiscordUsernameTextBox") as TextBox;
                var statusMessage = this.FindName("StatusMessage") as TextBlock;

                if (usernameTextBox == null || emailTextBox == null || discordUsernameTextBox == null || statusMessage == null)
                {
                    Debug.WriteLine("[ForgotPasswordView] Elements not found");
                    return;
                }

                // Validation
                if (string.IsNullOrWhiteSpace(usernameTextBox.Text))
                {
                    statusMessage.Text = "⚠️ Please enter Username!";
                    statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
                    return;
                }

                if (string.IsNullOrWhiteSpace(emailTextBox.Text))
                {
                    statusMessage.Text = "⚠️ Please enter Email!";
                    statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
                    return;
                }

                if (string.IsNullOrWhiteSpace(discordUsernameTextBox.Text))
                {
                    statusMessage.Text = "⚠️ Please enter Discord Username!";
                    statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
                    return;
                }

                // Processing
                statusMessage.Text = "⏳ Sending...";
                statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00d9ff"));

                SendDiscordMessage(usernameTextBox.Text, emailTextBox.Text, discordUsernameTextBox.Text, statusMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPasswordView] Error: {ex.Message}");
                var statusMessage = this.FindName("StatusMessage") as TextBlock;
                if (statusMessage != null)
                {
                    statusMessage.Text = $"❌ Error: {ex.Message}";
                    statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
                }
            }
        }

        private async void SendDiscordMessage(string username, string email, string discordUsername, TextBlock statusMessage)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var payload = new
                    {
                        embeds = new[]
                        {
                            new
                            {
                                title = "🔑 Password Change Request",
                                color = 51200,
                                fields = new[]
                                {
                                    new { name = "Username", value = username, inline = false },
                                    new { name = "Email", value = email, inline = false },
                                    new { name = "Discord Username", value = discordUsername, inline = false }
                                }
                            }
                        }
                    };

                    var jsonContent = JsonSerializer.Serialize(payload);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(DISCORD_WEBHOOK_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        statusMessage.Text = "✅ Request sent! You will receive a reply on Discord soon.";
                        statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff66"));

                        // Clear fields
                        var usernameTextBox = this.FindName("UsernameTextBox") as TextBox;
                        var emailTextBox = this.FindName("EmailTextBox") as TextBox;
                        var discordUsernameTextBox = this.FindName("DiscordUsernameTextBox") as TextBox;

                        if (usernameTextBox != null) usernameTextBox.Clear();
                        if (emailTextBox != null) emailTextBox.Clear();
                        if (discordUsernameTextBox != null) discordUsernameTextBox.Clear();
                    }
                    else
                    {
                        statusMessage.Text = $"❌ Error: {response.StatusCode}";
                        statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPasswordView] Error sending: {ex.Message}");
                statusMessage.Text = $"❌ Error: {ex.Message}";
                statusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6666"));
            }
        }
    }
}
