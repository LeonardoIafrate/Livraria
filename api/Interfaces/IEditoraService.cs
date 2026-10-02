using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Editora;
using api.Models;

namespace api.Interfaces
{
    public interface IEditoraService
    {
        Task<List<EditoraDto>> GetAllAsync(string? nome);
        Task<EditoraDto> GetByIdAsync(int id);
        Task<EditoraDto> CreateAsync(CreateEditoraDto createDto);
        Task<EditoraDto> UpdateAsync(UpdateEditoraDto updateDto, int id);
        Task DeleteAsync(int id);
    }
}