using api.Dtos.Autor;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;

namespace api.Services
{
    public class AutorService : IAutorService
    {
        private readonly IAutorRepository _autorRepo;
        public AutorService(IAutorRepository autorRepo)
        {
            _autorRepo = autorRepo;
        }

        public async Task<List<AutorDto>> GetAllAsync(string? nome)
        {
            var autores = string.IsNullOrWhiteSpace(nome)
                ? await _autorRepo.GetAllAsync()
                : await _autorRepo.SearchByNomeAsync(nome);

            return autores.Select(a => a.ToAutorDto()).ToList();
        }

        public async Task<AutorDto> GetByIdAsync(int id)
        {
            var autor = await _autorRepo.GetByIdAsync(id);
            if(autor == null)
                throw new NotFoundException("Nenhum autor encontrado com esse Id");

            return autor.ToAutorDto();
        }

        public async Task<AutorDto> CreateAsync(CreateAutorDto dto)
        {
            var autorModel = dto.ToAutorFromCreateDto();

            var criado = await _autorRepo.CreateAsync(autorModel);
            return criado.ToAutorDto();
        }

        public async Task<AutorDto> UpdateAsync(int id, UpdateAutorDto dto)
        {
            var autorModel = dto.ToAutorFromUpdate();

            var autorExistente = await _autorRepo.GetByIdAsync(id);
            if(autorExistente == null)
                throw new NotFoundException("Nenhum autor foi encontrado com o Id informado.");

            var atualizado = await _autorRepo.UpdateAsync(id, autorModel);
            if(atualizado == null)
                throw new NotFoundException("Nenhum autor encontrado.");

            return atualizado.ToAutorDto();
        }

        public async Task DeleteAsync(int id)
        {
            var autorExistente = await _autorRepo.GetByIdAsync(id);
            if(autorExistente == null)
                throw new NotFoundException("Nenhum autor foi encontrado com o Id informado.");
            
            if(await _autorRepo.PossuiLivros(id))
                throw new ConflictException("Não é possível excluir um autor com livros vinculados.");

            await _autorRepo.DeleteAsync(id);
        }
    }
}