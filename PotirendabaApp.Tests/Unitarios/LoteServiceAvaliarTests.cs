using PotirendabaApp.Models;
using PotirendabaApp.Services;
using PotirendabaApp.Tests.Infra;
using Xunit;

namespace PotirendabaApp.Tests.Unitarios
{
    /// <summary>
    /// Testes unitários da regra de alerta (LoteService.Avaliar).
    /// Não usam banco de dados: avaliam apenas a classificação do lote
    /// a partir dos dias restantes até o vencimento.
    /// Correspondem aos casos manuais TC06 e TC07, incluindo os limites exatos.
    /// </summary>
    [Trait("Categoria", "Unitario")]
    public class LoteServiceAvaliarTests
    {
        [Theory]
        [InlineData(30, AcaoSugerida.Nenhuma)]
        [InlineData(8, AcaoSugerida.Nenhuma)]   // primeiro dia fora da janela de promoção
        [InlineData(7, AcaoSugerida.Promocao)]  // limite superior da promoção
        [InlineData(4, AcaoSugerida.Promocao)]  // limite inferior da promoção
        [InlineData(3, AcaoSugerida.Descarte)]  // limite do descarte (comparação <=)
        [InlineData(0, AcaoSugerida.Descarte)]  // vence hoje
        [InlineData(-5, AcaoSugerida.Descarte)] // já vencido
        public void Avaliar_classifica_pelo_numero_de_dias_restantes(int diasParaVencer, AcaoSugerida esperado)
        {
            var lote = new Lote { Quantidade = 10, DataValidade = Dados.Dia(diasParaVencer) };

            var acao = LoteService.Avaliar(lote);

            Assert.Equal(esperado, acao);
        }

        [Fact]
        public void Avaliar_sem_data_de_validade_valida_nao_gera_alerta()
        {
            var lote = new Lote { Quantidade = 10, DataValidade = "" };

            Assert.Equal(AcaoSugerida.Nenhuma, LoteService.Avaliar(lote));
        }

        [Fact]
        public void Limites_de_alerta_sao_7_e_3_dias()
        {
            Assert.Equal(7, LoteService.DiasLimitePromocao);
            Assert.Equal(3, LoteService.DiasLimiteDescarte);
        }
    }
}
