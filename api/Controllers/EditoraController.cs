using api.Dtos.Editora;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(Roles = nameof(PerfilUsuario.Admin))]
    [ApiController]
    [Route("api/editoras")]
    public class EditoraController : ControllerBase
    {
        private readonly IEditoraService _editoraService;

        public EditoraController(IEditoraService editoraService)
        {
            _editoraService = editoraService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<EditoraDto>>> GetAll([FromQuery] string? nome)
        {
            var editoras = await _editoraService.GetAllAsync(nome);
            return Ok(editoras);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EditoraDto>> GetById([FromRoute] int id)
        {
            var editora = await _editoraService.GetByIdAsync(id);
            return Ok(editora);
        }

        [HttpPost]
        public async Task<ActionResult<EditoraDto>> Create([FromBody] CreateEditoraDto dto)
        {
            var criada = await _editoraService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new {id = criada.Id}, criada);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EditoraDto>> Update([FromBody] UpdateEditoraDto dto, [FromRoute]int id)
        {
            var atualizada = await _editoraService.UpdateAsync(dto, id);
            return Ok(atualizada);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _editoraService.DeleteAsync(id);
            return NoContent();
        }
    }
}