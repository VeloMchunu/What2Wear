using What2Wear.Shared.Models;

namespace What2Wear.Services
{
    public interface IOutfitSuggestionService
    {
        Task<OutfitSuggestionResponse> GetOutfitSuggestionAsync(string themeId, List<int> photoIds);
        Task<List<OutfitTheme>> GetAvailableThemesAsync();
    }

    public class OutfitSuggestionService : IOutfitSuggestionService
    {
        private readonly IApiClient _apiClient;

        public OutfitSuggestionService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<OutfitSuggestionResponse> GetOutfitSuggestionAsync(string themeId, List<int> photoIds)
        {
            try
            {
                var request = new OutfitSuggestionRequest
                {
                    ThemeId = themeId,
                    AvailablePhotoIds = photoIds
                };

                var response = await _apiClient.GetOutfitSuggestionAsync(request);
                return response ?? new OutfitSuggestionResponse
                {
                    Success = false,
                    Message = "Failed to get outfit suggestion"
                };
            }
            catch (Exception ex)
            {
                return new OutfitSuggestionResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<List<OutfitTheme>> GetAvailableThemesAsync()
        {
            return new List<OutfitTheme>
            {
                new OutfitTheme
                {
                    Id = "office",
                    Name = "Office Wear",
                    Description = "Professional business attire for the workplace",
                    Icon = "💼"
                },
                new OutfitTheme
                {
                    Id = "casual",
                    Name = "Casual",
                    Description = "Comfortable everyday outfits",
                    Icon = "👕"
                },
                new OutfitTheme
                {
                    Id = "formal",
                    Name = "Formal",
                    Description = "Elegant outfits for special occasions",
                    Icon = "🎩"
                },
                new OutfitTheme
                {
                    Id = "sports",
                    Name = "Sports & Active",
                    Description = "Athletic and sporty wear",
                    Icon = "⚽"
                },
                new OutfitTheme
                {
                    Id = "date",
                    Name = "Date Night",
                    Description = "Stylish outfits for romantic occasions",
                    Icon = "💃"
                },
                new OutfitTheme
                {
                    Id = "weekend",
                    Name = "Weekend Chic",
                    Description = "Relaxed but put-together looks",
                    Icon = "☀️"
                },
                new OutfitTheme
                {
                    Id = "party",
                    Name = "Party",
                    Description = "Fun and trendy party outfits",
                    Icon = "🎉"
                },
                new OutfitTheme
                {
                    Id = "minimalist",
                    Name = "Minimalist",
                    Description = "Simple, clean, and timeless looks",
                    Icon = "✨"
                }
            };
        }
    }
}
