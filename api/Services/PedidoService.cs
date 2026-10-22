using api.Dtos.Pedido;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class PedidoService : IPedidoService
    {
        private static readonly Dictionary<StatusPedido, StatusPedido[]> Transicoes = new()
        {
            [StatusPedido.Pendente] = new[] {StatusPedido.Pago},
            [StatusPedido.Pago] = new[]{StatusPedido.Enviado},  
            [StatusPedido.Enviado] = new[]{StatusPedido.Entregue}  
        };

        private readonly IPedidoRepository _pedidoRepo;
        private readonly IEnderecoRepository _enderecoRepo;
        private readonly ILivroRepository _livroRepo;

        public PedidoService(IPedidoRepository pedidoRepo,
                IEnderecoRepository enderecoRepo, 
                ILivroRepository livroRepo)
        {
            _pedidoRepo = pedidoRepo;
            _enderecoRepo = enderecoRepo;
            _livroRepo = livroRepo;
        }
        public async Task<List<PedidoDto>> GetAllAsync(string? nomeUsuario)
        {
            var pedidos = string.IsNullOrWhiteSpace(nomeUsuario)
                ? await _pedidoRepo.GetAllAsync()
                : await _pedidoRepo.GetByNomeUsuarioAsync(nomeUsuario);
            return pedidos.Select(p => p.ToPedidoDto()).ToList();
        }

        public async Task<List<PedidoDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var pedidos = await _pedidoRepo.GetByIdUsuarioAsync(usuarioId);
            return pedidos.Select(p => p.ToPedidoDto()).ToList();
        }

        public async Task<PedidoDto> GetByIdAsync(int id, int usuarioId, bool ehAdmin)
        {
            var pedido = await ObterPedidoAcessivelAsync(id, usuarioId, ehAdmin);
            return pedido.ToPedidoDto();
        }

        public async Task<PedidoDto> CreatePedidoAsync(int usuarioId, CreatePedidoDto pedidoDto)
        {
            var endereco = await _enderecoRepo.GetByIdAsync(pedidoDto.EnderecoId);
            
            if(endereco == null || !endereco.Ativo || endereco.UsuarioId != usuarioId)
                throw new BadRequestException("O endereço de entrega informado não existe.");

            var quantidades = pedidoDto.Itens
                                .GroupBy(i => i.LivroId)
                                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));
            
            var livros = await _livroRepo.GetByIdsAsync(quantidades.Keys.ToList());

            var inexistentes = quantidades.Keys.Except(livros.Select(l => l.Id)).ToList();
            if(inexistentes.Count > 0)
                throw new BadRequestException($"Livros inexistentes: {string.Join(",", inexistentes)}.");

            var pedido = new Pedido
            {
              UsuarioId = usuarioId,
              EnderecoEntrega = endereco.ToEnderecoEntrega(),
              Status = StatusPedido.Pendente  
            };

            foreach(var livro in livros)
            {
                var quantidade = quantidades[livro.Id];
                if (livro.Estoque == null || livro.Estoque.Quantidade < quantidade)
                    throw new ConflictException($"Estoque insuficiente para o livro {livro.Nome}.");

                livro.Estoque.Quantidade -= quantidade;
                livro.Estoque.DataAtualizacao = DateTime.UtcNow;

                pedido.Itens.Add(new ItemPedido
                {
                   LivroId = livro.Id,
                   Quantidade = quantidade,
                   PrecoUnitario = livro.Preco 
                });
            }

            pedido.ValorTotal = pedido.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

            var criado = await _pedidoRepo.CreatePedidoAsync(pedido);
            return criado.ToPedidoDto();
        }

        public async Task<PedidoDto> UpdateEnderecoPedidoAsync(int id, int usuarioId, AlterarEnderecoPedidoDto enderecoPedidoDto)
        {
            var pedido = await ObterPedidoAcessivelAsync(id, usuarioId, ehAdmin: false);
            if(pedido.Status is not (StatusPedido.Pendente or StatusPedido.Pago))
                throw new ConflictException("O endereço só pode ser alterado antes do envio.");

            var endereco = await _enderecoRepo.GetByIdAsync(enderecoPedidoDto.EnderecoId);
            if(endereco == null || !endereco.Ativo || endereco.UsuarioId != usuarioId)
                throw new BadRequestException("O endereço informado não existe.");

            var atualizado = await _pedidoRepo.UpdateEnderecoPedidoAsync(id, endereco.ToEnderecoEntrega());
            if(atualizado == null)
                throw new NotFoundException("Nenhum pedido econtrado.");

            return atualizado.ToPedidoDto();
        }

        public async Task<PedidoDto> AtualizarStatusAsync(int id, StatusPedido novoStatus)
        {
            var pedido = await _pedidoRepo.GetByIdAsync(id);
            if(pedido == null)
                throw new NotFoundException("Nenhum pedido encontrado com o Id informado");

            if(!Transicoes.TryGetValue(pedido.Status, out var permitidos) || !permitidos.Contains(novoStatus))
                throw new ConflictException($"Não é possível mudar o pedido {pedido.Status} para {novoStatus}.");

            var atualizado = await _pedidoRepo.AtualizarStatusAsync(id, novoStatus);
            if(atualizado == null)
                throw new NotFoundException("Nenhum pedido encontrado.");
            return atualizado.ToPedidoDto();
        }

        public async Task<PedidoDto> CancelarPedidoAsync(int id, int usuarioId, bool ehAdmin)
        {
            var pedido = await ObterPedidoAcessivelAsync(id, usuarioId, ehAdmin);

            if(pedido.Status is not (StatusPedido.Pendente or StatusPedido.Pago))
                throw new NotFoundException("Só é possível cancelar pedidos pendentes ou pagos.");

            var cancelado = await _pedidoRepo.CancelarPedidoAsync(id);
            if(cancelado == null)
                throw new NotFoundException("Nenhum pedido encontrado com esse Id.");

            return pedido.ToPedidoDto();
        }

        private async Task<Pedido> ObterPedidoAcessivelAsync(int id, int usuarioId, bool ehAdmin)
        {
            var pedido = await _pedidoRepo.GetByIdAsync(id);

            if(pedido == null || (!ehAdmin && pedido.UsuarioId != usuarioId))
                throw new NotFoundException("Nenhum pedido encontrado com esse Id.");

            return pedido;
        }
    }
}