using System.Text;
using System.Text.Json;
using What2Wear.Shared.Models;

namespace What2Wear.Web.Services
{
    public interface IOutfitSuggestionAiService
    {
        Task<OutfitSuggestionResponse> GenerateOutfitSuggestionAsync(string themeId, List<PhotoDto> availablePhotos);
    }

    public class OutfitSuggestionAiService : IOutfitSuggestionAiService
    {
        private readonly ILogger<OutfitSuggestionAiService> _logger;
        private readonly IConfiguration _configuration;

        public OutfitSuggestionAiService(ILogger<OutfitSuggestionAiService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<OutfitSuggestionResponse> GenerateOutfitSuggestionAsync(string themeId, List<PhotoDto> availablePhotos)
        {
            try
            {
                // Get theme details
                var themeDetails = GetThemeDetails(themeId);

                // For now, we'll implement a smart matching algorithm that selects items based on theme rules
                // In production, you would integrate with an actual AI service like OpenAI's DALL-E, Stable Diffusion, etc.

                var suggestedItems = SelectOutfitItems(themeDetails, availablePhotos);

                if (suggestedItems.Count == 0)
                {
                    return new OutfitSuggestionResponse
                    {
                        Success = false,
                        Message = "Not enough photos to create an outfit suggestion for this theme"
                    };
                }

                // Generate styling tips based on theme
                var stylingTips = GenerateStylingTips(themeId);

                // Create a visual representation (in production, use DALL-E or similar)
                var generatedImageUrl = await GenerateOutfitVisualization(themeDetails, suggestedItems);

                return new OutfitSuggestionResponse
                {
                    Success = true,
                    Message = "Outfit suggestion generated successfully",
                    Theme = themeDetails["name"],
                    Items = suggestedItems,
                    StylingTips = stylingTips,
                    GeneratedImageUrl = generatedImageUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outfit suggestion");
                return new OutfitSuggestionResponse
                {
                    Success = false,
                    Message = "Error: " + ex.Message
                };
            }
        }

        private Dictionary<string, string> GetThemeDetails(string themeId)
        {
            return themeId switch
            {
                "office" => new Dictionary<string, string>
                {
                    { "name", "Office Wear" },
                    { "focus", "Professional, structured, neutral colors" },
                    { "priority", "blazer,shirt,pants,skirt,shoes" }
                },
                "casual" => new Dictionary<string, string>
                {
                    { "name", "Casual" },
                    { "focus", "Comfortable, relaxed, everyday pieces" },
                    { "priority", "jeans,t-shirt,sneakers,jacket,sweater" }
                },
                "formal" => new Dictionary<string, string>
                {
                    { "name", "Formal" },
                    { "focus", "Elegant, sophisticated, luxury pieces" },
                    { "priority", "dress,suit,heels,tie,jacket" }
                },
                "sports" => new Dictionary<string, string>
                {
                    { "name", "Sports & Active" },
                    { "focus", "Athletic, functional, performance gear" },
                    { "priority", "leggings,sports-top,sneakers,jacket,shorts" }
                },
                "date" => new Dictionary<string, string>
                {
                    { "name", "Date Night" },
                    { "focus", "Stylish, flattering, romantic vibes" },
                    { "priority", "dress,heels,top,pants,accessories" }
                },
                "weekend" => new Dictionary<string, string>
                {
                    { "name", "Weekend Chic" },
                    { "focus", "Put-together but relaxed" },
                    { "priority", "jeans,blouse,cardigan,loafers,sneakers" }
                },
                "party" => new Dictionary<string, string>
                {
                    { "name", "Party" },
                    { "focus", "Fun, trendy, standout pieces" },
                    { "priority", "dress,top,heels,jacket,accessories" }
                },
                "minimalist" => new Dictionary<string, string>
                {
                    { "name", "Minimalist" },
                    { "focus", "Simple, clean lines, neutral palette" },
                    { "priority", "basics,neutral-colors,shoes,timeless-pieces" }
                },
                _ => new Dictionary<string, string>
                {
                    { "name", "Outfit" },
                    { "focus", "Stylish combination" },
                    { "priority", "shirt,pants,shoes" }
                }
            };
        }

        private List<OutfitItemSuggestion> SelectOutfitItems(Dictionary<string, string> themeDetails, List<PhotoDto> availablePhotos)
        {
            var suggestions = new List<OutfitItemSuggestion>();
            var priorityItems = themeDetails["priority"].Split(',').ToList();
            var selectedPhotoIds = new HashSet<int>();

            // Select items based on clothing type priority for the theme
            foreach (var priority in priorityItems)
            {
                var matchingPhoto = availablePhotos.FirstOrDefault(p =>
                    !selectedPhotoIds.Contains(p.Id) &&
                    p.ClothingType.Contains(priority, StringComparison.OrdinalIgnoreCase));

                if (matchingPhoto != null)
                {
                    suggestions.Add(new OutfitItemSuggestion
                    {
                        ClothingType = matchingPhoto.ClothingType,
                        PhotoId = matchingPhoto.Id,
                        PhotoUrl = matchingPhoto.Url,
                        Reason = GetItemReason(matchingPhoto.ClothingType, themeDetails["name"])
                    });
                    selectedPhotoIds.Add(matchingPhoto.Id);
                }

                if (suggestions.Count >= 4) // Limit to 4 items per outfit
                {
                    break;
                }
            }

            // Fill remaining slots with any available photos if not enough priority matches
            if (suggestions.Count < 3)
            {
                var remaining = availablePhotos.Where(p => !selectedPhotoIds.Contains(p.Id)).ToList();
                foreach (var photo in remaining.Take(4 - suggestions.Count))
                {
                    suggestions.Add(new OutfitItemSuggestion
                    {
                        ClothingType = photo.ClothingType,
                        PhotoId = photo.Id,
                        PhotoUrl = photo.Url,
                        Reason = $"Completes your {themeDetails["name"]} look perfectly"
                    });
                }
            }

            return suggestions;
        }

        private string GetItemReason(string clothingType, string theme)
        {
            return clothingType.ToLower() switch
            {
                _ when clothingType.Contains("blazer", StringComparison.OrdinalIgnoreCase) 
                    => "Provides professional structure and polish",
                _ when clothingType.Contains("shirt", StringComparison.OrdinalIgnoreCase) 
                    => "Versatile base layer for any outfit",
                _ when clothingType.Contains("pants", StringComparison.OrdinalIgnoreCase) 
                    => "Flattering and classic foundation",
                _ when clothingType.Contains("jeans", StringComparison.OrdinalIgnoreCase) 
                    => "Timeless casual essential",
                _ when clothingType.Contains("dress", StringComparison.OrdinalIgnoreCase) 
                    => "Elegant one-piece solution",
                _ when clothingType.Contains("shoes", StringComparison.OrdinalIgnoreCase) 
                    => "Completes your look with style",
                _ when clothingType.Contains("jacket", StringComparison.OrdinalIgnoreCase) 
                    => "Adds depth and dimension",
                _ when clothingType.Contains("sweater", StringComparison.OrdinalIgnoreCase) 
                    => "Cozy comfort with sophistication",
                _ => $"Perfect choice for {theme}"
            };
        }

        private string GenerateStylingTips(string themeId)
        {
            return themeId switch
            {
                "office" => "Keep your palette neutral with pops of color through accessories. Ensure all pieces are well-fitted and structured. Opt for quality fabrics that convey professionalism.",
                "casual" => "Mix and match basics with different washes and textures. Add comfort without sacrificing style. Sneakers or flats work great for all-day wear.",
                "formal" => "Choose a cohesive color scheme (jewel tones or classic blacks/whites work best). Ensure proper tailoring for an impeccable fit. Add elegant jewelry to elevate the look.",
                "sports" => "Focus on performance fabrics that move with you. Choose bright colors or fun patterns to stay motivated. Proper footwear is essential for comfort and performance.",
                "date" => "Choose colors that complement your skin tone. Aim for a balance of comfort and elegance. Confidence is your best accessory!",
                "weekend" => "Mix casual and dressed-up pieces. Comfort and style go hand-in-hand. Accessories can elevate simple basics.",
                "party" => "Don't be afraid to take risks with bold colors or patterns. Add statement jewelry or accessories. Make sure you feel confident and comfortable.",
                "minimalist" => "Quality over quantity - choose timeless pieces. Stick to a neutral color palette with occasional accent colors. Less is more with clean lines and simple silhouettes.",
                _ => "Mix and match to find your personal style. Ensure proper fit and quality fabrics. Confidence makes any outfit look better."
            };
        }

        private async Task<string> GenerateOutfitVisualization(Dictionary<string, string> themeDetails, List<OutfitItemSuggestion> items)
        {
            try
            {
                // For now, return a placeholder or use a simple image generation service
                // In production, integrate with DALL-E, Stable Diffusion, or similar

                // Placeholder: return a solid color based on theme
                var themeColor = themeDetails.Keys.FirstOrDefault() switch
                {
                    "office" => "#2C3E50", // Dark blue-gray
                    "casual" => "#3498DB", // Light blue
                    "formal" => "#1A1A1A", // Black
                    "sports" => "#E74C3C", // Red
                    "date" => "#E91E63", // Pink
                    "weekend" => "#F39C12", // Orange
                    "party" => "#9B59B6", // Purple
                    "minimalist" => "#ECF0F1", // Light gray
                    _ => "#95A5A6" // Gray
                };

                // In production, you would call an AI image generation API here
                // For demo purposes, we'll return a simple styled placeholder
                // Real implementation would look like:
                // var response = await callDalleOrStableDiffusion(theme, items);

                return GeneratePlaceholderImage(themeColor, themeDetails["name"], items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outfit visualization");
                return string.Empty;
            }
        }

        private string GeneratePlaceholderImage(string color, string theme, List<OutfitItemSuggestion> items)
        {
            // Return a data URI with a simple SVG representation
            var itemsText = string.Join("\n", items.Select((item, index) =>
                $"<text x='10' y='{50 + (index * 30)}' font-size='16' fill='white'>{item.ClothingType}</text>"));

            var svg = $@"
            <svg width='400' height='300' xmlns='http://www.w3.org/2000/svg'>
                <rect width='400' height='300' fill='{color}'/>
                <text x='200' y='40' font-size='24' font-weight='bold' fill='white' text-anchor='middle'>{theme}</text>
                <text x='200' y='80' font-size='14' fill='white' text-anchor='middle' opacity='0.8'>Suggested Outfit</text>
                {itemsText}
            </svg>";

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
            return $"data:image/svg+xml;base64,{base64}";
        }
    }
}
