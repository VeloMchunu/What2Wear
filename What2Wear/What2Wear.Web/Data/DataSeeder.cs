using What2Wear.Web.Models;

namespace What2Wear.Web.Data
{
    public class DataSeeder
    {
        private readonly What2WearDbContext _context;

        public DataSeeder(What2WearDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Only seed if no users exist
            if (_context.Users.Any())
            {
                return;
            }

            var dummyUsers = new List<User>
            {
                new User
                {
                    Email = "alice@example.com",
                    FullName = "Alice Johnson",
                    PasswordHash = "placeholder_hash_1",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "bob@example.com",
                    FullName = "Bob Smith",
                    PasswordHash = "placeholder_hash_2",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "charlie@example.com",
                    FullName = "Charlie Brown",
                    PasswordHash = "placeholder_hash_3",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "diana@example.com",
                    FullName = "Diana Prince",
                    PasswordHash = "placeholder_hash_4",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "eve@example.com",
                    FullName = "Eve Wilson",
                    PasswordHash = "placeholder_hash_5",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Users.AddRangeAsync(dummyUsers);
            await _context.SaveChangesAsync();
        }
    }
}
