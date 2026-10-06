using api.Dtos.Estoque;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/estoques")]
    public class EstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;
        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EstoqueDto>>> GetAll([FromQuery]string? nomeLivro)
        {
            var estoques = await _estoqueService.GetAllAsync(nomeLivro);
            return Ok(estoques);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EstoqueDto>> GetById([FromRoute] int id)
        {
            var estoque = await _estoqueService.GetByIdAsync(id);
            return Ok(estoque);
        }

        [HttpGet("livro/{livroId:int}")]
        public async Task<ActionResult<EstoqueDto>> GetByLivroId([FromRoute] int livroId)
        {
            var estoque = await _estoqueService.GetByLivroIdAsync(livroId);
            return Ok(estoque);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EstoqueDto>> Update([FromRoute] int id, [FromBody] UpdateEstoqueDto dto)
        {
            var atualizado = await _estoqueService.UpdateAsync(id, dto);
            return Ok(atualizado);
        }
    }
}