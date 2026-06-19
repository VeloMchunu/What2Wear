namespace What2Wear.Shared.Models
{
    public class PhotoUploadRequest
    {
        public string ClothingType { get; set; } = null!;
        public string? Tags { get; set; }
        public string? Notes { get; set; }
    }

    public class PhotoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string ClothingType { get; set; } = null!;
        public string? Tags { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PhotoUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public PhotoDto? Photo { get; set; }
    }
}
