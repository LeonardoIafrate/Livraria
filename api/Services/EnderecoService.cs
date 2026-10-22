using api.Dtos.Endereco;
using api.Exceptions;
using api.Interfaces;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class EnderecoService : IEnderecoService
    {
        private readonly IEnderecoRepository _enderecoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        public EnderecoService(IEnderecoRepository enderecoRepository, IUsuarioRepository usuarioRepo)
        {
            _enderecoRepo = enderecoRepository;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<EnderecoDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            if(!await _usuarioRepo.ExistsAsync(usuarioId))
                throw new NotFoundException("Nenhum usuário encontrado com esse Id.");
            
            var enderecos = await _enderecoRepo.GetByUsuarioIdAsync(usuarioId);
            return enderecos.Select(e => e.ToEnderecoDto()).ToList();
        }

        public async Task<EnderecoDto> GetByIdAsync(int id, int usuarioId)
        {
            var endereco = await ObterEnderecoAtivo(id, usuarioId);
            return endereco.ToEnderecoDto();
        }

        public async Task<EnderecoDto> CreateAsync(int usuarioId, CreateEnderecoDto dto)
        {
            if(!await _usuarioRepo.ExistsAsync(usuarioId))
                throw new NotFoundException("Nenhum usuário encontrado com esse Id.");

            var enderecoModel = dto.ToEnderecoFromBaseDto();
            enderecoModel.UsuarioId = usuarioId;

            var primeiro = !await _enderecoRepo.PossuiEnderecoAtivo(usuarioId);
            enderecoModel.Principal = primeiro;

            var criado = await _enderecoRepo.CreateAsync(enderecoModel);

            if(!primeiro && dto.Principal)
                criado = await _enderecoRepo.DefinirPrincipalAsync(criado.Id) ?? criado;
            return criado.ToEnderecoDto();
        }

        public async Task<EnderecoDto> UpdateAsync(int id, int usuarioId, UpdateEnderecoDto dto)
        {
            await ObterEnderecoAtivo(id, usuarioId);

            var enderecoModel = dto.ToEnderecoFromBaseDto();

            var atualizado = await _enderecoRepo.UpdateAsync(id, enderecoModel);
            if(atualizado == null)
                throw new NotFoundException("Nenhum endereço encontrado com esse Id.");

            return atualizado.ToEnderecoDto();
        }

        public async Task<EnderecoDto> DefinirPrincipalAsync(int id, int usuarioId)
        {
            var existente = await ObterEnderecoAtivo(id, usuarioId);
            if(existente.Principal)
                return existente.ToEnderecoDto();

            var atualizado = await _enderecoRepo.DefinirPrincipalAsync(id);
            if(atualizado == null)
                throw new NotFoundException("Nenhum endereço encontrado com esse Id.");

            return atualizado.ToEnderecoDto();
        }

        public async Task ExcluirAsync(int id, int usuarioId)
        {
            var existente = await ObterEnderecoAtivo(id, usuarioId);
            
            var eraPrincipal = existente.Principal;


            await _enderecoRepo.DeleteAsync(id);

            if (eraPrincipal)
            {
                var restantes = await _enderecoRepo.GetByUsuarioIdAsync(usuarioId);
                var proximo = restantes.FirstOrDefault();

                if(proximo != null)
                    await _enderecoRepo.DefinirPrincipalAsync(proximo.Id);
            }
        }

        private async Task<Endereco> ObterEnderecoAtivo(int id, int usuarioId)
        {
            var endereco = await _enderecoRepo.GetByIdAsync(id);
            if(endereco == null || !endereco.Ativo)
                throw new NotFoundException("Nenhum endereço encontrado com esse Id.");

            return endereco;
        }
    }
}