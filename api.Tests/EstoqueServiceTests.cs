using api.Dtos.Estoque;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using NSubstitute;
using Xunit;

namespace api.Tests.Services;

public class EstoqueServiceTests
{
    private const int EstoqueId = 1;
    private const int LivroId = 5;

    private readonly IEstoqueRepository _estoqueRepo = Substitute.For<IEstoqueRepository>();

    private EstoqueService CriarServico() => new(_estoqueRepo);
    
    private static Estoque CriarEstoque(int id = EstoqueId, int livroId = LivroId, int quantidade = 10) => new()
    {
        Id = id,
        LivroId = livroId,
        Quantidade = quantidade,
        Livro = new Livro { Id = livroId, Nome = "Dom Casmurro" }
    };

    private static UpdateEstoqueDto CriarUpdateDto(int quantidade = 25) => new() { Quantidade = quantidade };


    [Fact]
    public async Task GetAll_SemNome_ListaTodosENaoBusca()
    {
        _estoqueRepo.GetAllAsync().Returns(new List<Estoque> { CriarEstoque(1), CriarEstoque(2, 6) });

        var resultado = await CriarServico().GetAllAsync(null);

        Assert.Equal(2, resultado.Count);
        await _estoqueRepo.DidNotReceive().SearchByNomeLivroAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_NomeEmBranco_ListaTodos()
    {
        _estoqueRepo.GetAllAsync().Returns(new List<Estoque> { CriarEstoque() });

        var resultado = await CriarServico().GetAllAsync("   ");

        Assert.Single(resultado);
        await _estoqueRepo.DidNotReceive().SearchByNomeLivroAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAll_ComNome_BuscaPeloNomeDoLivro()
    {
        _estoqueRepo.SearchByNomeLivroAsync("casmurro").Returns(new List<Estoque> { CriarEstoque() });

        var resultado = await CriarServico().GetAllAsync("casmurro");

        Assert.Single(resultado);
        await _estoqueRepo.DidNotReceive().GetAllAsync();
    }

    [Fact]
    public async Task GetAll_BuscaSemResultado_DevolveListaVazia()
    {
        _estoqueRepo.SearchByNomeLivroAsync("zzz").Returns(new List<Estoque>());

        var resultado = await CriarServico().GetAllAsync("zzz");

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GetById_Inexistente_LancaNotFound()
    {
        _estoqueRepo.GetByIdAsync(EstoqueId).Returns((Estoque?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(EstoqueId));
    }

    [Fact]
    public async Task GetById_Existente_DevolveODto()
    {
        _estoqueRepo.GetByIdAsync(EstoqueId).Returns(CriarEstoque());

        var resultado = await CriarServico().GetByIdAsync(EstoqueId);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task GetByLivroId_Inexistente_LancaNotFound()
    {
        _estoqueRepo.GetByLivroIdAsync(LivroId).Returns((Estoque?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByLivroIdAsync(LivroId));
    }

    [Fact]
    public async Task GetByLivroId_Existente_DevolveODto()
    {
        _estoqueRepo.GetByLivroIdAsync(LivroId).Returns(CriarEstoque());

        var resultado = await CriarServico().GetByLivroIdAsync(LivroId);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task Update_Inexistente_LancaNotFound()
    {
        _estoqueRepo.UpdateAsync(EstoqueId, Arg.Any<Estoque>()).Returns((Estoque?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(EstoqueId, CriarUpdateDto()));
    }

    [Fact]
    public async Task Update_Valido_EnviaAQuantidadeDoDtoAoRepositorio()
    {
        Estoque? enviado = null;
        _estoqueRepo.UpdateAsync(EstoqueId, Arg.Do<Estoque>(e => enviado = e))
            .Returns(CriarEstoque(quantidade: 25));

        var resultado = await CriarServico().UpdateAsync(EstoqueId, CriarUpdateDto(quantidade: 25));

        Assert.NotNull(resultado);
        Assert.NotNull(enviado);
        Assert.Equal(25, enviado.Quantidade);
    }

    [Fact]
    public async Task Update_QuantidadeZero_EhPermitida()
    {
        _estoqueRepo.UpdateAsync(EstoqueId, Arg.Any<Estoque>()).Returns(CriarEstoque(quantidade: 0));

        var resultado = await CriarServico().UpdateAsync(EstoqueId, CriarUpdateDto(quantidade: 0));

        Assert.NotNull(resultado);
    }
}