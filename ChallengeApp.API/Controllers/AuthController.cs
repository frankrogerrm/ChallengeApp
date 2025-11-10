using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChallengeApp.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("users")]
        public IActionResult GetMockUsers()
        {
            var result = Users.Select(u => new
            {
                Email = u.Email,
                Role = u.Role
            }).ToList();

            return Ok(result);
        }

        private static readonly List<MockUser> Users = new()
        {
            new MockUser { Email = "admin@company.com", Password = "Admin@123", Role = "Admin" },
            new MockUser { Email = "contributor@company.com", Password = "Contributor@123", Role = "Contributor" },
            new MockUser { Email = "manager@company.com", Password = "Manager@123", Role = "Manager" },
            new MockUser { Email = "viewer@company.com", Password = "Viewer@123", Role = "Viewer" }
        };

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = Users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = GenerateToken(user);
            return Ok(new
            {
                token,
                user = new { user.Email, user.Role }
            });
        }

        private string GenerateToken(MockUser user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "supersecretkey12345");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: "ChallengeApp",
                audience: "ChallengeAppUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class MockUser
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }
    }
}
