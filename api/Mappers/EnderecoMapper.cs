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

        public static EnderecoEntrega ToEnderecoEntrega(this Endereco endereco)
        {
            return new EnderecoEntrega
            {
                Cep = endereco.Cep,
                Rua = endereco.Rua,
                Numero = endereco.Numero,
                Complemento = endereco.Complemento,
                Bairro = endereco.Bairro,
                Cidade = endereco.Cidade,
                Uf = endereco.Uf
            };
        }
        public static string ToEnderecoFormatado(this EnderecoEntrega e)
        {
            return Formatar(e.Rua, e.Numero, e.Bairro, e.Cidade, e.Uf, e.Complemento, e.Cep);
        }

        public static string ToEnderecoFormatado(this Endereco e)
        {
            return Formatar(e.Rua, e.Numero, e.Bairro, e.Cidade, e.Uf, e.Complemento, e.Cep);
        }


         private static string Formatar(
            string rua, string numero, string bairro, string cidade, string uf,
            string? complemento, string cep)
        {
            var linhas = new List<string>
            {
                $"{rua}, {numero}",
                bairro,
                $"{cidade}/{uf}"
            };
 
            if (!string.IsNullOrWhiteSpace(complemento))
                linhas.Add(complemento);

            linhas.Add(cep.Length == 8 ? $"{cep[..5]}-{cep[5..]}" : cep);
 
            return string.Join("\n", linhas);
        }
    }
}