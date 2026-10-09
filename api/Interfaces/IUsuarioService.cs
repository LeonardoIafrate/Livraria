using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Usuario;

namespace api.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDto>> GetAllAsync(string? nome);
        Task<UsuarioDto> GetByIdAsync(int id);
        Task<UsuarioDto> GetByEmailAsync(string email);
        Task<UsuarioDto> GetByCpfAsync(string cpf);
        Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto);
        Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto);
        Task DeleteAsync(int id);
        Task DesativarAsync(int id);
    }
}