using api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.Tests
{
    public class GlobalExceptionHandlerTests
    {
        private static async Task<(int Status, string Corpo)> Executar(Exception excecao)
        {
            var contexto = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection().BuildServiceProvider()
            };
            contexto.Response.Body = new MemoryStream();

            var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

            var tratou = await handler.TryHandleAsync(contexto, excecao, CancellationToken.None);
            Assert.True(tratou);

            contexto.Response.Body.Position = 0;
            var corpo = await new StreamReader(contexto.Response.Body).ReadToEndAsync();

            return(contexto.Response.StatusCode, corpo);
        }

        [Theory]
        [InlineData(typeof(BadRequestException), 400)]
        [InlineData(typeof(UnauthorizedException), 401)]
        [InlineData(typeof(ForbiddenException), 403)]
        [InlineData(typeof(NotFoundException), 404)]
        [InlineData(typeof(ConflictException), 409)]
        public async Task ExcecaoDoProjeto_VirarOStatusCertoEMostraAMensagem(Type tipo, int statusEsperado)
        {
            var excecao = (Exception)Activator.CreateInstance(tipo, "mensagem de teste")!;

            var (status, corpo) = await Executar(excecao);

            Assert.Equal(statusEsperado, status);
            Assert.Contains("mensagem de teste", corpo);
        }

        [Fact]
        public async Task ExcecaoDesconhecida_Vira500ENaoVazaOsDetalhes()
        {
            var excecao = new InvalidOperationException("string-de-conexao-secreta");

            var (status, corpo) = await Executar(excecao);

            Assert.Equal(500, status);
            Assert.DoesNotContain("string-de-conexao-secreta", corpo);
        }

        [Fact]
        public async Task ConflitoDeConcorrencia_Vira409ComMensagemLegivel()
        {
            var excecao = new DbUpdateConcurrencyException("mesnagem técnica internar do EF");

            var (status, corpo) = await Executar(excecao);

            Assert.Equal(409, status);
            Assert.Contains("Tente novamente", corpo);
            Assert.DoesNotContain("Mensagem técnica interna do EF", corpo);
        }
    }
}