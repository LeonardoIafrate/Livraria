using api.Interfaces;
using api.Models;

namespace api.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        public Task<List<Usuario>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> GetByCpfAsync(string cpf)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> GetByNomeAsync(string nome)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> GetByUsernameAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario> CreateAsync(Usuario usuarioModel)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario?> UpdateAsync(int id, Usuario usuarioModel)
        {
            throw new NotImplementedException();
        }
    }
}