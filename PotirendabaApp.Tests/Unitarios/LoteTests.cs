using PotirendabaApp.Models;
using PotirendabaApp.Tests.Infra;
using Xunit;

namespace PotirendabaApp.Tests.Unitarios
{
    /// <summary>Testes unitários da propriedade calculada Lote.DiasParaVencer.</summary>
    [Trait("Categoria", "Unitario")]
    public class LoteTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(0)]
        [InlineData(-3)]
        public void DiasParaVencer_conta_os_dias_entre_hoje_e_a_validade(int dias)
        {
            var lote = new Lote { DataValidade = Dados.Dia(dias) };

            Assert.Equal(dias, lote.DiasParaVencer);
        }

        [Fact]
        public void DiasParaVencer_com_data_invalida_retorna_valor_maximo()
        {
            var lote = new Lote { DataValidade = "data inválida" };

            Assert.Equal(int.MaxValue, lote.DiasParaVencer);
        }
    }
}
