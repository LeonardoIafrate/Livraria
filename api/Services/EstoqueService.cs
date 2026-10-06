using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Estoque;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;

namespace api.Services
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueRepository _estoqueRepo;
        
        public EstoqueService(IEstoqueRepository estoqueRepo)
        {
            _estoqueRepo = estoqueRepo;
        }

        public async Task<List<EstoqueDto>> GetAllAsync(string? nomeLivro)
        {
            if (string.IsNullOrWhiteSpace(nomeLivro))
            {
                var todas = await _estoqueRepo.GetAllAsync();
                return todas.Select(e => e.ToEstoqueDto()).ToList();
            }

            var livros = await _estoqueRepo.SearchByNomeLivroAsync(nomeLivro);
            if(!livros.Any())
                throw new NotFoundException($"Nenhum livro com o nome {nomeLivro} foi encontrado");

            return livros.Select(e => e.ToEstoqueDto()).ToList();
        }

        public async Task<EstoqueDto> GetByIdAsync(int id)
        {
            var estoque = await _estoqueRepo.GetByIdAsync(id);

            if(estoque == null)
                throw new NotFoundException("Nenhum estoque encontrado com este Id.");

            return estoque.ToEstoqueDto();
        }

        public async Task<EstoqueDto> GetByLivroId(int livroId)
        {
            var estoque = await _estoqueRepo.GetByLivroIdAsync(livroId);

            if (estoque == null)
                throw new NotFoundException("Nenhum estoque encontrado com o Id do livro informado.");

            return estoque.ToEstoqueDto();
        }

        public async Task<EstoqueDto> UpdateAsync(int id, UpdateEstoqueDto dto)
        {
            var estoqueModel = dto.ToEstoqueFromUpdate();

            var existente = _estoqueRepo.GetByIdAsync(id);

            if(existente == null)
                throw new NotFoundException("Nenhum livro encontrado com esse Id");
            
            var atualizado = await _estoqueRepo.UpdateAsync(id, estoqueModel);
            if(atualizado == null)
                throw new NotFoundException("Estoque não encontrado.");

            return atualizado.ToEstoqueDto();
        }
    }
}