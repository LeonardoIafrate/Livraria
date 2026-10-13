using api.Dtos.Auth;

namespace api.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(LoginDto dto);
    }
}