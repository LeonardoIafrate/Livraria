using api.Dtos.Auth;
using api.Models;

namespace api.Interfaces
{
    public interface ITokenService
    {
        TokenDto GerarToken(Usuario usuario);
    }
}