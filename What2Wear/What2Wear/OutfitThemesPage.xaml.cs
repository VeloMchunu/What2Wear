using What2Wear.Shared.Models;
using What2Wear.Services;
using System.Collections.ObjectModel;

namespace What2Wear
{
    public partial class OutfitThemesPage : ContentPage
    {
        private readonly IOutfitSuggestionService _outfitSuggestionService;
        private readonly IApiClient _apiClient;
        private ObservableCollection<OutfitTheme> _themes = new();
        private List<PhotoDto> _userPhotos = new();
        private OutfitTheme? _selectedTheme;
        private bool _isLoading = false;

        public OutfitThemesPage()
        {
            InitializeComponent();
            _outfitSuggestionService = IPlatformApplication.Current?.Services.GetService<IOutfitSuggestionService>() 
                ?? throw new InvalidOperationException("OutfitSuggestionService not found");
            _apiClient = IPlatformApplication.Current?.Services.GetService<IApiClient>() 
                ?? throw new InvalidOperationException("ApiClient not found");

            // Wire up event handlers
            ThemesCollectionView.SelectionChanged += OnThemeSelected;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadThemesAndPhotos();
        }

        private async Task LoadThemesAndPhotos()
        {
            try
            {
                // Load themes
                var themes = await _outfitSuggestionService.GetAvailableThemesAsync();
                _themes.Clear();
                foreach (var theme in themes)
                {
                    _themes.Add(theme);
                }
                ThemesCollectionView.ItemsSource = _themes;

                // Load user photos
                var result = await _apiClient.GetPhotosAsync();
                if (result.Success)
                {
                    _userPhotos = result.Photos.ToList();
                    UpdateEmptyState();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }

        private void UpdateEmptyState()
        {
            if (_userPhotos.Count == 0)
            {
                EmptyState.IsVisible = true;
                ThemesCollectionView.IsVisible = false;
            }
            else
            {
                EmptyState.IsVisible = false;
                ThemesCollectionView.IsVisible = true;
            }
        }

        private async void OnThemeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is OutfitTheme selectedTheme)
            {
                _selectedTheme = selectedTheme;
                await GenerateOutfitSuggestion();
            }
        }

        private async Task GenerateOutfitSuggestion()
        {
            if (_selectedTheme == null || _userPhotos.Count == 0)
            {
                return;
            }

            try
            {
                _isLoading = true;
                LoadingStack.IsVisible = true;
                OutfitDisplayStack.IsVisible = false;

                // Get suggestion from API
                var photoIds = _userPhotos.Select(p => p.Id).ToList();
                var suggestion = await _outfitSuggestionService.GetOutfitSuggestionAsync(_selectedTheme.Id, photoIds);

                if (suggestion.Success)
                {
                    // Display the generated outfit image
                    if (!string.IsNullOrEmpty(suggestion.GeneratedImageUrl))
                    {
                        OutfitImage.Source = suggestion.GeneratedImageUrl;
                    }

                    // Display outfit items
                    OutfitItemsCollectionView.ItemsSource = new ObservableCollection<OutfitItemSuggestion>(suggestion.Items);

                    // Display styling tips
                    StylingTipsLabel.Text = suggestion.StylingTips;

                    LoadingStack.IsVisible = false;
                    OutfitDisplayStack.IsVisible = true;
                }
                else
                {
                    LoadingStack.IsVisible = false;
                    await DisplayAlert("Error", suggestion.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                LoadingStack.IsVisible = false;
                await DisplayAlert("Error", $"Failed to generate suggestion: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
