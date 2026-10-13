using api.Dtos.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(token);
        }

        [Authorize]
        [HttpGet("claims")]
        public IActionResult Claims()
        {
            var claims = User.Claims.Select(c => new {c.Type, c.Value});
            return Ok(claims);
        }
    }
}