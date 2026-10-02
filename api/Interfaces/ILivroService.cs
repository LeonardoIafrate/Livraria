using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Livro;
using api.Helpers;
using api.Models;

namespace api.Interfaces
{
    public interface ILivroService
    {
        Task<PagedResult<LivroDto>> GetAllAsync(LivroQueryObject query);
        Task<LivroDto> GetByIdAsync(int id);
        Task<LivroDto> GetByIsbnAsync(string isbn);
        Task<LivroDto> CreateAsync(CreateLivroDto createDto);
        Task<LivroDto> UpdateAsync(int id, UpdateLivroDto updateDto);
        Task DeleteAsync(int id);
    }
}