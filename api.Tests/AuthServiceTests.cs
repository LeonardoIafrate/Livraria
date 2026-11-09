using api.Dtos.Auth;
using api.Exceptions;
using api.Helpers;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class AuthServiceTests
{
    private const string Email = "ana@exemplo.com";
    private const string SenhaCorreta = "Senha123";
    private const string SenhaErrada = "Senha12345";
    private const string TokenDeTeste = "token-de-teste";

    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly PasswordHasher<Usuario> _hasher = new();
    private readonly FakeTimeProvider _tempo = new(new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero));

    private AuthService CriarServico() => new(_usuarioRepo, _tokenService, _hasher, _tempo);

    private Usuario CriarUsuario(bool ativo = true)
    {
        var usuario = new Usuario { Id = 1, Email = Email, Ativo = ativo };
        usuario.SenhaHash = _hasher.HashPassword(usuario, SenhaCorreta);
        return usuario;
    }

    private Usuario PrepararUsuario(bool ativo = true)
    {
        var usuario = CriarUsuario(ativo);
        _usuarioRepo.GetByEmailAsync(Email).Returns(usuario);
        _tokenService.GerarToken(usuario).Returns(new TokenDto { Token = TokenDeTeste });
        return usuario;
    }

    private Task<TokenDto> Login(string senha) =>
        CriarServico().LoginAsync(new LoginDto { Email = Email, Senha = senha });

    private async Task FalharVezes(Usuario usuario, int vezes)
    {
        for (var i = 0; i < vezes; i++)
        {
            var antes = usuario.TentativasFalhas;

            await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaErrada));

            Assert.True(usuario.TentativasFalhas != antes || usuario.BloqueadoAte != null,
                "a falha de senha não foi registrada: o usuário foi encontrado?");
        }
    }

    private async Task AssertGravacoesAsync(int quantidade) =>
        await _usuarioRepo.Received(quantidade)
            .AtualizarBloqueioAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime?>());

    [Fact]
    public async Task Login_EmailInexistente_LancaUnauthorizedENaoGrava()
    {
        _usuarioRepo.GetByEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CriarServico().LoginAsync(new LoginDto { Email = "nao@existe.com", Senha = SenhaCorreta }));

        await AssertGravacoesAsync(0);
    }

    [Fact]
    public async Task Login_ContaDesativada_LancaUnauthorizedENaoGrava()
    {
        PrepararUsuario(ativo: false);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaCorreta));

        await AssertGravacoesAsync(0);
        _tokenService.DidNotReceive().GerarToken(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Login_SenhaErrada_IncrementaContadorEGrava()
    {
        var usuario = PrepararUsuario();

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaErrada));

        Assert.Equal(1, usuario.TentativasFalhas);
        Assert.Null(usuario.BloqueadoAte);
        await _usuarioRepo.Received(1).AtualizarBloqueioAsync(usuario.Id, 1, null);
    }

    [Fact]
    public async Task Login_UmaFalhaAntesDoLimite_AindaNaoBloqueia()
    {
        var usuario = PrepararUsuario();

        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas - 1);

        Assert.Null(usuario.BloqueadoAte);
        Assert.Equal(PoliticaDeLogin.MaxTentativas - 1, usuario.TentativasFalhas);

        var resultado = await Login(SenhaCorreta);
        Assert.Equal(TokenDeTeste, resultado.Token);
    }

    [Fact]
    public async Task Login_UltimaFalhaDoLimite_BloqueiaPelaDuracaoDaPolitica()
    {
        var usuario = PrepararUsuario();

        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas);

        var esperado = _tempo.GetUtcNow().UtcDateTime.Add(PoliticaDeLogin.DuracaoBloqueio);
        Assert.Equal(esperado, usuario.BloqueadoAte);
        Assert.Equal(0, usuario.TentativasFalhas);

        await AssertGravacoesAsync(PoliticaDeLogin.MaxTentativas);
        await _usuarioRepo.Received(1).AtualizarBloqueioAsync(usuario.Id, 0, esperado);
    }

    [Fact]
    public async Task Login_ContaBloqueada_RejeitaMesmoComSenhaCorreta()
    {
        var usuario = PrepararUsuario();

        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas);
        Assert.NotNull(usuario.BloqueadoAte);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaCorreta));

        _tokenService.DidNotReceive().GerarToken(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Login_ContaBloqueada_NaoRevelaMensagem()
    {
        var usuario = PrepararUsuario();
        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas);

        var comSenhaCorreta = await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaCorreta));
        var comSenhaErrada = await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaErrada));

        Assert.Equal(comSenhaCorreta.Message, comSenhaErrada.Message);
    }

    [Fact]
    public async Task Login_SenhaErradaEnquantoBloqueado_NaoAdicionaTempoNemGrava()
    {
        var usuario = PrepararUsuario();
        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas);
        var bloqueio = usuario.BloqueadoAte;

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaErrada));

        Assert.Equal(bloqueio, usuario.BloqueadoAte);
        Assert.Equal(0, usuario.TentativasFalhas);
        await AssertGravacoesAsync(PoliticaDeLogin.MaxTentativas);
    }

    [Fact]
    public async Task Login_UmSegundoAntesDoFimDoBloqueio_ContinuaBloqueado()
    {
        var usuario = PrepararUsuario();
        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas);

        _tempo.Advance(PoliticaDeLogin.DuracaoBloqueio - TimeSpan.FromSeconds(1));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Login(SenhaCorreta));
        _tokenService.DidNotReceive().GerarToken(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Login_AposOPrazoDoBloqueio_VoltaAFuncionarEZeraOBloqueio()
    {
        var usuario = PrepararUsuario();
        await FalharVezes(usuario, PoliticaDeLogin.MaxTentativas);

        _tempo.Advance(PoliticaDeLogin.DuracaoBloqueio + TimeSpan.FromSeconds(1));

        var resultado = await Login(SenhaCorreta);

        Assert.Equal(TokenDeTeste, resultado.Token);
        Assert.Null(usuario.BloqueadoAte);
        Assert.Equal(0, usuario.TentativasFalhas);
        await _usuarioRepo.Received(1).AtualizarBloqueioAsync(usuario.Id, 0, null);
    }

    [Fact]
    public async Task Login_CorretoAposFalhas_ZeraContadorEGravaOReset()
    {
        var usuario = PrepararUsuario();
        await FalharVezes(usuario, 3);

        var resultado = await Login(SenhaCorreta);

        Assert.Equal(0, usuario.TentativasFalhas);
        Assert.Equal(TokenDeTeste, resultado.Token);
        await _usuarioRepo.Received(1).AtualizarBloqueioAsync(usuario.Id, 0, null);
    }

    [Fact]
    public async Task Login_CredenciaisCorretas_DevolveTokenENaoGravaNada()
    {
        PrepararUsuario();

        var resultado = await CriarServico().LoginAsync(
            new LoginDto { Email = "  ANA@exemplo.com ", Senha = SenhaCorreta });

        Assert.Equal(TokenDeTeste, resultado.Token);
        await AssertGravacoesAsync(0);
    }
}