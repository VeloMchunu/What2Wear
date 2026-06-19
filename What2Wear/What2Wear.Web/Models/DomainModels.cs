namespace What2Wear.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }

    public class Photo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string ClothingType { get; set; } = null!;
        public string? Tags { get; set; }
        public string? Notes { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
