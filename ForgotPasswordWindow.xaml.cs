using System.Windows;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;

namespace SkyboundLauncher
{
    /// <summary>
    /// Interaction logic for ForgotPasswordWindow.xaml
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        private const string DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1451316947512983655/flwlbmC8QunpSC0wVxziEjrJYxhU8QG17HOxmVxq0rEI7i8E1TfkhndLlW-NwkctrA85";
        private static readonly HttpClient _httpClient = new HttpClient();

        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[ForgotPasswordWindow] Window loaded");
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get form values
                string username = UsernameTextBox.Text?.Trim() ?? "";
                string email = EmailTextBox.Text?.Trim() ?? "";
                string discordUsername = DiscordUsernameTextBox.Text?.Trim() ?? "";

                // Validate fields
                if (string.IsNullOrEmpty(username))
                {
                    StatusMessage.Text = "Please enter username!";
                    StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff6b6b"));
                    return;
                }

                if (string.IsNullOrEmpty(email))
                {
                    StatusMessage.Text = "Please enter email!";
                    StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff6b6b"));
                    return;
                }

                if (string.IsNullOrEmpty(discordUsername))
                {
                    StatusMessage.Text = "Please enter Discord username!";
                    StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff6b6b"));
                    return;
                }

                // Disable button during submission
                SubmitButton.IsEnabled = false;
                StatusMessage.Text = "Sending...";
                StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00d9ff"));

                // Send Discord webhook message
                await SendDiscordMessage(username, email, discordUsername);

                // Clear fields
                UsernameTextBox.Clear();
                EmailTextBox.Clear();
                DiscordUsernameTextBox.Clear();

                // Show success message
                StatusMessage.Text = "Password change request sent! Check your Discord.";
                StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#51cf66"));

                Debug.WriteLine("[ForgotPasswordWindow] Message successfully sent");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPasswordWindow] Error: {ex.Message}");
                StatusMessage.Text = $"Error: {ex.Message}";
                StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ff6b6b"));
            }
            finally
            {
                SubmitButton.IsEnabled = true;
            }
        }

        private async Task SendDiscordMessage(string username, string email, string discordUsername)
        {
            try
            {
                // Create Discord embed message
                var embedData = new
                {
                    embeds = new object[]
                    {
                        new
                        {
                            title = "🔑 Password Change Request",
                            description = "New password change request for account",
                            color = 51200, // Cyan color
                            fields = new object[]
                            {
                                new
                                {
                                    name = "Username",
                                    value = username,
                                    inline = true
                                },
                                new
                                {
                                    name = "Email",
                                    value = email,
                                    inline = true
                                },
                                new
                                {
                                    name = "Discord Username",
                                    value = discordUsername,
                                    inline = false
                                }
                            },
                            timestamp = DateTime.UtcNow.ToString("O")
                        }
                    }
                };

                // Convert to JSON
                string jsonContent = JsonSerializer.Serialize(embedData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Send to Discord
                var response = await _httpClient.PostAsync(DISCORD_WEBHOOK_URL, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Discord API error: {response.StatusCode}");
                }

                Debug.WriteLine("[ForgotPasswordWindow] Message sent to Discord");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForgotPasswordWindow] Error sending to Discord: {ex.Message}");
                throw;
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
    }
}
