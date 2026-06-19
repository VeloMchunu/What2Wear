using Microsoft.AspNetCore.Mvc;
using What2Wear.Shared.Models;
using What2Wear.Web.Services;

namespace What2Wear.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Signup attempt for email: {Email}", request.Email);
            var response = await _authService.SignupAsync(request);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            var response = await _authService.LoginAsync(request);

            return response.Success ? Ok(response) : Unauthorized(response);
        }
    }
}
