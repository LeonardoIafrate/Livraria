using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Livro;
using api.Exceptions;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class LivroService : ILivroService
    {
        private readonly ILivroRepository _livroRepo;
        private readonly IEditoraRepository _editoraRepo;

        public LivroService(ILivroRepository livroRepo, IEditoraRepository editoraRepo)
        {
            _livroRepo = livroRepo;
            _editoraRepo = editoraRepo;
        }

        public async Task<PagedResult<LivroDto>> GetAllAsync(LivroQueryObject query)
        {
            var livros = await _livroRepo.GetAllAsync(query);
            return livros.Map(l => l.ToLivroDto());
        }

        public async Task<LivroDto> GetByIdAsync(int id)
        {
            var livro = await _livroRepo.GetByIdAsync(id);
            
            if(livro == null)
                throw new NotFoundException("Livro não encontrado.");

            return livro.ToLivroDto();
        }

        public async Task<LivroDto> GetByIsbnAsync(string isbn)
        {
            var isbnFormatado = isbn.Replace("-", "").FormatarIsbn();

            var livro = await _livroRepo.GetByIsbnAsync(isbnFormatado);

            if(livro == null)
                throw new NotFoundException("Livro não encontrado");

            return livro.ToLivroDto();
        }

        public async Task<LivroDto> CreateAsync(CreateLivroDto createDto)
        {
            var livroModel = createDto.ToLivroFromCreateDto();

            if(!await _editoraRepo.ExistsAsync(livroModel.EditoraId))
                throw new NotFoundException("A editora informada não existe.");

            var isbnEmUso = await _livroRepo.GetByIsbnAsync(livroModel.Isbn);
            if(isbnEmUso != null)
                throw new ConflictException("Já existe outro livro cadastrado com esse ISBN.");

            var criado = await _livroRepo.CreateAsync(livroModel);
            return criado.ToLivroDto();
        }

        public async Task<LivroDto> UpdateAsync(int id, UpdateLivroDto updateDto)
        {
            var livroModel = updateDto.ToLivroFromUpdate();

            var existente = await _livroRepo.GetByIdAsync(id);
            if(existente == null)
                throw new NotFoundException("Livro não encontrado.");

            if(!await _editoraRepo.ExistsAsync(livroModel.EditoraId))
                throw new NotFoundException("A editora informada não foi encontrada.");
            
            var isbnEmUso = await _livroRepo.GetByIsbnAsync(livroModel.Isbn);
            if(isbnEmUso != null && isbnEmUso.Id != id)
                throw new ConflictException("Já existe outro livro cadastrado com esse ISBN.");
            
            var atualizado = await _livroRepo.UpdateAsync(id, livroModel);
            
            if(atualizado == null)
                throw new NotFoundException("Livro não encontrado.");

            return atualizado.ToLivroDto();
        }

        public async Task DeleteAsync(int id)
        {
            var existente = await _livroRepo.GetByIdAsync(id);
            
            if(existente == null)
                throw new NotFoundException("Livro não encontrado.");
            
            await _livroRepo.DeleteAsync(id);
        }

    }
}