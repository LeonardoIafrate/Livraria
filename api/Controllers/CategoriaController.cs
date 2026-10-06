using api.Dtos.Categoria;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }        

        [HttpGet]
        public async Task<ActionResult<List<CategoriaDto>>> GetAll([FromQuery] string? genero)
        {
            var categorias = await _categoriaService.GetAllAsync(genero);
            return Ok(categorias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaDto>> GetById([FromRoute] int id)
        {
            var categoria = await _categoriaService.GetByIdAsync(id);
            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> Create([FromBody] CreateCategoriaDto dto)
        {
            var criada = await _categoriaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new{id = criada.Id}, criada);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoriaDto>> Update([FromRoute] int id, [FromBody] UpdateCategoriaDto dto)
        {
            var atualizada = await _categoriaService.UpdateAsync(id, dto);
            return Ok(atualizada);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _categoriaService.DeleteAsync(id);
            return NoContent();
        }
    }
}