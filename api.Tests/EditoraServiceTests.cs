using api.Dtos.Editora;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class EditoraServiceTests
{
    private const int EditoraId = 1;

    private readonly IEditoraRepository _editoraRepo = Substitute.For<IEditoraRepository>();

    private EditoraService CriarServico() => new(_editoraRepo);

    private static Editora CriarEditora(int id = EditoraId, string nome = "Editora Aurora") => new()
    {
        Id = id,
        Nome = nome
    };

    private static CreateEditoraDto CriarDto(string nome = "Editora Horizonte") => new() { Nome = nome };

    private static UpdateEditoraDto CriarUpdateDto(string nome = "Editora Horizonte") => new() { Nome = nome };

    [Fact]
    public async Task GetAll_SemNome_ListaTodasENaoBusca()
    {
        _editoraRepo.GetAllAsync().Returns(new List<Editora> { CriarEditora(1), CriarEditora(2, "Editora Cais") });

        var resultado = await CriarServico().GetAllAsync(null);

        Assert.Equal(2, resultado.Count);
        await _editoraRepo.DidNotReceive().SearchByNameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_NomeEmBranco_ListaTodas()
    {
        _editoraRepo.GetAllAsync().Returns(new List<Editora> { CriarEditora() });

        var resultado = await CriarServico().GetAllAsync("   ");

        Assert.Single(resultado);
        await _editoraRepo.DidNotReceive().SearchByNameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_ComNome_BuscaPeloNome()
    {
        _editoraRepo.SearchByNameAsync("aurora").Returns(new List<Editora> { CriarEditora() });

        var resultado = await CriarServico().GetAllAsync("aurora");

        Assert.Single(resultado);
        await _editoraRepo.DidNotReceive().GetAllAsync();
    }

    [Fact]
    public async Task GetAll_BuscaSemResultado_DevolveListaVazia()
    {
        _editoraRepo.SearchByNameAsync("zzz").Returns(new List<Editora>());

        var resultado = await CriarServico().GetAllAsync("zzz");

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetById_Inexistente_LancaNotFound()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns((Editora?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(EditoraId));
    }

    [Fact]
    public async Task GetById_Existente_DevolveODto()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns(CriarEditora());

        var resultado = await CriarServico().GetByIdAsync(EditoraId);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task Create_NomeEmUso_LancaConflictENaoSalva()
    {
        _editoraRepo.GetByNameAsync("Editora Horizonte").Returns(CriarEditora(id: 9, nome: "Editora Horizonte"));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().CreateAsync(CriarDto()));

        await _editoraRepo.DidNotReceive().CreateAsync(Arg.Any<Editora>());
    }

    [Fact]
    public async Task Create_Valido_SalvaONomeDoDto()
    {
        _editoraRepo.GetByNameAsync("Editora Horizonte").Returns((Editora?)null);
        Editora? salva = null;
        _editoraRepo.CreateAsync(Arg.Do<Editora>(e => salva = e)).Returns(chamada => chamada.Arg<Editora>());

        var resultado = await CriarServico().CreateAsync(CriarDto());

        Assert.NotNull(resultado);
        Assert.NotNull(salva);
        Assert.Equal("Editora Horizonte", salva.Nome);
    }

    [Fact]
    public async Task Update_Inexistente_LancaNotFoundENaoAtualiza()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns((Editora?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(CriarUpdateDto(), EditoraId));

        await _editoraRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Editora>());
    }

    [Fact]
    public async Task Update_NomeDeOutraEditora_LancaConflictENaoAtualiza()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns(CriarEditora());
        _editoraRepo.GetByNameAsync("Editora Horizonte").Returns(CriarEditora(id: 9, nome: "Editora Horizonte"));

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().UpdateAsync(CriarUpdateDto(), EditoraId));

        await _editoraRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Editora>());
    }

    [Fact]
    public async Task Update_MantendoOProprioNome_Funciona()
    {
        var editora = CriarEditora(nome: "Editora Aurora");
        _editoraRepo.GetByIdAsync(EditoraId).Returns(editora);
        _editoraRepo.GetByNameAsync("Editora Aurora").Returns(editora);
        _editoraRepo.UpdateAsync(EditoraId, Arg.Any<Editora>()).Returns(editora);

        var resultado = await CriarServico().UpdateAsync(CriarUpdateDto("Editora Aurora"), EditoraId);

        Assert.NotNull(resultado);
        await _editoraRepo.Received(1).UpdateAsync(EditoraId, Arg.Any<Editora>());
    }

    [Fact]
    public async Task Update_RepositorioDevolveNull_LancaNotFound()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns(CriarEditora());
        _editoraRepo.GetByNameAsync("Editora Horizonte").Returns((Editora?)null);
        _editoraRepo.UpdateAsync(EditoraId, Arg.Any<Editora>()).Returns((Editora?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(CriarUpdateDto(), EditoraId));
    }

    [Fact]
    public async Task Update_Valido_EnviaONomeDoDtoAoRepositorio()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns(CriarEditora());
        _editoraRepo.GetByNameAsync("Editora Horizonte").Returns((Editora?)null);
        Editora? enviada = null;
        _editoraRepo.UpdateAsync(EditoraId, Arg.Do<Editora>(e => enviada = e)).Returns(CriarEditora(nome: "Editora Horizonte"));

        var resultado = await CriarServico().UpdateAsync(CriarUpdateDto(), EditoraId);

        Assert.NotNull(resultado);
        Assert.NotNull(enviada);
        Assert.Equal("Editora Horizonte", enviada.Nome);
    }

    [Fact]
    public async Task Delete_Inexistente_LancaNotFoundENaoExclui()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns((Editora?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DeleteAsync(EditoraId));

        await _editoraRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_ComLivros_LancaConflictENaoExclui()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns(CriarEditora());
        _editoraRepo.PossuiLivrosAsync(EditoraId).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => CriarServico().DeleteAsync(EditoraId));

        await _editoraRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_SemLivros_ChamaORepositorio()
    {
        _editoraRepo.GetByIdAsync(EditoraId).Returns(CriarEditora());
        _editoraRepo.PossuiLivrosAsync(EditoraId).Returns(false);

        await CriarServico().DeleteAsync(EditoraId);

        await _editoraRepo.Received(1).DeleteAsync(EditoraId);
    }
}