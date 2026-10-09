using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Usuario;
using api.Models;

namespace api.Mappers
{
    public static class UsuarioMapper
    {
        public static UsuarioDto ToUsuarioDto(this Usuario usuario)
        {
            return new UsuarioDto
            {
              Id = usuario.Id,
              Nome = usuario.Nome,
              Email = usuario.Email,
              Cpf = usuario.Cpf,
              DataNascimento = usuario.DataNascimento,
              Perfil = usuario.Perfil,
              DataCadastro = usuario.DataCadastro,
              Ativo = usuario.Ativo  
            };
        }

        public static Usuario ToUsuarioFromCreateDto(this CreateUsuarioDto dto)
        {
            return new Usuario
            {
              Nome = dto.Nome.Trim(),
              Email = dto.Email.Trim(),
              Cpf = dto.Cpf.Trim(),
              DataNascimento = dto.DataNascimento,
              Ativo = true,
              Enderecos = new List<Endereco>
              {
                  dto.Endereco.ToEnderecoFromBaseDto(principal: true)
              }
            };
        }

        public static Usuario ToUsuarioFromUpdateDto(this UpdateUsuarioDto dto)
        {
            return new Usuario
            {
              Nome = dto.Nome.Trim(),
              Email = dto.Email.Trim(),
              DataNascimento = dto.DataNascimento  
            };
        }
    }
}