using api.Dtos.Endereco;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class EnderecoServiceTests
{
    private const int UsuarioId = 1;
    private const int OutroUsuarioId = 99;

    private readonly IEnderecoRepository _enderecoRepo = Substitute.For<IEnderecoRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();

    private EnderecoService CriarServico() => new(_enderecoRepo, _usuarioRepo);

    private static Endereco CriarEndereco(
        int id = 1, int usuarioId = UsuarioId, bool principal = false, bool ativo = true) => new()
    {
        Id = id,
        UsuarioId = usuarioId,
        Principal = principal,
        Ativo = ativo,
        Cep = "09010100",
        Rua = "Rua das Flores",
        Numero = "123",
        Bairro = "Centro",
        Cidade = "Santo André",
        Uf = "SP"
    };

    private static CreateEnderecoDto CriarDto(bool principal = false) => new()
    {
        Cep = "09010-100",
        Rua = " Rua das Flores ",
        Numero = "123",
        Bairro = "Centro",
        Cidade = "Santo André",
        Uf = "sp",
        Principal = principal
    };

    [Fact]
    public async Task GetByUsuarioId_UsuarioInexistente_LancaNotFound()
    {
        _usuarioRepo.ExistsAsync(UsuarioId).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByUsuarioIdAsync(UsuarioId));
    }

    [Fact]
    public async Task GetByUsuarioId_SemEnderecos_DevolveListaVazia()
    {
        _usuarioRepo.ExistsAsync(UsuarioId).Returns(true);
        _enderecoRepo.GetByUsuarioIdAsync(UsuarioId).Returns(new List<Endereco>());

        var resultado = await CriarServico().GetByUsuarioIdAsync(UsuarioId);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetById_EnderecoDeOutroUsuario_LancaNotFound()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(usuarioId: OutroUsuarioId));

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(1, UsuarioId));
    }

    [Fact]
    public async Task GetById_EnderecoDesativado_LancaNotFound()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(ativo: false));

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(1, UsuarioId));
    }

    [Fact]
    public async Task GetById_EnderecoDoProprioUsuario_Devolve()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco());

        var resultado = await CriarServico().GetByIdAsync(1, UsuarioId);

        Assert.Equal(1, resultado.Id);
    }

    [Fact]
    public async Task Create_UsuarioInexistente_LancaNotFound()
    {
        _usuarioRepo.ExistsAsync(UsuarioId).Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().CreateAsync(UsuarioId, CriarDto()));

        await _enderecoRepo.DidNotReceive().CreateAsync(Arg.Any<Endereco>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_PrimeiroEndereco_VemComoPrincipalENormalizado(bool pediuPrincipal)
    {
        _usuarioRepo.ExistsAsync(UsuarioId).Returns(true);
        _enderecoRepo.PossuiEnderecoAtivoAsync(UsuarioId).Returns(false);

        Endereco? salvo = null;
        _enderecoRepo.CreateAsync(Arg.Do<Endereco>(e => salvo = e))
            .Returns(chamada => chamada.Arg<Endereco>());

        await CriarServico().CreateAsync(UsuarioId, CriarDto(principal: pediuPrincipal));

        Assert.NotNull(salvo);
        Assert.True(salvo.Principal);
        Assert.Equal(UsuarioId, salvo.UsuarioId);
        Assert.Equal("09010100", salvo.Cep);
        Assert.Equal("Rua das Flores", salvo.Rua);
        Assert.Equal("SP", salvo.Uf);

        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Create_SegundoEndereco_SemPedirPrincipal_NaoViraPrincipal()
    {
        _usuarioRepo.ExistsAsync(UsuarioId).Returns(true);
        _enderecoRepo.PossuiEnderecoAtivoAsync(UsuarioId).Returns(true);

        Endereco? salvo = null;
        _enderecoRepo.CreateAsync(Arg.Do<Endereco>(e => salvo = e))
            .Returns(chamada => chamada.Arg<Endereco>());

        await CriarServico().CreateAsync(UsuarioId, CriarDto(principal: false));

        Assert.NotNull(salvo);
        Assert.False(salvo.Principal);

        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Create_SegundoEndereco_PedindoPrincipal_ChamaDefinirPrincipal()
    {
        _usuarioRepo.ExistsAsync(UsuarioId).Returns(true);
        _enderecoRepo.PossuiEnderecoAtivoAsync(UsuarioId).Returns(true);

        _enderecoRepo.CreateAsync(Arg.Any<Endereco>()).Returns(chamada =>
        {
            var endereco = chamada.Arg<Endereco>();
            endereco.Id = 7;
            return endereco;
        });
        _enderecoRepo.DefinirPrincipalAsync(7).Returns(CriarEndereco(id: 7, principal: true));

        var resultado = await CriarServico().CreateAsync(UsuarioId, CriarDto(principal: true));

        Assert.True(resultado.Principal);
        await _enderecoRepo.Received(1).DefinirPrincipalAsync(7);
    }

    [Fact]
    public async Task Update_EnderecoDeOutroUsuario_LancaNotFoundENaoAltera()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(usuarioId: OutroUsuarioId));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CriarServico().UpdateAsync(1, UsuarioId, new UpdateEnderecoDto { Cep = "09010100" }));

        await _enderecoRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Endereco>());
    }

    [Fact]
    public async Task Update_EnderecoDesativado_LancaNotFound()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(ativo: false));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CriarServico().UpdateAsync(1, UsuarioId, new UpdateEnderecoDto { Cep = "09010100" }));
    }

    [Fact]
    public async Task Update_Valido_EnviaDadosNormalizadosAoRepositorio()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco());

        Endereco? enviado = null;
        _enderecoRepo.UpdateAsync(1, Arg.Do<Endereco>(e => enviado = e)).Returns(CriarEndereco());

        await CriarServico().UpdateAsync(1, UsuarioId, new UpdateEnderecoDto
        {
            Cep = "09010-100",
            Rua = " Rua Nova ",
            Numero = "50",
            Bairro = "Jardim",
            Cidade = "São Paulo",
            Uf = "sp"
        });

        Assert.NotNull(enviado);
        Assert.Equal("09010100", enviado.Cep);
        Assert.Equal("Rua Nova", enviado.Rua);
        Assert.Equal("SP", enviado.Uf);
    }

    [Fact]
    public async Task DefinirPrincipal_EnderecoDeOutroUsuario_LancaNotFoundENaoAltera()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(usuarioId: OutroUsuarioId));

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DefinirPrincipalAsync(1, UsuarioId));

        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task DefinirPrincipal_JaEhOPrincipal_NaoChamaORepositorio()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(principal: true));

        var resultado = await CriarServico().DefinirPrincipalAsync(1, UsuarioId);

        Assert.True(resultado.Principal);
        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task DefinirPrincipal_Valido_ChamaORepositorio()
    {
        _enderecoRepo.GetByIdAsync(2).Returns(CriarEndereco(id: 2, principal: false));
        _enderecoRepo.DefinirPrincipalAsync(2).Returns(CriarEndereco(id: 2, principal: true));

        var resultado = await CriarServico().DefinirPrincipalAsync(2, UsuarioId);

        Assert.True(resultado.Principal);
        await _enderecoRepo.Received(1).DefinirPrincipalAsync(2);
    }

    [Fact]
    public async Task Excluir_EnderecoDeOutroUsuario_LancaNotFoundENaoExclui()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(usuarioId: OutroUsuarioId));

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().ExcluirAsync(1, UsuarioId));

        await _enderecoRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Excluir_EnderecoSecundario_NaoPromoveNingem()
    {
        _enderecoRepo.GetByIdAsync(2).Returns(CriarEndereco(id: 2, principal: false));

        await CriarServico().ExcluirAsync(2, UsuarioId);

        await _enderecoRepo.Received(1).DeleteAsync(2);
        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Excluir_EnderecoPrincipal_PromoveOPrimeiroQueSobrar()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(id: 1, principal: true));
        _enderecoRepo.GetByUsuarioIdAsync(UsuarioId).Returns(new List<Endereco>
        {
            CriarEndereco(id: 2),
            CriarEndereco(id: 3)
        });

        await CriarServico().ExcluirAsync(1, UsuarioId);

        await _enderecoRepo.Received(1).DeleteAsync(1);
        await _enderecoRepo.Received(1).DefinirPrincipalAsync(2);
        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(3);
    }

    [Fact]
    public async Task Excluir_UnicoEnderecoPrincipal_NaoTentaPromoverNinguem()
    {
        _enderecoRepo.GetByIdAsync(1).Returns(CriarEndereco(id: 1, principal: true));
        _enderecoRepo.GetByUsuarioIdAsync(UsuarioId).Returns(new List<Endereco>());

        await CriarServico().ExcluirAsync(1, UsuarioId);

        await _enderecoRepo.Received(1).DeleteAsync(1);
        await _enderecoRepo.DidNotReceive().DefinirPrincipalAsync(Arg.Any<int>());
    }
}