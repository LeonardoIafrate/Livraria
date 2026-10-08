using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _dbContext;
        public UsuarioRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _dbContext.Usuario
                        .AsNoTracking()
                        .OrderBy(u => u.Nome)
                        .ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _dbContext.Usuario.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> GetByCpfAsync(string cpf)
        {
            return await _dbContext.Usuario.FirstOrDefaultAsync(u => u.Cpf == cpf);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbContext.Usuario.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<Usuario>> SearchByNomeAsync(string nome)
        {
            var termo = nome.Trim();
            return await _dbContext.Usuario
                        .AsNoTracking()
                        .Where(u => u.Nome.Contains(termo))
                        .ToListAsync();
        }

        public async Task<Usuario> CreateAsync(Usuario usuarioModel)
        {
            await _dbContext.Usuario.AddAsync(usuarioModel);
            await _dbContext.SaveChangesAsync();
            return usuarioModel;
        }

        public async Task<Usuario?> UpdateAsync(int id, Usuario usuarioModel)
        {
            var existente = await _dbContext.Usuario.FirstOrDefaultAsync(u => u.Id == id);

            if(existente == null)
                return null;

            existente.Nome = usuarioModel.Nome;
            existente.Email = usuarioModel.Email;
            existente.DataNascimento = usuarioModel.DataNascimento;

            await _dbContext.SaveChangesAsync();
            return existente;
        }

        public async Task<Usuario?> DeleteAsync(int id)
        {
            var usuarioModel = await _dbContext.Usuario.FirstOrDefaultAsync(u => u.Id == id);
            if(usuarioModel == null)
                return null;
            
            _dbContext.Usuario.Remove(usuarioModel);
            await _dbContext.SaveChangesAsync();

            return usuarioModel;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Usuario.AnyAsync(u => u.Id == id);
        }

        public async Task<Usuario?> DesativarAsync(int id)
        {
            var existente = await _dbContext.Usuario.Include(u => u.Enderecos).FirstOrDefaultAsync(u => u.Id == id);
            if(existente == null)
                return null;

            existente.Ativo = false;
            
            foreach (var endereco in existente.Enderecos){
                endereco.Ativo = false;
                endereco.Principal = false;
            }

            await _dbContext.SaveChangesAsync();
            return existente;
        }
    }
}