using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Endereco;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnderecoController : ControllerBase
    {
        private readonly IEnderecoService _enderecoService;

        public EnderecoController(IEnderecoService enderecoService)
        {
            _enderecoService = enderecoService;
        }

        [HttpGet("usuarios/{usuarioId:int}/enderecos")]
        [Authorize]
        public async Task<ActionResult<List<EnderecoDto>>> GetByUsuarioId([FromRoute] int usuarioId)
        {
            var enderecos = await _enderecoService.GetByUsuarioIdAsync(usuarioId);
            return Ok(enderecos);
        }

        [HttpGet("enderecos/{id:int}")]
        [Authorize]
        public async Task<ActionResult<EnderecoDto>> GetById([FromRoute] int id)
        {
            var endereco = await _enderecoService.GetByIdAsync(id);
            return Ok(endereco);
        }

        [HttpPost("usuario/{usuarioId:int}/enderecos")]
        [Authorize]
        public async Task<ActionResult<EnderecoDto>> Create([FromRoute] int usuarioId, [FromBody] CreateEnderecoDto dto)
        {
            var criado = await _enderecoService.CreateAsync(usuarioId, dto);
            return CreatedAtAction(nameof(GetById), new {id = criado.Id}, criado);
        }

        [HttpPut("enderecos/{id:int}")]
        [Authorize]
        public async Task<ActionResult<EnderecoDto>> Update([FromRoute] int id, [FromBody] UpdateEnderecoDto dto)
        {
            var atualizado = await _enderecoService.UpdateAsync(id, dto);
            return Ok(atualizado);
        }

        [HttpPut("enderecos/{id:int}/principal")]
        [Authorize]
        public async Task<ActionResult<EnderecoDto>> DefinirPrincipal([FromRoute]int id)
        {
            var atualizado = await _enderecoService.DefinirPrincipalAsync(id);
            return Ok(id);
        }

        [HttpDelete("enderecos/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Desativar([FromRoute] int id)
        {
            await _enderecoService.DesativarAsync(id);
            return NoContent();
        }
    }
}