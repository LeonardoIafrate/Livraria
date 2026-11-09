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
        private readonly TimeProvider _tempo;

        public UsuarioService(IUsuarioRepository usuarioRepo, IPasswordHasher<Usuario> passwordHasher, TimeProvider tempo)
        {
            _usuarioRepo = usuarioRepo;
            _passwordHasher = passwordHasher;
            _tempo = tempo;
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

        public Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto) => CriarAsync(dto, PerfilUsuario.Cliente);

        public Task<UsuarioDto> CreateAdminAsync(CreateUsuarioDto dto) => CriarAsync(dto, PerfilUsuario.Admin);

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

        public async Task<UsuarioDto> AlterarAdminAsync(int id)
        {
            var usuarioModel = await _usuarioRepo.GetByIdAsync(id);
            if(usuarioModel == null)
                throw new NotFoundException("Nenhum usuário encontrado com o Id informado.");

            if(usuarioModel.Ativo == false)
                throw new ConflictException("O usuário está inativo");

            await _usuarioRepo.AlterarUsuarioAdminAsync(usuarioModel.Id);
            return usuarioModel.ToUsuarioDto();
        }

        public async Task AlterarSenhaAsync(int id, AlterarSenhaDto dto)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);

            if(usuario == null || !usuario.Ativo)
                throw new NotFoundException("Nenhum usuário encontrado.");

            var agora = _tempo.GetUtcNow().UtcDateTime;

            if(usuario.EstaBloqueado(agora))
                throw new BadRequestException("Muitas tentativas incorretas. Tente novamente mais tarde.");

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.SenhaAtual);

            if(resultado == PasswordVerificationResult.Failed)
            {
                usuario.RegistrarFalhasDeSenha(agora, PoliticaDeLogin.MaxTentativas, PoliticaDeLogin.DuracaoBloqueio);
                throw new BadRequestException("Senha atual incorreta.");
            }


            if (usuario.TentativasFalhas > 0 || usuario.BloqueadoAte != null)
            {
                usuario.LimparFalhas();
                await _usuarioRepo.AtualizarBloqueioAsync(usuario.Id, usuario.TentativasFalhas, usuario.BloqueadoAte);
            }

            if(dto.NovaSenha == dto.SenhaAtual)
                throw new BadRequestException("A nova senha deve ser diferente da atual");

            var novoHash = _passwordHasher.HashPassword(usuario, dto.NovaSenha);
            await _usuarioRepo.UpdateSenhaAsync(usuario.Id, novoHash);
        }
        
        public async Task DeleteAsync(int id)
        {
            var existente = await _usuarioRepo.GetByIdAsync(id);
            
            if(existente == null)
                throw new NotFoundException("Nenhum usuário encontrado com esse Id.");

            if(await _usuarioRepo.PossuiPedidosAsync(id))
                throw new ConflictException("Um usuário que possuí pedidos não pode ser excluído.");

            await _usuarioRepo.DeleteAsync(id);
        }

        public async Task DesativarAsync(int id)
        {
            var desativado = await _usuarioRepo.DesativarAsync(id);
            if(desativado == null)
                throw new NotFoundException("Nenhum usuário encontrado com esse Id.");
        }

        private async Task<UsuarioDto> CriarAsync(CreateUsuarioDto dto, PerfilUsuario perfil)
        {
            var usuarioModel = dto.ToUsuarioFromCreateDto();

            if(await _usuarioRepo.GetByCpfAsync(usuarioModel.Cpf) != null)
                throw new ConflictException("O CPF informado já está em uso.");

            if(await _usuarioRepo.GetByEmailAsync(usuarioModel.Email) != null)
                throw new ConflictException("O E-mail informado já está em uso.");
                
            usuarioModel.Perfil = perfil;
            usuarioModel.SenhaHash = _passwordHasher.HashPassword(usuarioModel, dto.Senha);

            var criado = await _usuarioRepo.CreateAsync(usuarioModel);

            return criado.ToUsuarioDto();
        }
    }
}