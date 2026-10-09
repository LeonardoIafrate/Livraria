using api.Dtos.Categoria;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;

namespace api.Services
{
    public class CategoriaService : ICategoriaService
    {

        private readonly ICategoriaRepository _categoriaRepo;

        public CategoriaService(ICategoriaRepository categoriaRepo)
        {
            _categoriaRepo = categoriaRepo;
        }

        public async Task<List<CategoriaDto>> GetAllAsync(string? genero)
        {
            var categorias = string.IsNullOrWhiteSpace(genero)
                ? await _categoriaRepo.GetAllAsync()
                : await _categoriaRepo.SearchByGeneroAsync(genero);

            return categorias.Select(c => c.ToCategoriaDto()).ToList();
        }

        public async Task<CategoriaDto> GetByIdAsync(int id)
        {
            var categoria = await _categoriaRepo.GetByIdAsync(id);

            if(categoria == null)
                throw new NotFoundException("Não existe nenhuma categoria com o id informado");

            return categoria.ToCategoriaDto();
        }

        public async Task<CategoriaDto> CreateAsync(CreateCategoriaDto createDto)
        {
            var categoriaModel = createDto.ToCategoriaFromCreateDto();

            var categoriaEmUso = await _categoriaRepo.GetByGeneroAsync(categoriaModel.Genero);
            if(categoriaEmUso != null)
                throw new ConflictException($"O Gênero {categoriaEmUso.Genero} já existe.");

            var criada = await _categoriaRepo.CreateAsync(categoriaModel);
            return criada.ToCategoriaDto();
        }

        public async Task<CategoriaDto> UpdateAsync(int id, UpdateCategoriaDto updateDto)
        {
           var categoriaModel = updateDto.ToCategoriaFromUpdate();

            var categoriaExistente = _categoriaRepo.GetByIdAsync(id);
            if(categoriaExistente == null)
                throw new NotFoundException("Não existe uma categoria com este Id");
            
            var categoriaEmUso = await _categoriaRepo.GetByGeneroAsync(categoriaModel.Genero);
            if(categoriaEmUso != null)
                throw new ConflictException($"O Gênero {categoriaEmUso.Genero} já existe.");

            var atualizada = await _categoriaRepo.UpdateAsync(id, categoriaModel);
            if(atualizada == null)
                throw new NotFoundException("Categoria não encontrada");
            
            return atualizada.ToCategoriaDto();
        }

        public async Task DeleteAsync(int id)
        {

            var categoriaExistente = await _categoriaRepo.GetByIdAsync(id);
            if(categoriaExistente == null)
                throw new NotFoundException("Categoria não encontrada");


            if (await _categoriaRepo.PossuiLivroAsync(id))
                throw new ConflictException("Não é possível excluir um gênero que possuí livros vinculados.");
            
            await _categoriaRepo.DeleteAsync(id);
        }
    }
}