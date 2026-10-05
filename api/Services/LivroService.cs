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
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IAutorRepository _autorRepo;

        public LivroService(ILivroRepository livroRepo, IEditoraRepository editoraRepo, ICategoriaRepository categoriaRepo, IAutorRepository autorRepo)
        {
            _livroRepo = livroRepo;
            _editoraRepo = editoraRepo;
            _categoriaRepo = categoriaRepo;
            _autorRepo = autorRepo;
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

            livroModel.Categorias = await ObterCategoriasAsync(createDto.CategoriaIds);

            livroModel.Autores = await ObterAutoresAsync(createDto.AutoresIds);

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
            
            livroModel.Categorias = await ObterCategoriasAsync(updateDto.CategoriaIds);

            livroModel.Autores = await ObterAutoresAsync(updateDto.AutoresIds);

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

        public async Task<List<Categoria>> ObterCategoriasAsync(List<int> categoriaIds)
        {
            var ids = categoriaIds.Distinct().ToList();

            if(ids.Count == 0)
                return new List<Categoria>();

            var categorias = await _categoriaRepo.GetByIdsAsync(ids);

            if(categorias.Count != ids.Count)
            {
                var inexistentes = ids.Except(categorias.Select(c => c.Id));
                throw new BadRequestException($"Categorias inexistentes: {string.Join(", ", inexistentes)}.");
            }

            return categorias;
        }

        public async Task<List<Autor>> ObterAutoresAsync(List<int> autoresIds)
        {
            var ids = autoresIds.Distinct().ToList();

            if(ids.Count == 0)
                return new List<Autor>();

            var autores = await _autorRepo.GetByIdsAsync(ids);

            if(autores.Count != ids.Count)
            {
                var inexistentes = ids.Except(autores.Select(a => a.Id));
                throw new BadRequestException($"Autores inexistentes: {string.Join(", ", inexistentes)}");
            }

            return autores;
        }

    }
}