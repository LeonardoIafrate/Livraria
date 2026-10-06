using api.Dtos.Editora;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;

namespace api.Services
{
    public class EditoraService : IEditoraService
    {
        private readonly IEditoraRepository _editoraRepo;

        public EditoraService(IEditoraRepository editoraRepo)
        {
            _editoraRepo = editoraRepo;
        }

        public async Task<List<EditoraDto>> GetAllAsync(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                var todas = await _editoraRepo.GetAllAsync();
                return todas.Select(e => e.ToEditoraDto()).ToList();
            }

            var editoras = await _editoraRepo.SearchByNameAsync(nome);
            if(!editoras.Any())
                throw new NotFoundException($"Nenhuma editora encontrada com o nome '{nome}'.");

            return editoras.Select(e => e.ToEditoraDto()).ToList();
        }

        public async Task<EditoraDto> GetByIdAsync(int id)
        {
            var editora = await _editoraRepo.GetByIdAsync(id);

            if(editora == null)
                throw new NotFoundException("Editora não encontrada");

            return editora.ToEditoraDto();
        }

        public async Task<EditoraDto> CreateAsync(CreateEditoraDto createDto)
        {
            var editoraModel = createDto.ToEditoraFromCreateDto();

            var nomeEmUso = await _editoraRepo.GetByNameAsync(editoraModel.Nome);
            if (nomeEmUso != null)
                throw new ConflictException("Já existe uma editora com esse nome");
            
            var criada = await _editoraRepo.CreateAsync(editoraModel);
            return criada.ToEditoraDto();
        }

        public async Task<EditoraDto> UpdateAsync(UpdateEditoraDto updateDto, int id)
        {
            var editoraModel = updateDto.ToEditoraFromUpdateDto();

            var existente = await _editoraRepo.GetByIdAsync(id);
            if(existente == null)
                throw new NotFoundException("Editora não encontrada.");
            
            var nomeEmUso = await _editoraRepo.GetByNameAsync(editoraModel.Nome);
            if (nomeEmUso != null)
                throw new ConflictException("Já existe uma editora com esse nome.");
            
            var atualizada = await _editoraRepo.UpdateAsync(id, editoraModel);
            if (atualizada == null)
                throw new NotFoundException("Editora não encontrada.");
            
            return atualizada.ToEditoraDto();
        }

        public async Task DeleteAsync(int id)
        {
            var existente = await _editoraRepo.GetByIdAsync(id);

            if (existente == null)
                throw new NotFoundException("Editora não encontrada");

            if (await _editoraRepo.PossuiLivrosAsync(id))
                throw new ConflictException("Não é possível excluir uma editora com livros ainda cadastrados.");

            await _editoraRepo.DeleteAsync(id);
        }
    }
}