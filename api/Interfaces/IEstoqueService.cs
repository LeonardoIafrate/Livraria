using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Estoque;
using api.Models;

namespace api.Interfaces
{
    public interface IEstoqueService
    {
        Task<List<EstoqueDto>> GetAllAsync(string? nomeLivro);
        Task<EstoqueDto> GetByIdAsync(int id);
        Task<EstoqueDto> GetByLivroId(int livroId);
        Task<EstoqueDto> UpdateAsync(int id, UpdateEstoqueDto dto);
    }
}