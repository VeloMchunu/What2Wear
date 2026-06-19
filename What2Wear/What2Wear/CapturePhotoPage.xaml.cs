using What2Wear.Services;
using What2Wear.Shared.Models;

namespace What2Wear
{
    public partial class CapturePhotoPage : ContentPage
    {
        private readonly IApiClient _apiClient;
        private FileResult? _currentPhoto;

        public CapturePhotoPage()
        {
            InitializeComponent();
            _apiClient = IPlatformApplication.Current?.Services.GetService<IApiClient>();
        }

        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    _currentPhoto = photo;
                    var stream = await photo.OpenReadAsync();
                    PhotoImage.Source = ImageSource.FromStream(() => stream);
                    NoPhotoLabel.IsVisible = false;
                    NoPhotoText.IsVisible = false;
                    UploadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to capture photo: {ex.Message}", "OK");
            }
        }

        private async void OnSelectFromGalleryClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a photo",
                    FileTypes = FilePickerFileType.Images
                });

                if (photo != null)
                {
                    _currentPhoto = photo;
                    var stream = await photo.OpenReadAsync();
                    PhotoImage.Source = ImageSource.FromStream(() => stream);
                    NoPhotoLabel.IsVisible = false;
                    NoPhotoText.IsVisible = false;
                    UploadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select photo: {ex.Message}", "OK");
            }
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            if (_currentPhoto == null)
            {
                await DisplayAlert("Error", "Please select a photo first", "OK");
                return;
            }

            if (ClothingTypePicker.SelectedIndex < 0)
            {
                await DisplayAlert("Error", "Please select a clothing type", "OK");
                return;
            }

            UploadButton.IsEnabled = false;
            UploadIndicator.IsRunning = true;
            UploadIndicator.IsVisible = true;

            try
            {
                using var stream = await _currentPhoto.OpenReadAsync();
                var request = new PhotoUploadRequest
                {
                    ClothingType = ClothingTypePicker.Items[ClothingTypePicker.SelectedIndex],
                    Tags = TagsEntry.Text,
                    Notes = NotesEntry.Text
                };

                var response = await _apiClient.UploadPhotoAsync(stream, _currentPhoto.FileName, request);

                if (response.Success)
                {
                    StatusLabel.Text = "Photo uploaded successfully!";
                    StatusLabel.TextColor = Color.FromArgb("#34C759");
                    StatusLabel.IsVisible = true;

                    // Reset form after 2 seconds
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("photos");
                }
                else
                {
                    await DisplayAlert("Error", response.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Upload failed: {ex.Message}", "OK");
            }
            finally
            {
                UploadButton.IsEnabled = true;
                UploadIndicator.IsRunning = false;
                UploadIndicator.IsVisible = false;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
