using api.Dtos.Pedido;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpGet("meus-pedidos")]
        public async Task<ActionResult<List<PedidoDto>>> GetMeusPedidos()
        {
            var pedidos = await _pedidoService.GetByUsuarioIdAsync(User.GetUsuarioByID());
            return Ok(pedidos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoDto>> GetById([FromRoute] int id)
        {
            var pedido = await _pedidoService.GetByIdAsync(id, User.GetUsuarioByID(), User.EhAdmin());
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<PedidoDto>> Create([FromBody] CreatePedidoDto dto)
        {
            var criado = await _pedidoService.CreatePedidoAsync(User.GetUsuarioByID(), dto);
            return CreatedAtAction(nameof(GetById), new{id = criado.Id}, criado);
        }

        [HttpPatch("{id:int}/endereco")]
        public async Task<ActionResult<PedidoDto>> UpdateEndereco([FromRoute] int id, [FromBody] AlterarEnderecoPedidoDto dto)
        {
            var atualizado = await _pedidoService.UpdateEnderecoPedidoAsync(id, User.GetUsuarioByID(), dto);
            return Ok(atualizado);
        }

        [HttpPatch("{id:int}/cancelar")]
        public async Task<ActionResult<PedidoDto>> Cancelar([FromRoute] int id)
        {
            var cancelado = await _pedidoService.CancelarPedidoAsync(id, User.GetUsuarioByID(), User.EhAdmin());
            return Ok(cancelado);
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpGet]
        public async Task<ActionResult<List<PedidoDto>>> GetAll([FromQuery] string? nome)
        {
            var pedidos = await _pedidoService.GetAllAsync(nome);
            return Ok(pedidos);
        }

        [Authorize(Roles = nameof(PerfilUsuario.Admin))]
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<PedidoDto>> AtualizarStatus([FromRoute] int id, [FromBody] StatusPedido novoStatus)
        {
            var atualizado = await _pedidoService.AtualizarStatusAsync(id, novoStatus);
            return Ok(atualizado);
        }

    }
}