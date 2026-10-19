using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _dbContext;

        public PedidoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Pedido>> GetAllAsync()
        {
            return await _dbContext.Pedido.AsNoTracking()
                            .Include(p => p.Usuario)
                            .Include(p => p.Endereco)
                            .Include(p => p.Itens)
                                .ThenInclude(i => i.Livro)
                            .OrderByDescending(p => p.DataPedido)
                            .ToListAsync();
        }

        public async Task<Pedido?> GetByIdAsync(int id)
        {
            return await _dbContext.Pedido
                            .Include(p => p.Usuario)
                            .Include(p => p.Endereco)
                            .Include(p => p.Itens)
                                .ThenInclude(i => i.Livro)
                            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Pedido>> GetByIdUsuarioAsync(int usuarioId)
        {
            return await _dbContext.Pedido
                            .AsNoTracking()
                            .Where(p => p.UsuarioId == usuarioId)
                            .Include(p => p.Usuario)
                            .Include(p => p.Endereco)
                            .Include(p => p.Itens)
                                .ThenInclude(i => i.Livro)
                            .OrderByDescending(p => p.DataPedido)
                            .ToListAsync();
        }

        public async Task<List<Pedido>> GetByNomeUsuarioAsync(string nomeUsuario)
        {
            var nome = nomeUsuario.Trim();

            return await _dbContext.Pedido
                            .AsNoTracking()
                            .Where(p => p.Usuario.Nome.Contains(nome))
                            .Include(p => p.Usuario)
                            .Include(p => p.Endereco)
                            .Include(p => p.Itens)
                                .ThenInclude(i => i.Livro)
                            .OrderByDescending(p => p.DataPedido)
                            .ToListAsync();
        }

        public async Task<Pedido> CreatePedidoAsync(Pedido pedidoModel)
        {
            await _dbContext.Pedido.AddAsync(pedidoModel);
            await _dbContext.SaveChangesAsync();
            
            return await GetByIdAsync(pedidoModel.Id) ?? pedidoModel;
        }

        public async Task<Pedido?> UpdateEnderecoPedidoAsync(int id, int enderecoId)
        {
            var pedidoExistente = await _dbContext.Pedido.FirstOrDefaultAsync(p => p.Id == id);
            if(pedidoExistente == null)
                return null;

            pedidoExistente.EnderecoId = enderecoId;
            await _dbContext.SaveChangesAsync();

            return pedidoExistente;
        }

        public async Task<Pedido?> AtualizarStatusAsync(int id, StatusPedido status)
        {
            var pedidoExistente = await _dbContext.Pedido.FirstOrDefaultAsync(p => p.Id == id);
            if(pedidoExistente == null)
                return null;

            pedidoExistente.Status = status;
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<Pedido?> CancelarPedidoAsync(int id)
        {
            var pedidoExistente = await _dbContext.Pedido.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id);
            if(pedidoExistente == null)
                return null;

            var livroIds = pedidoExistente.Itens.Select(i => i.LivroId).ToList();

            var estoques = await _dbContext.Estoque
                            .Where(e => livroIds
                            .Contains(e.LivroId))
                            .ToListAsync();

            foreach(var item in pedidoExistente.Itens)
            {
                var estoque = estoques.FirstOrDefault(e => e.LivroId == item.LivroId);

                if(estoque != null)
                {
                    estoque.Quantidade += item.Quantidade;
                    estoque.DataAtualizacao = DateTime.UtcNow;
                }
            }

            pedidoExistente.Status = StatusPedido.Cancelado;

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(id);

        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Pedido.AnyAsync(p => p.Id == id);
        }
    }
}