using api.Dtos.Categoria;
using api.Models;

namespace api.Mappers
{
    public static class CategoriaMapper
    {
        public static CategoriaDto ToCategoriaDto(this Categoria categoria)
        {
            return new CategoriaDto
            {
                Id = categoria.Id,
                Genero = categoria.Genero
            };
        }

        public static Categoria ToCategoriaFromCreateDto(this CreateCategoriaDto dto)
        {
            return new Categoria
            {
              Genero = dto.Genero.Trim()
            };
        }

        public static Categoria ToCategoriaFromUpdate(this UpdateCategoriaDto dto)
        {
            return new Categoria
            {
                Genero = dto.Genero.Trim()
            };
        }
    }
}