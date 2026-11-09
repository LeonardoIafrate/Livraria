using api.Dtos.Auth;
using api.Exceptions;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private const string CredenciaisInvalidas = "E-mail ou senha inválidos";

        private static readonly string HashFalso = 
            new PasswordHasher<Usuario>().HashPassword(new Usuario(), Guid.NewGuid().ToString());

        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        private const int MaxTentativas = 5;
        private static readonly TimeSpan DuracaoBloqueio = TimeSpan.FromMinutes(15);

        private readonly TimeProvider _tempo;

        public AuthService(
            IUsuarioRepository usuarioRepo, 
            ITokenService tokenService, 
            IPasswordHasher<Usuario> passwordHasher,
            TimeProvider tempo
            )
        {
            _usuarioRepo = usuarioRepo;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _tempo = tempo;
        }
        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var usuario = await _usuarioRepo.GetByEmailAsync(email);

            if(usuario == null || !usuario.Ativo)
            {
                _passwordHasher.VerifyHashedPassword(new Usuario(), HashFalso , dto.Senha);
                throw new UnauthorizedException(CredenciaisInvalidas);
            }

            var agora = _tempo.GetUtcNow().UtcDateTime;

            if(usuario.EstaBloqueado(agora))
                throw new UnauthorizedException(CredenciaisInvalidas);

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.Senha);

            if(resultado == PasswordVerificationResult.Failed)
            {
                usuario.RegistrarFalhasDeSenha(agora, PoliticaDeLogin.MaxTentativas, PoliticaDeLogin.DuracaoBloqueio);
                await _usuarioRepo.AtualizarBloqueioAsync(usuario.Id, usuario.TentativasFalhas, usuario.BloqueadoAte);
                throw new UnauthorizedException(CredenciaisInvalidas);
            }

            if (usuario.TentativasFalhas > 0 || usuario.BloqueadoAte != null)
            {
                usuario.LimparFalhas();
                await _usuarioRepo.AtualizarBloqueioAsync(usuario.Id, usuario.TentativasFalhas, usuario.BloqueadoAte);
            }

            return _tokenService.GerarToken(usuario);
        }
    }
}