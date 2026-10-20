using api.Dtos.Usuario;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto dto)
        {
            var criado = await _usuarioService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new {id = criado.Id}, criado);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UsuarioDto>> GetMe()
        {
            var usuario = await _usuarioService.GetByIdAsync(User.GetUsuarioByID());
            return Ok(usuario);
        }

        [HttpPut("me")]         
        public async Task<ActionResult<UsuarioDto>> Update([FromBody] UpdateUsuarioDto dto)
        {
            var atualizado = await _usuarioService.UpdateAsync(User.GetUsuarioByID(), dto);
            return Ok(atualizado);
        }

        [HttpPut("me/senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            await _usuarioService.AlterarSenhaAsync(User.GetUsuarioByID(), dto);
            return NoContent();
        }

        [HttpPatch("me/desativar")]
        public async Task<IActionResult> Desativar()
        {
            await _usuarioService.DesativarAsync(User.GetUsuarioByID());
            return NoContent();
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpGet]
        public async Task<ActionResult<List<UsuarioDto>>> GetAll([FromQuery]string? nome)
        {
            var usuarios = await _usuarioService.GetAllAsync(nome);
            return Ok(usuarios);
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> GetById([FromRoute] int id)
        {
            var usuario = await _usuarioService.GetByIdAsync(id);
            return Ok(usuario);
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UsuarioDto>> GetByEmail([FromRoute] string email)
        {
            var usuario = await _usuarioService.GetByEmailAsync(email);
            return Ok(usuario);
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpPatch("{id:int}/desativar")]
        public async Task<IActionResult> Desativar([FromRoute] int id)
        {
            await _usuarioService.DesativarAsync(id);
            return NoContent();
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _usuarioService.DeleteAsync(id);
            return NoContent();
        }
    }
}