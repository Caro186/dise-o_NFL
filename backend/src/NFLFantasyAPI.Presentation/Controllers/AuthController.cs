using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using Microsoft.Extensions.Logging;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroDto registroDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registroDto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("desbloquear")]
        public async Task<IActionResult> Desbloquear([FromBody] DesbloquearCuentaDto dto)
        {
            var result = await _authService.DesbloquearCuentaAsync(dto.Email);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var result = await _authService.GetUsuariosAsync();
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("usuario/{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var result = await _authService.GetUsuarioAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpDelete("usuario/{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var result = await _authService.DeleteUsuarioAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
