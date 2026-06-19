using Microsoft.EntityFrameworkCore;
using What2Wear.Shared.Models;
using What2Wear.Web.Data;
using What2Wear.Web.Models;

namespace What2Wear.Web.Services
{
    public interface IPhotoService
    {
        Task<PhotoUploadResponse> UploadPhotoAsync(int userId, IFormFile file, PhotoUploadRequest request);
        Task<IEnumerable<PhotoDto>> GetUserPhotosAsync(int userId);
        Task<PhotoDto?> GetPhotoAsync(int photoId, int userId);
        Task<bool> DeletePhotoAsync(int photoId, int userId);
    }

    public class PhotoService : IPhotoService
    {
        private readonly What2WearDbContext _dbContext;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<PhotoService> _logger;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public PhotoService(What2WearDbContext dbContext, IWebHostEnvironment hostEnvironment, ILogger<PhotoService> logger)
        {
            _dbContext = dbContext;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public async Task<PhotoUploadResponse> UploadPhotoAsync(int userId, IFormFile file, PhotoUploadRequest request)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return new PhotoUploadResponse { Success = false, Message = "No file provided." };
                }

                if (file.Length > MaxFileSizeBytes)
                {
                    return new PhotoUploadResponse { Success = false, Message = "File size exceeds 10 MB limit." };
                }

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!AllowedExtensions.Contains(extension))
                {
                    return new PhotoUploadResponse { Success = false, Message = "Invalid file type. Allowed types: jpg, jpeg, png, gif, webp." };
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", userId.ToString());
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                // Create photo record
                var photo = new Photo
                {
                    UserId = userId,
                    FileName = fileName,
                    FileUrl = $"/uploads/{userId}/{fileName}",
                    ClothingType = request.ClothingType,
                    Tags = request.Tags,
                    Notes = request.Notes,
                    FileSizeBytes = file.Length,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Photos.Add(photo);
                await _dbContext.SaveChangesAsync();

                var photoDto = MapToPhotoDto(photo);
                return new PhotoUploadResponse
                {
                    Success = true,
                    Message = "Photo uploaded successfully.",
                    Photo = photoDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for user {UserId}", userId);
                return new PhotoUploadResponse { Success = false, Message = "An error occurred during upload." };
            }
        }

        public async Task<IEnumerable<PhotoDto>> GetUserPhotosAsync(int userId)
        {
            var photos = await _dbContext.Photos
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return photos.Select(MapToPhotoDto);
        }

        public async Task<PhotoDto?> GetPhotoAsync(int photoId, int userId)
        {
            var photo = await _dbContext.Photos
                .FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId);

            return photo == null ? null : MapToPhotoDto(photo);
        }

        public async Task<bool> DeletePhotoAsync(int photoId, int userId)
        {
            try
            {
                var photo = await _dbContext.Photos
                    .FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId);

                if (photo == null)
                {
                    return false;
                }

                // Delete file from disk
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", userId.ToString(), photo.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Delete from database
                _dbContext.Photos.Remove(photo);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo {PhotoId} for user {UserId}", photoId, userId);
                return false;
            }
        }

        private static PhotoDto MapToPhotoDto(Photo photo)
        {
            return new PhotoDto
            {
                Id = photo.Id,
                UserId = photo.UserId,
                FileName = photo.FileName,
                Url = photo.FileUrl,
                ClothingType = photo.ClothingType,
                Tags = photo.Tags,
                Notes = photo.Notes,
                CreatedAt = photo.CreatedAt
            };
        }
    }
}
