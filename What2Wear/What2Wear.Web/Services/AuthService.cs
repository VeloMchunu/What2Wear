using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using What2Wear.Shared.Models;
using What2Wear.Web.Data;
using What2Wear.Web.Models;

namespace What2Wear.Web.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> SignupAsync(SignupRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        string GenerateJwtToken(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly What2WearDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(What2WearDbContext dbContext, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> SignupAsync(SignupRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return new AuthResponse { Success = false, Message = "User already exists with this email." };
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Signup successful.",
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup");
                return new AuthResponse { Success = false, Message = "An error occurred during signup." };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new AuthResponse { Success = false, Message = "Invalid email or password." };
                }

                if (!user.IsActive)
                {
                    return new AuthResponse { Success = false, Message = "User account is inactive." };
                }

                var token = GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthResponse { Success = false, Message = "An error occurred during login." };
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "your-secret-key-change-this-in-production"));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpirationHours"] ?? "24")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
