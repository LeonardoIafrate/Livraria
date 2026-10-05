using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Categoria;
using api.Models;

namespace api.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> GetAllAsync(string? genero);
        Task<CategoriaDto> GetByIdAsync(int id);
        Task<CategoriaDto> CreateAsync(CreateCategoriaDto createDto); 
        Task<CategoriaDto> UpdateAsync(int id, UpdateCategoriaDto updateDto);
        Task DeleteAsync(int id);
    }
}