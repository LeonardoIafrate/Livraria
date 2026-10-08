using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class EnderecoRepository : IEnderecoRepository
    {
        private readonly AppDbContext _dbContext;
        public EnderecoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Endereco>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _dbContext.Endereco
                        .AsNoTracking()
                        .Where(e => e.UsuarioId == usuarioId && e.Ativo)
                        .OrderByDescending(e => e.Principal)
                        .ThenBy(e => e.Rua)
                        .ToListAsync();
        }

        public async Task<Endereco?> GetByIdAsync(int id)
        {
            return await _dbContext.Endereco.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> PossuiEnderecoAtivo(int usuarioId)
        {
            return await _dbContext.Endereco.AnyAsync(e => e.UsuarioId == usuarioId && e.Ativo);
        }

        public async Task<Endereco> CreateAsync(Endereco enderecoModel)
        {
            await _dbContext.Endereco.AddAsync(enderecoModel);
            await _dbContext.SaveChangesAsync();
            return enderecoModel;
        }

        public async Task<Endereco?> UpdateAsync(int id, Endereco enderecoModel)
        {
            var existente = await _dbContext.Endereco.FirstOrDefaultAsync(e => e.Id == id);
            if(existente == null)
                return null;
            
            existente.Cep = enderecoModel.Cep;
            existente.Rua = enderecoModel.Rua;
            existente.Numero = enderecoModel.Numero;
            existente.Complemento = enderecoModel.Complemento;
            existente.Bairro = enderecoModel.Bairro;
            existente.Cidade = enderecoModel.Cidade;
            existente.Uf = enderecoModel.Uf;

            await _dbContext.SaveChangesAsync();
            return existente;
        }

        public async Task<Endereco?> DefinirPrincipalAsync(int id)
        {
            var endereco = await _dbContext.Endereco.FirstOrDefaultAsync(e => e.Id == id && e.Ativo);
            if(endereco == null)
                return null;

            await using var transacao = await _dbContext.Database.BeginTransactionAsync();

            var anteriores = await _dbContext.Endereco
                .Where(e => e.UsuarioId == endereco.UsuarioId && e.Principal && e.Id != id)
                .ToListAsync();

            foreach(var anterior in anteriores)
                anterior.Principal = false;

            await _dbContext.SaveChangesAsync();

            endereco.Principal = true;
            await _dbContext.SaveChangesAsync();

            await transacao.CommitAsync();
            return endereco;
        }

        public async Task<Endereco?> DesativaAsync(int id)
        {
            var enderecoModel = await _dbContext.Endereco.FirstOrDefaultAsync(e => e.Id == id);
            if(enderecoModel == null)
                return null;

            enderecoModel.Ativo = false;
            enderecoModel.Principal = false;
            await _dbContext.SaveChangesAsync();
            
            return enderecoModel;
        }
    }
}