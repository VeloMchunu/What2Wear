using What2Wear.Services;
using What2Wear.Shared.Models;

namespace What2Wear
{
    public partial class LoginPage : ContentPage
    {
        private readonly IApiClient _apiClient;

        public LoginPage()
        {
            InitializeComponent();
            _apiClient = IPlatformApplication.Current?.Services.GetService<IApiClient>();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ErrorLabel.Text = "Please enter email and password";
                ErrorLabel.IsVisible = true;
                return;
            }

            LoginButton.IsEnabled = false;

            try
            {
                var request = new LoginRequest
                {
                    Email = EmailEntry.Text,
                    Password = PasswordEntry.Text
                };

                var response = await _apiClient.LoginAsync(request);

                if (response.Success && response.Token != null)
                {
                    _apiClient.SetToken(response.Token);
                    ErrorLabel.IsVisible = false;
                    await Shell.Current.GoToAsync("///photos");
                }
                else
                {
                    ErrorLabel.Text = response.Message;
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Error: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///signup");
        }
    }
}
