using System.Security.Claims;
using System.Text;
using api.Dtos.Auth;
using api.Interfaces;
using api.Models;
using api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly SymmetricSecurityKey _chave;

        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;

            if(string.IsNullOrWhiteSpace(_settings.Chave) || Encoding.UTF8.GetByteCount(_settings.Chave) < 32)
            {
                throw new InvalidOperationException(
                    "A Configuração Jwt:Chave está ausente ou tem menos de 32 bytes. " + "Defina com: dotnet user-secrets set \"Jwt:Chave\" \"<valor>\"");
            }

            _chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Chave));
        }
        public TokenDto GerarToken(Usuario usuario)
        {
            var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoEmMinutos);

            var descriptador = new SecurityTokenDescriptor
            {
              Subject = new ClaimsIdentity(new[]
              {
                  new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                  new Claim(ClaimTypes.Email, usuario.Email),
                  new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
              }),

              Expires = expiraEm,
              Issuer = _settings.Issuer,
              Audience = _settings.Audience,
              SigningCredentials = new SigningCredentials(_chave, SecurityAlgorithms.HmacSha256)  
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptador);

            return new TokenDto
            {
              Token = token,
              ExpiraEm = expiraEm  
            };
        }
    }
}