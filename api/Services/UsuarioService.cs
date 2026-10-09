using api.Dtos.Usuario;
using api.Exceptions;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        public UsuarioService(IUsuarioRepository usuarioRepo, IPasswordHasher<Usuario> passwordHasher)
        {
            _usuarioRepo = usuarioRepo;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UsuarioDto>> GetAllAsync(string? nome)
        {
            var usuarios = string.IsNullOrWhiteSpace(nome) 
                ? await _usuarioRepo.GetAllAsync() 
                : await _usuarioRepo.SearchByNomeAsync(nome);

            return usuarios.Select(u => u.ToUsuarioDto()).ToList();
        }

        public async Task<UsuarioDto> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);
            if(usuario == null)
                throw new NotFoundException("Nenhum usuário encontrado.");

            return usuario.ToUsuarioDto();
        }

        public async Task<UsuarioDto> GetByCpfAsync(string cpf)
        {
            var usuario = await _usuarioRepo.GetByCpfAsync(CpfHellper.SomenteDigitos(cpf));
            if(usuario == null)
                throw new NotFoundException("Nenhum usuário encontrado");
            
            return usuario.ToUsuarioDto();
        }

        public async Task<UsuarioDto> GetByEmailAsync(string email)
        {
            var usuario = await _usuarioRepo.GetByEmailAsync(email.Trim().ToLowerInvariant());
            if(usuario == null)
                throw new NotFoundException("Nenhum usuário encontrado");

            return usuario.ToUsuarioDto();
        }

        public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto)
        {
            var usuarioModel = dto.ToUsuarioFromCreateDto();

            var cpfEmUso = await _usuarioRepo.GetByCpfAsync(usuarioModel.Cpf);
            if(cpfEmUso != null)
                throw new ConflictException("O CPF informado já está em uso.");

            var emailEmUso = await _usuarioRepo.GetByEmailAsync(usuarioModel.Email);
            if(emailEmUso != null)
                throw new ConflictException("O E-mail informado já está em uso.");

            usuarioModel.SenhaHash = _passwordHasher.HashPassword(usuarioModel, dto.Senha);

            var endereco = dto.Endereco.ToEnderecoFromBaseDto();
            endereco.Principal = true;
            endereco.Ativo = true;
            usuarioModel.Enderecos.Add(endereco);

            var criado = await _usuarioRepo.CreateAsync(usuarioModel);
            return criado.ToUsuarioDto();
        }

        public async Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto)
        {
            var usuarioModel = dto.ToUsuarioFromUpdateDto();

            var existente = await _usuarioRepo.GetByIdAsync(id);
            if(existente == null)
                throw new NotFoundException("Nenhum usuário foi encontrado com esse id.");

            var emailEmUso = await _usuarioRepo.GetByEmailAsync(usuarioModel.Email);
            if(emailEmUso != null && emailEmUso.Id != id)
                throw new ConflictException("Esse E-mail já está em uso.");

            var atualizado = await _usuarioRepo.UpdateAsync(id, usuarioModel);
            if(atualizado == null)
                throw new NotFoundException("Usuário não encontrado.");
            return atualizado.ToUsuarioDto();
        }
        
        public async Task DeleteAsync(int id)
        {
            var existente = await _usuarioRepo.GetByIdAsync(id);
            if(existente == null)
                throw new NotFoundException("Nenhum usuário encontrado com esse Id.");

            await _usuarioRepo.DeleteAsync(id);
        }

        public async Task DesativarAsync(int id)
        {
            var desativado = await _usuarioRepo.DesativarAsync(id);
            if(desativado == null)
                throw new NotFoundException("Nenhum usuário encontrado com esse Id.");
        }
    }
}