using api.Dtos.Autor;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class AutorServiceTests
{
    private const int AutorId = 1;

    private readonly IAutorRepository _autorRepo = Substitute.For<IAutorRepository>();

    private AutorService CriarServico() => new(_autorRepo);

    private static Autor CriarAutor(int id = AutorId, string nome = "Machado de Assis") => new()
    {
        Id = id,
        Nome = nome,
        Nacionalidade = "Brasileira",
        Biografia = "Escritor carioca, fundador da Academia Brasileira de Letras."
    };

    private static CreateAutorDto CriarDto() => new()
    {
        Nome = "Clarice Lispector",
        Nacionalidade = "Brasileira",
        Biografia = "Escritora nascida na Ucrânia e naturalizada brasileira."
    };

    private static UpdateAutorDto CriarUpdateDto() => new()
    {
        Nome = "Machado de Assis (editado)",
        Nacionalidade = "Brasileira",
        Biografia = "Biografia atualizada."
    };

    [Fact]
    public async Task GetAll_SemNome_ListaTodosENaoBusca()
    {
        _autorRepo.GetAllAsync().Returns(new List<Autor> { CriarAutor(1), CriarAutor(2, "Lima Barreto") });

        var resultado = await CriarServico().GetAllAsync(null);

        Assert.Equal(2, resultado.Count);
        await _autorRepo.DidNotReceive().SearchByNomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_NomeEmBranco_ListaTodos()
    {
        _autorRepo.GetAllAsync().Returns(new List<Autor> { CriarAutor() });

        var resultado = await CriarServico().GetAllAsync("   ");

        Assert.Single(resultado);
        await _autorRepo.DidNotReceive().SearchByNomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_ComNome_BuscaPeloNome()
    {
        _autorRepo.SearchByNomeAsync("machado").Returns(new List<Autor> { CriarAutor() });

        var resultado = await CriarServico().GetAllAsync("machado");

        Assert.Single(resultado);
        await _autorRepo.DidNotReceive().GetAllAsync();
    }

    [Fact]
    public async Task GetAll_BuscaSemResultado_DevolveListaVazia()
    {
        _autorRepo.SearchByNomeAsync("zzz").Returns(new List<Autor>());

        var resultado = await CriarServico().GetAllAsync("zzz");

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetById_Inexistente_LancaNotFound()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns((Autor?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(AutorId));
    }

    [Fact]
    public async Task GetById_Existente_DevolveODto()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns(CriarAutor());

        var resultado = await CriarServico().GetByIdAsync(AutorId);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task Create_Valido_SalvaOsDadosDoDto()
    {
        Autor? salvo = null;
        _autorRepo.CreateAsync(Arg.Do<Autor>(a => salvo = a)).Returns(chamada => chamada.Arg<Autor>());

        var resultado = await CriarServico().CreateAsync(CriarDto());

        Assert.NotNull(resultado);
        Assert.NotNull(salvo);
        Assert.Equal("Clarice Lispector", salvo.Nome);
        Assert.Equal("Brasileira", salvo.Nacionalidade);
        Assert.Equal("Escritora nascida na Ucrânia e naturalizada brasileira.", salvo.Biografia);
    }

    [Fact]
    public async Task Update_Inexistente_LancaNotFoundENaoAtualiza()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns((Autor?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(AutorId, CriarUpdateDto()));

        await _autorRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Autor>());
    }

    [Fact]
    public async Task Update_RepositorioDevolveNull_LancaNotFound()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns(CriarAutor());
        _autorRepo.UpdateAsync(AutorId, Arg.Any<Autor>()).Returns((Autor?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(AutorId, CriarUpdateDto()));
    }

    [Fact]
    public async Task Update_Valido_EnviaOsDadosDoDtoAoRepositorio()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns(CriarAutor());
        Autor? enviado = null;
        _autorRepo.UpdateAsync(AutorId, Arg.Do<Autor>(a => enviado = a)).Returns(CriarAutor());

        var resultado = await CriarServico().UpdateAsync(AutorId, CriarUpdateDto());

        Assert.NotNull(resultado);
        Assert.NotNull(enviado);
        Assert.Equal("Machado de Assis (editado)", enviado.Nome);
        Assert.Equal("Biografia atualizada.", enviado.Biografia);
    }

    [Fact]
    public async Task Delete_Inexistente_LancaNotFoundENaoExclui()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns((Autor?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DeleteAsync(AutorId));

        await _autorRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_ComLivrosVinculados_LancaConflictENaoExclui()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns(CriarAutor());
        _autorRepo.PossuiLivros(AutorId).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().DeleteAsync(AutorId));

        await _autorRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_SemLivros_ChamaORepositorio()
    {
        _autorRepo.GetByIdAsync(AutorId).Returns(CriarAutor());
        _autorRepo.PossuiLivros(AutorId).Returns(false);

        await CriarServico().DeleteAsync(AutorId);

        await _autorRepo.Received(1).DeleteAsync(AutorId);
    }
}