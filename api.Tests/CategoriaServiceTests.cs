using api.Dtos.Categoria;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class CategoriaServiceTests
{
    private const int CategoriaId = 1;

    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();

    private CategoriaService CriarServico() => new(_categoriaRepo);

    private static Categoria CriarCategoria(int id = CategoriaId, string genero = "Romance") => new()
    {
        Id = id,
        Genero = genero
    };

    private static CreateCategoriaDto CriarDto(string genero = "Aventura") => new() { Genero = genero };

    private static UpdateCategoriaDto CriarUpdateDto(string genero = "Aventura") => new() { Genero = genero };


    [Fact]
    public async Task GetAll_SemGenero_ListaTodasENaoBusca()
    {
        _categoriaRepo.GetAllAsync().Returns(new List<Categoria> { CriarCategoria(1), CriarCategoria(2, "Clássico") });

        var resultado = await CriarServico().GetAllAsync(null);

        Assert.Equal(2, resultado.Count);
        await _categoriaRepo.DidNotReceive().SearchByGeneroAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_GeneroEmBranco_ListaTodas()
    {
        _categoriaRepo.GetAllAsync().Returns(new List<Categoria> { CriarCategoria() });

        var resultado = await CriarServico().GetAllAsync("   ");

        Assert.Single(resultado);
        await _categoriaRepo.DidNotReceive().SearchByGeneroAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_ComGenero_BuscaPeloGenero()
    {
        _categoriaRepo.SearchByGeneroAsync("rom").Returns(new List<Categoria> { CriarCategoria() });

        var resultado = await CriarServico().GetAllAsync("rom");

        Assert.Single(resultado);
        await _categoriaRepo.DidNotReceive().GetAllAsync();
    }

    [Fact]
    public async Task GetAll_BuscaSemResultado_DevolveListaVazia()
    {
        _categoriaRepo.SearchByGeneroAsync("zzz").Returns(new List<Categoria>());

        var resultado = await CriarServico().GetAllAsync("zzz");

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetById_Inexistente_LancaNotFound()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(CategoriaId));
    }

    [Fact]
    public async Task GetById_Existente_DevolveODto()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(CriarCategoria());

        var resultado = await CriarServico().GetByIdAsync(CategoriaId);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task Create_GeneroEmUso_LancaConflictENaoSalva()
    {
        _categoriaRepo.GetByGeneroAsync("Aventura").Returns(CriarCategoria(id: 9, genero: "Aventura"));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().CreateAsync(CriarDto("Aventura")));

        await _categoriaRepo.DidNotReceive().CreateAsync(Arg.Any<Categoria>());
    }

    [Fact]
    public async Task Create_Valido_SalvaOGeneroDoDto()
    {
        _categoriaRepo.GetByGeneroAsync("Aventura").Returns((Categoria?)null);
        Categoria? salva = null;
        _categoriaRepo.CreateAsync(Arg.Do<Categoria>(c => salva = c)).Returns(chamada => chamada.Arg<Categoria>());

        var resultado = await CriarServico().CreateAsync(CriarDto("Aventura"));

        Assert.NotNull(resultado);
        Assert.NotNull(salva);
        Assert.Equal("Aventura", salva.Genero);
    }

    [Fact]
    public async Task Update_Inexistente_LancaNotFoundENaoAtualiza()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(CategoriaId, CriarUpdateDto()));

        await _categoriaRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Categoria>());
    }

    [Fact]
    public async Task Update_GeneroDeOutraCategoria_LancaConflictENaoAtualiza()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(CriarCategoria());
        _categoriaRepo.GetByGeneroAsync("Aventura").Returns(CriarCategoria(id: 9, genero: "Aventura"));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().UpdateAsync(CategoriaId, CriarUpdateDto("Aventura")));

        await _categoriaRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Categoria>());
    }

    [Fact]
    public async Task Update_MantendoOProprioGenero_Funciona()
    {
        var categoria = CriarCategoria(genero: "Romance");
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(categoria);
        _categoriaRepo.GetByGeneroAsync("Romance").Returns(categoria);
        _categoriaRepo.UpdateAsync(CategoriaId, Arg.Any<Categoria>()).Returns(categoria);

        var resultado = await CriarServico().UpdateAsync(CategoriaId, CriarUpdateDto("Romance"));

        Assert.NotNull(resultado);
        await _categoriaRepo.Received(1).UpdateAsync(CategoriaId, Arg.Any<Categoria>());
    }

    [Fact]
    public async Task Update_RepositorioDevolveNull_LancaNotFound()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(CriarCategoria());
        _categoriaRepo.GetByGeneroAsync("Aventura").Returns((Categoria?)null);
        _categoriaRepo.UpdateAsync(CategoriaId, Arg.Any<Categoria>()).Returns((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(CategoriaId, CriarUpdateDto()));
    }

    [Fact]
    public async Task Update_Valido_EnviaOGeneroDoDtoAoRepositorio()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(CriarCategoria());
        _categoriaRepo.GetByGeneroAsync("Aventura").Returns((Categoria?)null);
        Categoria? enviada = null;
        _categoriaRepo.UpdateAsync(CategoriaId, Arg.Do<Categoria>(c => enviada = c)).Returns(CriarCategoria(genero: "Aventura"));

        var resultado = await CriarServico().UpdateAsync(CategoriaId, CriarUpdateDto("Aventura"));

        Assert.NotNull(resultado);
        Assert.NotNull(enviada);
        Assert.Equal("Aventura", enviada.Genero);
    }

    [Fact]
    public async Task Delete_Inexistente_LancaNotFoundENaoExclui()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns((Categoria?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DeleteAsync(CategoriaId));

        await _categoriaRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_ComLivrosVinculados_LancaConflictENaoExclui()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(CriarCategoria());
        _categoriaRepo.PossuiLivroAsync(CategoriaId).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().DeleteAsync(CategoriaId));

        await _categoriaRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_SemLivros_ChamaORepositorio()
    {
        _categoriaRepo.GetByIdAsync(CategoriaId).Returns(CriarCategoria());
        _categoriaRepo.PossuiLivroAsync(CategoriaId).Returns(false);

        await CriarServico().DeleteAsync(CategoriaId);

        await _categoriaRepo.Received(1).DeleteAsync(CategoriaId);
    }
}