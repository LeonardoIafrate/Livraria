using api.Dtos.Usuario;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/Usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioDto>>> GetAll([FromQuery]string? nome)
        {
            var usuarios = await _usuarioService.GetAllAsync(nome);
            return Ok(usuarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> GetById([FromRoute] int id)
        {
            var usuario = await _usuarioService.GetByIdAsync(id);
            return Ok(usuario);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UsuarioDto>> GetByEmail([FromRoute] string email)
        {
            var usuario = await _usuarioService.GetByEmailAsync(email);
            return Ok(usuario);
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto dto)
        {
            var criado = await _usuarioService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new {id = criado.Id}, criado);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> Update([FromRoute] int id, [FromBody] UpdateUsuarioDto dto)
        {
            var atualizado = await _usuarioService.UpdateAsync(id, dto);
            return Ok(atualizado);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Desativar([FromRoute] int id)
        {
            await _usuarioService.DesativarAsync(id);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _usuarioService.DeleteAsync(id);
            return NoContent();
        }
    }
}