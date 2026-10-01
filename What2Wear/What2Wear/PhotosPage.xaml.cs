using What2Wear.Services;
using System.Collections.ObjectModel;

namespace What2Wear
{
    public partial class PhotosPage : ContentPage
    {
        private readonly IApiClient _apiClient;
        private ObservableCollection<PhotoItem> _photos = new();

        public PhotosPage()
        {
            InitializeComponent();
            _apiClient = IPlatformApplication.Current?.Services.GetService<IApiClient>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPhotos();
        }

        private async Task LoadPhotos()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            try
            {
                var (success, photos) = await _apiClient.GetPhotosAsync();
                if (success)
                {
                    _photos.Clear();
                    foreach (var photo in photos)
                    {
                        _photos.Add(new PhotoItem 
                        { 
                            Id = photo.Id,
                            Url = photo.Url, 
                            ClothingType = photo.ClothingType,
                            Tags = photo.Tags ?? "No tags",
                            CreatedAt = photo.CreatedAt
                        });
                    }

                    PhotosCollectionView.ItemsSource = _photos;
                    EmptyStateContainer.IsVisible = _photos.Count == 0;
                    PhotoCountLabel.Text = $"{_photos.Count} {(_photos.Count == 1 ? "Item" : "Items")}";
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load photos", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnCameraClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///capture");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (confirm)
            {
                _apiClient.SetToken(null);
                await Shell.Current.GoToAsync("///login");
            }
        }
    }

    public class PhotoItem
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public string ClothingType { get; set; } = null!;
        public string Tags { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
