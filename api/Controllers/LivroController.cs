using api.Dtos.Livro;
using api.Helpers;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/livros")]
    public class LivroController : ControllerBase
    {
        private readonly ILivroService _livroService;

        public LivroController(ILivroService livroService)
        {
            _livroService = livroService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<LivroDto>>> GetAll([FromQuery] LivroQueryObject query)
        {
            var livros = await _livroService.GetAllAsync(query);
            return Ok(livros);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LivroDto>> GetById([FromRoute]int id)
        {
            var livro = await _livroService.GetByIdAsync(id);
            return Ok(livro);   
        }

        [HttpGet("isbn/{isbn}")]
        public async Task<ActionResult<LivroDto>> GetByIsbn([FromRoute] string isbn)
        {
            var livro = await _livroService.GetByIsbnAsync(isbn);
            return Ok(livro);
        }

        [HttpPost]
        public async Task<ActionResult<LivroDto>> Create([FromBody] CreateLivroDto dto)
        {
            var criada = await _livroService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new{id = criada.Id}, criada);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<LivroDto>> Update([FromRoute] int id, [FromBody] UpdateLivroDto dto)
        {
            var atualizada = await _livroService.UpdateAsync(id, dto);
            return Ok(atualizada);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            await _livroService.DeleteAsync(id);
            return NoContent();
        }
    }
}