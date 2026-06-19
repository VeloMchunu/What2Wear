using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using What2Wear.Shared.Models;
using What2Wear.Web.Services;

namespace What2Wear.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OutfitsController : ControllerBase
    {
        private readonly IOutfitSuggestionAiService _aiService;
        private readonly IPhotoService _photoService;
        private readonly ILogger<OutfitsController> _logger;

        public OutfitsController(IOutfitSuggestionAiService aiService, IPhotoService photoService, ILogger<OutfitsController> logger)
        {
            _aiService = aiService;
            _photoService = photoService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpPost("suggest")]
        public async Task<IActionResult> GetOutfitSuggestion([FromBody] OutfitSuggestionRequest request)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            try
            {
                _logger.LogInformation("Outfit suggestion request for user {UserId} with theme {Theme}", userId, request.ThemeId);

                // Fetch the selected photos
                var photos = new List<PhotoDto>();
                foreach (var photoId in request.AvailablePhotoIds)
                {
                    var photo = await _photoService.GetPhotoAsync(photoId, userId);
                    if (photo != null)
                    {
                        photos.Add(photo);
                    }
                }

                if (photos.Count == 0)
                {
                    return BadRequest(new OutfitSuggestionResponse
                    {
                        Success = false,
                        Message = "No valid photos found"
                    });
                }

                // Generate outfit suggestion using AI
                var suggestion = await _aiService.GenerateOutfitSuggestionAsync(request.ThemeId, photos);

                return Ok(suggestion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outfit suggestion");
                return StatusCode(500, new OutfitSuggestionResponse
                {
                    Success = false,
                    Message = "Error generating outfit suggestion: " + ex.Message
                });
            }
        }
    }
}
