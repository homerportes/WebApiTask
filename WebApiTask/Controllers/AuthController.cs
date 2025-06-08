using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApplicationLayer.Services.Auth;
using ApplicationLayer.Services.Auth.Dto;
using WebApiTask.Auth.Dto;

namespace WebApiTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtAuthService _auth;
        private readonly ILogger<AuthController> _log;

        public AuthController(IJwtAuthService auth, ILogger<AuthController> log)
        {
            _auth = auth;
            _log = log;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest req)
        {
            if (!_auth.ValidateUser(req.Username, req.Password, out var role))
                return Unauthorized(new { message = "Credenciales inválidas" });

            var resp = _auth.GenerateToken(req.Username, role);
            _log.LogInformation("User {User} logged in", req.Username);
            return Ok(resp);
        }
    }
}
