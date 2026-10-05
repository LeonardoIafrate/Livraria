using api.Dtos.Autor;
using api.Helpers;
using api.Models;

namespace api.Mappers
{
    public static class AutorMapper
    {
        public static AutorDto ToAutorDto(this Autor autor)
        {
            return new AutorDto
            {
                Id = autor.Id,
                Nome = autor.Nome,
                Biografia = autor.Biografia,
                Nacionalidade = autor.Nacionalidade
            };
        }

        public static Autor ToAutorFromCreateDto(this CreateAutorDto dto)
        {
            return new Autor
            {
                Nome = dto.Nome.Trim(),
                Biografia = dto.Biografia.FormatarOpcional(),
                Nacionalidade = dto.Nacionalidade.FormatarOpcional()
            };
        }

        public static Autor ToAutorFromUpdate(this UpdateAutorDto dto)
        {
            return new Autor
            {
                Nome = dto.Nome.Trim(),
                Biografia = dto.Biografia.FormatarOpcional(),
                Nacionalidade = dto.Nacionalidade.FormatarOpcional()
            };
        }
    }
}