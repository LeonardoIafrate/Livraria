using api.Interfaces;
using api.Models;

namespace api.Repository
{
    public class EnderecoRepository : IEnderecoRepository
    {
        public Task<List<Endereco>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Endereco?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Endereco> CreateAsync(Endereco enderecoModel)
        {
            throw new NotImplementedException();
        }

        public Task<Endereco?> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Endereco?> UpdateAsync(int id, Endereco enderecoModel)
        {
            throw new NotImplementedException();
        }
    }
}