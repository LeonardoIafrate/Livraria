using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Endereco;

namespace api.Interfaces
{
    public interface IEnderecoService
    {
        Task<List<EnderecoDto>> GetByUsuarioId(int id);
        Task<EnderecoDto> GetById(int id);
        Task<EnderecoDto> CreateAsync(CreateEnderecoDto dto);
        Task<EnderecoDto> UpdateAsync(UpdateEnderecoDto dto);
        Task DeleteAsync (int id);
    }
}