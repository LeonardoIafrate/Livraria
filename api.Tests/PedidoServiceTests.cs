using api.Dtos.Pedido;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using NSubstitute;
using Xunit;

namespace api.Tests
{
    public class PedidoServiceTests
    {
        private const int UsuarioId = 1;
        private const int EnderecoId = 10;

        private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();
        private readonly IEnderecoRepository _enderecoRepo = Substitute.For<IEnderecoRepository>();
        private readonly ILivroRepository _livroRepo = Substitute.For<ILivroRepository>();

        private PedidoService CriarServico() => new(_pedidoRepo, _enderecoRepo, _livroRepo);

        private static Endereco CriarEndereco(int usuarioId = UsuarioId, bool ativo = true) => new()
        {
            Id = EnderecoId,
            UsuarioId = usuarioId,
            Ativo = ativo,
            Cep = "09010100",
            Rua = "Rua das Flores",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "Santo André",
            Uf = "SP"
        };

        private static Livro CriarLivro(int id = 1, decimal preco = 30m, int estoque = 10) => new()
        {
            Id = id,
            Nome = "Dom Casmurro",
            Preco = preco,
            Estoque = new Estoque { Quantidade = estoque }
        };

        private static CreatePedidoDto CriarDto(params (int livroId, int quantidade)[] itens)
        {
            var dto = new CreatePedidoDto {EnderecoId = EnderecoId};

            foreach (var (livroId, quantidade) in itens)
                dto.Itens.Add(new CreateItemPedidoDto {LivroId = livroId, Quantidade = quantidade});

            return dto;
        }

        [Fact]
        public async Task CreatePedido_EnderecoDeOutroUsuario_LancaBadRequest()
        {
            _enderecoRepo.GetByIdAsync(EnderecoId).Returns(CriarEndereco(usuarioId: 99));

            await Assert.ThrowsAsync<BadRequestException>(()=>
                CriarServico().CreatePedidoAsync(UsuarioId, CriarDto((1, 1))));
        }

        [Fact]
        public async Task CreatePedido_LivroInexistente_LancaBadRequest()
        {
            _enderecoRepo.GetByIdAsync(EnderecoId).Returns(CriarEndereco());
            _livroRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Livro>());
    
            await Assert.ThrowsAsync<BadRequestException>(() =>
                CriarServico().CreatePedidoAsync(UsuarioId, CriarDto((1, 1))));
        }

        [Fact]
        public async Task CreatePedido_EstoqueInsuficiente_LancaConflict()
        {
            _enderecoRepo.GetByIdAsync(EnderecoId).Returns(CriarEndereco());
            _livroRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Livro> {CriarLivro(estoque: 1)});

            await Assert.ThrowsAsync<ConflictException>(() => 
                CriarServico().CreatePedidoAsync(UsuarioId, CriarDto((1,2))));
        }

        [Fact]
        public async Task CreatePedido_Valido_CalculaTotalUsaPrecoDoServidorEDescontaEstoque()
        {
            var livro = CriarLivro(preco: 30m, estoque: 10);
    
            _enderecoRepo.GetByIdAsync(EnderecoId).Returns(CriarEndereco());
            _livroRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Livro> { livro });
            _pedidoRepo.CreatePedidoAsync(Arg.Any<Pedido>()).Returns(chamada => chamada.Arg<Pedido>());
    
            var resultado = await CriarServico().CreatePedidoAsync(UsuarioId, CriarDto((1,2), (1,1)));

            Assert.Single(resultado.Itens);
            Assert.Equal(3, resultado.Itens[0].Quantidade);
            Assert.Equal(30m, resultado.Itens[0].PrecoUnitario);
            Assert.Equal(90m, resultado.ValorTotal);
            Assert.Equal("Pendente", resultado.Status);
            Assert.Equal(7, livro.Estoque!.Quantidade);
        }

        [Fact]
        public async Task Cancelar_PedidoDeOutroUsuario_LancaNotFound()
        {
            _pedidoRepo.GetByIdAsync(5).Returns(
                new Pedido{Id = 5, UsuarioId = 2, Status = StatusPedido.Pendente});

            await Assert.ThrowsAsync<NotFoundException>(() => 
            CriarServico().CancelarPedidoAsync(5, usuarioId: UsuarioId, ehAdmin: false));
        }

        [Theory]
        [InlineData(StatusPedido.Enviado)]
        [InlineData(StatusPedido.Entregue)]
        [InlineData(StatusPedido.Cancelado)]
        public async Task Cancelar_StatusQueNaoPermite_LancaConflict(StatusPedido status)
        {
            _pedidoRepo.GetByIdAsync(5).Returns(
                new Pedido { Id = 5, UsuarioId = UsuarioId, Status = status});

            await Assert.ThrowsAsync<ConflictException>(() => CriarServico().CancelarPedidoAsync(5, UsuarioId, ehAdmin: false));
        }

        [Theory]
        [InlineData(StatusPedido.Pendente, StatusPedido.Enviado)]    
        [InlineData(StatusPedido.Pago, StatusPedido.Entregue)]       
        [InlineData(StatusPedido.Entregue, StatusPedido.Pendente)]   
        [InlineData(StatusPedido.Pago, StatusPedido.Cancelado)]
        public async Task AtualizarStatus_TransicaoInvalida_LancaConflict(StatusPedido atual, StatusPedido novo)
        {
            _pedidoRepo.GetByIdAsync(5).Returns(new Pedido {Id = 5, UsuarioId = UsuarioId, Status = atual});

            await Assert.ThrowsAsync<ConflictException>(() => 
                CriarServico().AtualizarStatusAsync(5, novo));
        }

        [Fact]
        public async Task AtualizarStatus_TransicaoValida_ChamaORepositorio()
        {
            var pedido = new Pedido {Id = 5, UsuarioId = UsuarioId, Status = StatusPedido.Pago};
            _pedidoRepo.GetByIdAsync(5).Returns(pedido);
            _pedidoRepo.AtualizarStatusAsync(5, StatusPedido.Enviado)
                       .Returns(new Pedido {Id = 5, UsuarioId = UsuarioId, Status = StatusPedido.Enviado});

            var resultado = await CriarServico().AtualizarStatusAsync(5, StatusPedido.Enviado);

            Assert.Equal("Enviado", resultado.Status);
            await _pedidoRepo.Received(1).AtualizarStatusAsync(5, StatusPedido.Enviado);
        }

    }
}