using LightSensorSimulator.Data;
using LightSensorSimulator.SimulatorServices;
using Microsoft.AspNetCore.Mvc;

namespace LightSensorSimulator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] LoginRequest request)
        {
            var token = await _tokenService.AuthenticateAsync(request.Username, request.Password);
            return token == null ? Unauthorized(new { message = "Username or password is incorrect" })
                : Ok(new LoginResponse { Token = token });
        }
    }
}
