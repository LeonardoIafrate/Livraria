using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Autor;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/autores")]
    public class AutorController : ControllerBase
    {
        private readonly IAutorService _autorService;

        public AutorController(IAutorService autorService)
        {
            _autorService = autorService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AutorDto>>> GetAll([FromQuery] string? nome)
        {
            var categorias = await _autorService.GetAllAsync(nome);
            return Ok(categorias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AutorDto>> GetById([FromRoute] int id)
        {
            var categoria = await _autorService.GetByIdAsync(id);
            return Ok(categoria);
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