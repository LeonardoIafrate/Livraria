using api.Dtos.Estoque;
using api.Models;

namespace api.Mappers
{
    public static class EstoqueMapper
    {
        public static EstoqueDto ToEstoqueDto(this Estoque estoque)
        {
            return new EstoqueDto
            {
              Id = estoque.Id,
              Quantidade = estoque.Quantidade,
              DataAtualizacao = estoque.DataAtualizacao,
              LivroId = estoque.LivroId,
              NomeLivro = estoque.Livro.Nome?? string.Empty  
            };
        }

        public static Estoque ToEstoqueFromUpdateDto(this UpdateEstoqueDto dto)
        {
            return new Estoque
            {
                Quantidade = dto.Quantidade
            };
        }
    }
}