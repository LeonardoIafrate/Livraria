using api.Helpers;
using Xunit;

namespace api.Tests
{
    public class CpfHelperTests
    {

        [Theory]
        [InlineData("529.982.247-25", true)]
        [InlineData("52998224725", true)]
        [InlineData("583.104.726-17", true)]
        [InlineData("111.111.111-11", false)]
        [InlineData("529.982.247-26", false)]
        [InlineData("123", false)]
        [InlineData("", false)]
        public void EhValido_DeveValidarCpf(string cpf, bool esperado)
        {
            var resultado = CpfHellper.EhValido(cpf);

            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void SomenteDigitos_DeveRemoverPontuacao()
        {
            var resultado = CpfHellper.SomenteDigitos("529.982.247-25");

            Assert.Equal("52998224725", resultado);
        }
    }
}