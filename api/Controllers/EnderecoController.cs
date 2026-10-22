using api.Dtos.Endereco;
using api.Helpers;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/endereco")]
    public class EnderecoController : ControllerBase
    {
        private readonly IEnderecoService _enderecoService;

        public EnderecoController(IEnderecoService enderecoService)
        {
            _enderecoService = enderecoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EnderecoDto>>> GetMeusEnderecos()
        {
            var enderecos = await _enderecoService.GetByUsuarioIdAsync(User.GetUsuarioByID());
            return Ok(enderecos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EnderecoDto>> GetById([FromRoute] int id)
        {
            var endereco = await _enderecoService.GetByIdAsync(id, User.GetUsuarioByID());
            return Ok(endereco);
        }

        [HttpPost]
        public async Task<ActionResult<EnderecoDto>> Create([FromBody] CreateEnderecoDto dto)
        {
            var criado = await _enderecoService.CreateAsync(User.GetUsuarioByID(), dto);
            return CreatedAtAction(nameof(GetById), new {id = criado.Id}, criado);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EnderecoDto>> Update([FromRoute] int id, [FromBody] UpdateEnderecoDto dto)
        {
            var atualizado = await _enderecoService.UpdateAsync(id, User.GetUsuarioByID(), dto);
            return Ok(atualizado);
        }

        [HttpPut("{id:int}/principal")]
        public async Task<ActionResult<EnderecoDto>> DefinirPrincipal([FromRoute]int id)
        {
            var atualizado = await _enderecoService.DefinirPrincipalAsync(id, User.GetUsuarioByID());
            return Ok(id);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir([FromRoute] int id)
        {
            await _enderecoService.ExcluirAsync(id, User.GetUsuarioByID());
            return NoContent();
        }
    }
}