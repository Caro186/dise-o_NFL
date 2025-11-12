using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
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
