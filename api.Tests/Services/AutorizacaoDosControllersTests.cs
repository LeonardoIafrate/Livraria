using System.Reflection;
using api.Controllers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace api.Tests.Services
{
    public class AutorizacaoDosControllersTests
    {
        private static readonly Type[] ControllersDoCatalogo =
        {
          typeof(EditoraController),  
          typeof(AutorController),  
          typeof(CategoriaController),  
          typeof(LivroController),  
          typeof(EstoqueController)  
        };

        private static readonly string[] EscritasPublicasPermitidas =
        {
          "UsuarioController.Create",
          "AuthController.Login"  
        };

        private record Acao(Type Controller, MethodInfo Metodo)
        {
            public string Nome => $"{Controller.Name}.{Metodo.Name}";

            public IEnumerable<string> VerboHttp => Metodo.GetCustomAttributes<HttpMethodAttribute>().SelectMany(a => a.HttpMethods);

            public bool EhEscrita => VerboHttp.Any(v => v != "GET");

            public bool TemAtributo<T>() where T : Attribute => 
                Metodo.GetCustomAttributes<T>().Any() || Controller.GetCustomAttributes<T>().Any();

            public bool ExigeAdmin()
            {
                if(Metodo.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                    return false;

                return Metodo.GetCustomAttributes<AuthorizeAttribute>()
                    .Concat(Controller.GetCustomAttributes<AuthorizeAttribute>())
                    .Any(a => a.Roles != null && a.Roles.Split(',').Select(r => r.Trim()).Contains(nameof(PerfilUsuario.Admin)));
            }
        }

        private static List<Acao> ObterAcoes()
        {
            return typeof(UsuarioController).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(t => t
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any())
                    .Select(m => new Acao(t, m)))
                .ToList();
        }

        [Fact]
        public void SoCadastroELoginSaoEscritasPublicas()
        {
            var escritasPublicas = ObterAcoes()
                .Where(a => a.EhEscrita && a.TemAtributo<AllowAnonymousAttribute>())
                .Select(a => a.Nome)
                .Except(EscritasPublicasPermitidas)
                .ToList();

            Assert.True(
                escritasPublicas.Count == 0,
                "Ações de escrita abertas a qualquer pessoa: " + string.Join(", ", escritasPublicas)
            );
        }

        [Fact]
        public void EscritaNoCatalogoExigeAdministrador()
        {
            var semAdmin = ObterAcoes()
                .Where(a => ControllersDoCatalogo.Contains(a.Controller))
                .Where(a => a.EhEscrita && !a.ExigeAdmin())
                .Select(a => a.Nome)
                .ToList();
    
            Assert.True(
                semAdmin.Count == 0,
                "Escritas do catálogo sem [Authorize(Roles = Admin)]: " + string.Join(", ", semAdmin));
        }
    }
}