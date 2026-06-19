using What2Wear.Services;
using What2Wear.Shared.Models;

namespace What2Wear
{
    public partial class SignupPage : ContentPage
    {
        private readonly IApiClient _apiClient;

        public SignupPage()
        {
            InitializeComponent();
            _apiClient = IPlatformApplication.Current?.Services.GetService<IApiClient>();
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text) || 
                string.IsNullOrWhiteSpace(EmailEntry.Text) || 
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ErrorLabel.Text = "Please fill all fields";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                ErrorLabel.Text = "Passwords do not match";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                ErrorLabel.Text = "Password must be at least 6 characters";
                ErrorLabel.IsVisible = true;
                return;
            }

            SignupButton.IsEnabled = false;

            try
            {
                var request = new SignupRequest
                {
                    FullName = FullNameEntry.Text,
                    Email = EmailEntry.Text,
                    Password = PasswordEntry.Text
                };

                var response = await _apiClient.SignupAsync(request);

                if (response.Success && response.Token != null)
                {
                    _apiClient.SetToken(response.Token);
                    ErrorLabel.IsVisible = false;
                    await Shell.Current.GoToAsync("photos");
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
                SignupButton.IsEnabled = true;
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("login");
        }
    }
}
