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
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;
        private readonly ILogger<PhotosController> _logger;

        public PhotosController(IPhotoService photoService, ILogger<PhotosController> logger)
        {
            _photoService = photoService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string clothingType, [FromForm] string? tags = null, [FromForm] string? notes = null)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var request = new PhotoUploadRequest
            {
                ClothingType = clothingType,
                Tags = tags,
                Notes = notes
            };

            _logger.LogInformation("Photo upload attempt by user {UserId}", userId);
            var response = await _photoService.UploadPhotoAsync(userId, file, request);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPhotos()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            _logger.LogInformation("Fetching photos for user {UserId}", userId);
            var photos = await _photoService.GetUserPhotosAsync(userId);

            return Ok(new { success = true, photos });
        }

        [HttpGet("{photoId}")]
        public async Task<IActionResult> GetPhoto(int photoId)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var photo = await _photoService.GetPhotoAsync(photoId, userId);
            if (photo == null)
            {
                return NotFound(new { success = false, message = "Photo not found." });
            }

            return Ok(new { success = true, photo });
        }

        [HttpDelete("{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            _logger.LogInformation("Delete photo {PhotoId} request by user {UserId}", photoId, userId);
            var success = await _photoService.DeletePhotoAsync(photoId, userId);

            if (!success)
            {
                return NotFound(new { success = false, message = "Photo not found." });
            }

            return Ok(new { success = true, message = "Photo deleted successfully." });
        }
    }
}
