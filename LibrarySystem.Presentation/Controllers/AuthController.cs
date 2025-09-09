using LibrarySystem.Presentation.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwt;
        private readonly List<AppUser> _users;

        public AuthController(IJwtTokenService jwt, List<AppUser> users)
        {
            _jwt = jwt;
            _users = users;
        }

        public record LoginRequest(string Username, string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username and Password are required.");

            var u = _users.FirstOrDefault(x =>
                x.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase) &&
                x.Password == req.Password);

            if (u is null) return Unauthorized("Invalid credentials.");

            var token = _jwt.CreateToken(u.Username, u.Role);
            return Ok(new { token, role = u.Role });
        }
    }
}
