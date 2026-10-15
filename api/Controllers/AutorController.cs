using api.Dtos.Autor;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Roles = nameof(PerfilUsuario.Admin))]
    [ApiController]
    [Route("api/autores")]
    public class AutorController : ControllerBase
    {
        private readonly IAutorService _autorService;

        public AutorController(IAutorService autorService)
        {
            _autorService = autorService;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<AutorDto>>> GetAll([FromQuery] string? nome)
        {
            var autores = await _autorService.GetAllAsync(nome);
            return Ok(autores);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AutorDto>> GetById([FromRoute] int id)
        {
            var autor = await _autorService.GetByIdAsync(id);
            return Ok(autor);
        }

        [HttpPost]
        public async Task<ActionResult<AutorDto>> Create([FromBody] CreateAutorDto dto)
        {
            var criado = await _autorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new{id = criado.Id}, criado);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AutorDto>> Update([FromRoute] int id, [FromBody] UpdateAutorDto dto)
        {
            var atualizado = await _autorService.UpdateAsync(id, dto);
            return Ok(atualizado);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _autorService.DeleteAsync(id);
            return NoContent();
        }
    }
}