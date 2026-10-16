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
              Cep = endereco.Cep,
              Rua = endereco.Rua,
              Numero = endereco.Numero,
              Complemento = endereco.Complemento,
              Bairro = endereco.Bairro,
              Cidade = endereco.Cidade,
              Uf = endereco.Uf,
              Principal = endereco.Principal,
              Ativo = endereco.Ativo
            };
        }

        public static Endereco ToEnderecoFromBaseDto(this EnderecoBaseDto dto, bool principal = true)
        {
            return new Endereco
            {
                Cep = CpfHellper.SomenteDigitos(dto.Cep),
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

        public static string ToEnderecoFormatado(this Endereco endereco)
        {
            var linhas = new List<string>
            {
              $"{endereco.Rua}, {endereco.Numero}",
              endereco.Bairro,
              $"{endereco.Cidade}, {endereco.Uf}"  
            };

            if(!string.IsNullOrWhiteSpace(endereco.Complemento))
                linhas.Add(endereco.Complemento);

            return string.Join("\n", linhas);
        }

        private static string FormatarCep(string cep)
        {
            return cep.Length == 8 ? $"{cep[5]}-{cep[5..]}" : cep;
        }
    }
}