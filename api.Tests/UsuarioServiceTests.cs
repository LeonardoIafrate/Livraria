using api.Dtos.Endereco;
using api.Dtos.Usuario;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class UsuarioServiceTests
{
    private const string SenhaAtual = "Senha123";

    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly PasswordHasher<Usuario> _hasher = new();

    private UsuarioService CriarServico() => new(_usuarioRepo, _hasher);
    private Usuario CriarUsuario(int id = 1, string email = "ana@email.com", bool ativo = true)
    {
        var usuario = new Usuario
        {
            Id = id,
            Nome = "Ana Souza",
            Email = email,
            Cpf = "52998224725",
            DataNascimento = new DateTime(1990, 5, 20),
            Ativo = ativo
        };
        usuario.SenhaHash = _hasher.HashPassword(usuario, SenhaAtual);
        return usuario;
    }

    private static CreateUsuarioDto CriarDto() => new()
    {
        Nome = "Ana Souza",
        Email = "  Ana@Email.COM ",
        Cpf = "529.982.247-25",
        Senha = SenhaAtual,
        DataNascimento = new DateTime(1990, 5, 20),
        Endereco = new EnderecoBaseDto
        {
            Cep = "09010-100",
            Rua = " Rua das Flores ",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "Santo André",
            Uf = "sp"
        }
    };

    private static UpdateUsuarioDto CriarUpdateDto(string email = "Novo@Email.com") => new()
    {
        Nome = "Ana Maria",
        Email = email,
        DataNascimento = new DateTime(1990, 5, 20)
    };


    [Fact]
    public async Task Create_CpfJaCadastrado_LancaConflictENaoSalva()
    {
        _usuarioRepo.GetByCpfAsync("52998224725").Returns(CriarUsuario(id: 9));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().CreateAsync(CriarDto()));

        await _usuarioRepo.DidNotReceive().CreateAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Create_EmailJaCadastrado_LancaConflictENaoSalva()
    {
        _usuarioRepo.GetByEmailAsync("ana@email.com").Returns(CriarUsuario(id: 9));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().CreateAsync(CriarDto()));

        await _usuarioRepo.DidNotReceive().CreateAsync(Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Create_Valido_GravaDadosNormalizadosSenhaComHashEEnderecoPrincipal()
    {
        Usuario? salvo = null;
        _usuarioRepo.CreateAsync(Arg.Do<Usuario>(u => salvo = u))
            .Returns(chamada => chamada.Arg<Usuario>());

        await CriarServico().CreateAsync(CriarDto());

        Assert.NotNull(salvo);

        Assert.Equal("ana@email.com", salvo.Email);
        Assert.Equal("52998224725", salvo.Cpf);

        Assert.NotEmpty(salvo.SenhaHash);
        Assert.NotEqual(SenhaAtual, salvo.SenhaHash);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(salvo, salvo.SenhaHash, SenhaAtual));

        Assert.Equal(PerfilUsuario.Cliente, salvo.Perfil);

        var endereco = Assert.Single(salvo.Enderecos);
        Assert.True(endereco.Principal);
        Assert.True(endereco.Ativo);
        Assert.Equal("09010100", endereco.Cep);
        Assert.Equal("Rua das Flores", endereco.Rua);
        Assert.Equal("SP", endereco.Uf);
    }

    [Fact]
    public async Task Update_UsuarioInexistente_LancaNotFound()
    {
        _usuarioRepo.GetByIdAsync(1).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(1, CriarUpdateDto()));
    }

    [Fact]
    public async Task Update_EmailDeOutroUsuario_LancaConflictENaoSalva()
    {
        _usuarioRepo.GetByIdAsync(1).Returns(CriarUsuario(id: 1));
        _usuarioRepo.GetByEmailAsync("novo@email.com").Returns(CriarUsuario(id: 2, email: "novo@email.com"));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().UpdateAsync(1, CriarUpdateDto()));

        await _usuarioRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Usuario>());
    }

    [Fact]
    public async Task Update_MantendoOProprioEmail_Funciona()
    {
        var usuario = CriarUsuario(id: 1);
        _usuarioRepo.GetByIdAsync(1).Returns(usuario);
        _usuarioRepo.GetByEmailAsync("ana@email.com").Returns(usuario);
        _usuarioRepo.UpdateAsync(1, Arg.Any<Usuario>()).Returns(usuario);

        await CriarServico().UpdateAsync(1, CriarUpdateDto(email: "ANA@email.com"));

        await _usuarioRepo.Received(1).UpdateAsync(1, Arg.Any<Usuario>());
    }

    [Fact]
    public async Task AlterarSenha_UsuarioInativo_LancaNotFound()
    {
        _usuarioRepo.GetByIdAsync(1).Returns(CriarUsuario(ativo: false));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CriarServico().AlterarSenhaAsync(1,
                new AlterarSenhaDto { SenhaAtual = SenhaAtual, NovaSenha = "NovaSenha456" }));
    }

    [Fact]
    public async Task AlterarSenha_SenhaAtualErrada_LancaBadRequestENaoGrava()
    {
        _usuarioRepo.GetByIdAsync(1).Returns(CriarUsuario());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            CriarServico().AlterarSenhaAsync(1,
                new AlterarSenhaDto { SenhaAtual = "errada123", NovaSenha = "NovaSenha456" }));

        await _usuarioRepo.DidNotReceive().UpdateSenhaAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AlterarSenha_NovaIgualAAtual_LancaBadRequestENaoGrava()
    {
        _usuarioRepo.GetByIdAsync(1).Returns(CriarUsuario());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            CriarServico().AlterarSenhaAsync(1,
                new AlterarSenhaDto { SenhaAtual = SenhaAtual, NovaSenha = SenhaAtual }));

        await _usuarioRepo.DidNotReceive().UpdateSenhaAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AlterarSenha_Valida_GravaOHashDaNovaSenha()
    {
        var usuario = CriarUsuario();
        _usuarioRepo.GetByIdAsync(1).Returns(usuario);

        string? novoHash = null;
        _usuarioRepo.UpdateSenhaAsync(1, Arg.Do<string>(h => novoHash = h)).Returns(true);

        await CriarServico().AlterarSenhaAsync(1,
            new AlterarSenhaDto { SenhaAtual = SenhaAtual, NovaSenha = "NovaSenha456" });

        Assert.NotNull(novoHash);

        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(usuario, novoHash, "NovaSenha456"));
        Assert.Equal(
            PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(usuario, novoHash, SenhaAtual));
    }

    [Fact]
    public async Task AlterarAdmin_UsuarioInexistente_LancaNotFoundENaoAltera()
    {
        _usuarioRepo.GetByIdAsync(2).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => 
            CriarServico().AlterarAdminAsync(2));

        await _usuarioRepo.DidNotReceive().AlterarUsuarioAdminAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task AlterarAdmin_UsuarioInativo_LancaConflictENaoAltera()
    {
        _usuarioRepo.GetByIdAsync(2).Returns(CriarUsuario(id: 2, ativo: false));

        await Assert.ThrowsAsync<ConflictException>(() =>
            CriarServico().AlterarAdminAsync(2));

        await _usuarioRepo.DidNotReceive().AlterarUsuarioAdminAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task AlterarAdmin_UsuarioAtivo_ChamaORepositorio()
    {
        _usuarioRepo.GetByIdAsync(2).Returns(CriarUsuario(id:2));

        await CriarServico().AlterarAdminAsync(2);

        await _usuarioRepo.Received(1).AlterarUsuarioAdminAsync(2);
    }

    [Fact]
    public async Task GetAll_SemNome_ListaTodos()
    {
        _usuarioRepo.GetAllAsync().Returns(new List<Usuario> { CriarUsuario() });

        var resultado = await CriarServico().GetAllAsync(null);

        Assert.Single(resultado);
        await _usuarioRepo.DidNotReceive().SearchByNomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_BuscaSemResultado_DevolveListaVaziaEmVezDeErro()
    {
        _usuarioRepo.SearchByNomeAsync("zzz").Returns(new List<Usuario>());

        var resultado = await CriarServico().GetAllAsync("zzz");

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetByEmail_NormalizaAntesDeBuscar()
    {
        _usuarioRepo.GetByEmailAsync("ana@email.com").Returns(CriarUsuario());

        var resultado = await CriarServico().GetByEmailAsync("  ANA@Email.com ");

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task GetByCpf_RemovePontuacaoAntesDeBuscar()
    {
        _usuarioRepo.GetByCpfAsync("52998224725").Returns(CriarUsuario());

        var resultado = await CriarServico().GetByCpfAsync("529.982.247-25");

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task GetById_Inexistente_LancaNotFound()
    {
        _usuarioRepo.GetByIdAsync(1).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(1));
    }

    [Fact]
    public async Task Desativar_Inexistente_LancaNotFound()
    {
        _usuarioRepo.DesativarAsync(1).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DesativarAsync(1));
    }

    [Fact]
    public async Task Desativar_Existente_ChamaORepositorio()
    {
        _usuarioRepo.DesativarAsync(1).Returns(CriarUsuario(ativo: false));

        await CriarServico().DesativarAsync(1);

        await _usuarioRepo.Received(1).DesativarAsync(1);
    }

    [Fact]
    public async Task Delete_Inexistente_LancaNotFoundENaoExclui()
    {
        _usuarioRepo.GetByIdAsync(1).Returns((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DeleteAsync(1));

        await _usuarioRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_Existente_ChamaORepositorio()
    {
        _usuarioRepo.GetByIdAsync(1).Returns(CriarUsuario());

        await CriarServico().DeleteAsync(1);

        await _usuarioRepo.Received(1).DeleteAsync(1);
    }

    [Fact]
    public async Task Delete_UsuarioComPedidos_LancaConflictENaoExclui()
    {
        _usuarioRepo.GetByIdAsync(1).Returns(CriarUsuario());
        _usuarioRepo.PossuiPedidosAsync(1).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().DeleteAsync(1));

        await _usuarioRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }
}