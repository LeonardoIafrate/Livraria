using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;

namespace api.Repository
{
    public class AutorRepository : IAutorRepository
    {
        public Task<List<Autor>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Autor?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Autor?> GetByNomeAsync(string nome)
        {
            throw new NotImplementedException();
        }

        public Task<List<Autor>> SearchByNomeAsync(string nome)
        {
            throw new NotImplementedException();
        }

        public Task<Autor> CreateAsync(Autor autor)
        {
            throw new NotImplementedException();
        }

        public Task<Autor?> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Exists(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PossuiLivros(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Autor?> UpdateAsync(int id, Autor autor)
        {
            throw new NotImplementedException();
        }
    }
}