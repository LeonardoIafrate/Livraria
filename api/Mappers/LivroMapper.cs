using api.Dtos.Livro;
using api.Models;
using api.Helpers;

namespace api.Mappers
{
    public static class LivroMapper
    {
        public static LivroDto ToLivroDto(this Livro livro)
        {
            return new LivroDto
            {
                Id = livro.Id,
                Nome = livro.Nome,
                Sinopse = livro.Sinopse,
                AnoLancamento = livro.AnoLancamento,
                Isbn = livro.Isbn,
                Preco = livro.Preco,
                NumeroPaginas = livro.NumeroPaginas,
                Idioma = livro.Idioma,
                DataCadastro = livro.DataCadastro,
                EditoraId = livro.EditoraId,
                EditoraNome = livro.Editora?.Nome?? string.Empty,
                Generos = livro.Categorias
                    .Select(c => c.Genero)
                    .OrderBy(g => g)
                    .ToList()
            };
        }

        public static Livro ToLivroFromCreateDto(this CreateLivroDto dto)
        {
            return new Livro
            {
                Nome = dto.Nome.Trim(),
                Sinopse = dto.Sinopse.FormatarOpcional(),
                AnoLancamento = dto.AnoLancamento,
                Isbn = dto.Isbn.FormatarIsbn(),
                Preco = dto.Preco,
                NumeroPaginas = dto.NumeroPaginas,
                Idioma = dto.Idioma.FormatarOpcional(),
                EditoraId = dto.EditoraId
            };
        }

        public static Livro ToLivroFromUpdate(this UpdateLivroDto dto)
        {
            return new Livro
            {
                Nome = dto.Nome.Trim(),
                Sinopse = dto.Sinopse.FormatarOpcional(),
                AnoLancamento = dto.AnoLancamento,
                Isbn = dto.Isbn.FormatarIsbn(),
                Preco = dto.Preco,
                NumeroPaginas = dto.NumeroPaginas,
                Idioma = dto.Idioma.FormatarOpcional(),
                EditoraId = dto.EditoraId
            };
        }
        
    }
}