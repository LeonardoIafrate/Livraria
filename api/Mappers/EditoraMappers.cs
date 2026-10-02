using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Editora;
using api.Models;

namespace api.Mappers
{
    public static class EditoraMappers
    {
        public static EditoraDto ToEditoraDto(this Editora editora)
        {
            return new EditoraDto
            {
                Id = editora.Id,
                Nome = editora.Nome
            };
        }

        public static Editora ToEditoraFromCreateDto(this CreateEditoraDto dto)
        {
            return new Editora
            {
                Nome = dto.Nome.Trim()
            };
        }

        public static Editora ToEditoraFromUpdateDto(this UpdateEditoraDto dto)
        {
            return new Editora
            {
                Nome = dto.Nome.Trim()
            };
        }
    }
}