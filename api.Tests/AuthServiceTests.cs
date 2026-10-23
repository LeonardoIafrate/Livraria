using api.Dtos.Auth;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class AuthServiceTests
{
    private const string SenhaCorreta = "Senha123";

    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly PasswordHasher<Usuario> _hasher = new();

    private AuthService CriarServico() => new(_usuarioRepo, _tokenService, _hasher);

    private Usuario CriarUsuario(bool ativo = true)
    {
        var usuario = new Usuario { Id = 1, Email = "ana@x.com", Ativo = ativo };
        usuario.SenhaHash = _hasher.HashPassword(usuario, SenhaCorreta);
        return usuario;
    }

    [Fact]
    public async Task Login_EmailInexistente_LancaUnauthorized()
    {
        _usuarioRepo.GetByEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CriarServico().LoginAsync(new LoginDto { Email = "nao@existe.com", Senha = SenhaCorreta }));
    }

    [Fact]
    public async Task Login_SenhaErrada_LancaUnauthorized()
    {
        _usuarioRepo.GetByEmailAsync("ana@x.com").Returns(CriarUsuario());

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CriarServico().LoginAsync(new LoginDto { Email = "ana@x.com", Senha = "outra-senha" }));
    }

    [Fact]
    public async Task Login_ContaDesativada_LancaUnauthorized()
    {
        _usuarioRepo.GetByEmailAsync("ana@x.com").Returns(CriarUsuario(ativo: false));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CriarServico().LoginAsync(new LoginDto { Email = "ana@x.com", Senha = SenhaCorreta }));
    }

    [Fact]
    public async Task Login_CredenciaisCorretas_DevolveToken()
    {
        var usuario = CriarUsuario();
        _usuarioRepo.GetByEmailAsync("ana@x.com").Returns(usuario);
        _tokenService.GerarToken(usuario).Returns(new TokenDto { Token = "token-de-teste" });

        var resultado = await CriarServico().LoginAsync(
            new LoginDto { Email = "  ANA@x.com ", Senha = SenhaCorreta });

        Assert.Equal("token-de-teste", resultado.Token);
    }
}