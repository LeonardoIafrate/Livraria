using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Endereco;
using api.Models;

namespace api.Interfaces
{
    public interface IEnderecoService
    {
        Task<List<EnderecoDto>> GetByUsuarioIdAsync(int usuarioId);
        Task<EnderecoDto> GetByIdAsync(int id);
        Task<EnderecoDto> CreateAsync(int usuarioId, CreateEnderecoDto dto);
        Task<EnderecoDto> UpdateAsync(int id, UpdateEnderecoDto dto);
        Task<EnderecoDto> DefinirPrincipalAsync(int id);
        Task DesativarAsync(int id);
    }
}