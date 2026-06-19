namespace What2Wear.Shared.Models
{
    public class OutfitTheme
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Icon { get; set; } = null!;
    }

    public class OutfitSuggestionRequest
    {
        public string ThemeId { get; set; } = null!;
        public List<int> AvailablePhotoIds { get; set; } = new();
    }

    public class OutfitItemSuggestion
    {
        public string ClothingType { get; set; } = null!;
        public int PhotoId { get; set; }
        public string PhotoUrl { get; set; } = null!;
        public string Reason { get; set; } = null!;
    }

    public class OutfitSuggestionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string Theme { get; set; } = null!;
        public List<OutfitItemSuggestion> Items { get; set; } = new();
        public string StylingTips { get; set; } = null!;
        public string GeneratedImageUrl { get; set; } = null!;
    }

    public class OutfitGenerationRequest
    {
        public string Theme { get; set; } = null!;
        public List<string> PhotoUrls { get; set; } = new();
        public List<string> ClothingDescriptions { get; set; } = new();
    }
}
