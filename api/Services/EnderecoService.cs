using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<EnderecoDto> GetByIdAsync(int id)
        {
            var endereco = await ObterEnderecoAtivo(id);
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

        public async Task<EnderecoDto> UpdateAsync(int id, UpdateEnderecoDto dto)
        {
            await ObterEnderecoAtivo(id);

            var enderecoModel = dto.ToEnderecoFromBaseDto();

            var atualizado = await _enderecoRepo.UpdateAsync(id, enderecoModel);
            if(atualizado == null)
                throw new NotFoundException("Nenhum endereço encontrado com esse Id.");

            return atualizado.ToEnderecoDto();
        }

        public async Task<EnderecoDto> DefinirPrincipalAsync(int id)
        {
            var existente = await ObterEnderecoAtivo(id);
            if(existente.Principal)
                return existente.ToEnderecoDto();

            var atualizado = await _enderecoRepo.DefinirPrincipalAsync(id);
            if(atualizado == null)
                throw new NotFoundException("Nenhum endereço encontrado com esse Id.");

            return atualizado.ToEnderecoDto();
        }

        public async Task DesativarAsync(int id)
        {
            var existente = await ObterEnderecoAtivo(id);

            var ativos = await _enderecoRepo.GetByUsuarioIdAsync(existente.UsuarioId);
            if(ativos.Count <= 1)
                throw new ConflictException("Não é possível desativar o único endereço ativo. Cadastre outro antes.");
            
            var eraPrincipal = existente.Principal;

            await _enderecoRepo.DesativaAsync(id);

            if (eraPrincipal)
            {
                var proximo = ativos.First(e => e.Id != id);
                await _enderecoRepo.DefinirPrincipalAsync(proximo.Id);
            }
        }

        private async Task<Endereco> ObterEnderecoAtivo(int id)
        {
            var endereco = await _enderecoRepo.GetByIdAsync(id);
            if(endereco == null || !endereco.Ativo)
                throw new NotFoundException("Nenhum endereço encontrado com esse Id.");

            return endereco;
        }
    }
}