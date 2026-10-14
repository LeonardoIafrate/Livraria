using System.Security.Claims;
using api.Models;

namespace api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUsuarioByID(this ClaimsPrincipal user)
        {
            var valor = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if(!int.TryParse(valor, out var id))
                throw new UnauthorizedAccessException("Token inválido");

            return id;
        }

        public static bool EhAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole(nameof(PerfilUsuario.Admin));
        }
    }
}