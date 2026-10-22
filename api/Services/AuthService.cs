using api.Dtos.Auth;
using api.Exceptions;
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

        public AuthService(IUsuarioRepository usuarioRepo, ITokenService tokenService, IPasswordHasher<Usuario> passwordHasher)
        {
            _usuarioRepo = usuarioRepo;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }
        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var usuario = await _usuarioRepo.GetByEmailAsync(email);

            if(usuario == null)
            {
                _passwordHasher.VerifyHashedPassword(new Usuario(), HashFalso , dto.Senha);
                throw new UnauthorizedException(CredenciaisInvalidas);
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.Senha);

            if(resultado == PasswordVerificationResult.Failed || !usuario.Ativo)
                throw new UnauthorizedException(CredenciaisInvalidas);

            return _tokenService.GerarToken(usuario);
        }
    }
}