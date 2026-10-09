using api.Dtos.Endereco;
using api.Helpers;
using api.Models;

namespace api.Mappers
{
    public static class EnderecoMapper
    {
        public static EnderecoDto ToEnderecoDto(this Endereco endereco)
        {
            return new EnderecoDto
            {
              Id = endereco.Id,
              Cep = endereco.Cep.Trim(),
              Rua = endereco.Rua.Trim(),
              Numero = endereco.Numero.Trim(),
              Complemento = endereco.Complemento.FormatarOpcional(),
              Bairro = endereco.Bairro.Trim(),
              Cidade = endereco.Cidade.Trim(),
              Uf = endereco.Uf.Trim().ToUpperInvariant(),
              Principal = endereco.Principal,
              Ativo = endereco.Ativo
            };
        }

        public static Endereco ToEnderecoFromBaseDto(this EnderecoBaseDto dto, bool principal = true)
        {
            return new Endereco
            {
                Cep = dto.Cep.Trim(),
                Rua = dto.Rua.Trim(),
                Numero = dto.Numero.Trim(),
                Complemento = dto.Complemento.FormatarOpcional(),
                Bairro = dto.Bairro.Trim(),
                Cidade = dto.Cidade.Trim(),
                Uf = dto.Uf.Trim().ToUpperInvariant(),
                Principal = principal,
                Ativo = true
            };
        }

        public static Endereco ToEnderecoFromCreateDto(this CreateEnderecoDto dto)
        {
            return new Endereco
            {
              Cep = dto.Endereco.Cep,
              Rua = dto.Endereco.Rua.Trim(),
              Numero = dto.Endereco.Numero.Trim(),
              Complemento = dto.Endereco.Complemento.FormatarOpcional(),
              Bairro = dto.Endereco.Bairro.Trim(),
              Cidade = dto.Endereco.Cidade.Trim(),
              Uf = dto.Endereco.Uf.Trim(),
              Principal = dto.Principal,
              Ativo = true
            };
        }

        public static Endereco ToEnderecoFromUpdate(this UpdateEnderecoDto dto)
        {
            return new Endereco
            {
              Cep = dto.Endereco.Cep,
              Rua = dto.Endereco.Rua.Trim(),
              Numero = dto.Endereco.Numero.Trim(),
              Complemento = dto.Endereco.Complemento.FormatarOpcional(),
              Bairro = dto.Endereco.Bairro.Trim(),
              Cidade = dto.Endereco.Cidade.Trim(),
              Uf = dto.Endereco.Uf.Trim()
            };
        }
    }
}