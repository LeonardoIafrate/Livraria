using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Editora;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/editoras")]
    public class EditoraController : ControllerBase
    {
        private readonly IEditoraService _editoraService;

        public EditoraController(IEditoraService editoraService)
        {
            _editoraService = editoraService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EditoraDto>>> GetAll([FromQuery] string? nome)
        {
            var editoras = await _editoraService.GetAllAsync(nome);
            return Ok(editoras);
        }

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