using System;
using System.Linq;
using PotirendabaApp.Services;
using PotirendabaApp.Tests.Infra;
using Xunit;

namespace PotirendabaApp.Tests.Integracao
{
    /// <summary>
    /// Testes de integração do consumo FEFO (LoteService.ConsumirFEFO + DatabaseHelper),
    /// executados contra um banco SQLite real, criado vazio para cada teste.
    /// Correspondem aos casos manuais TC04 e TC05, mais cenários de borda.
    /// </summary>
    [Trait("Categoria", "Integracao")]
    public class LoteServiceConsumirFefoTests : IDisposable
    {
        private readonly BancoDeTeste _banco = new BancoDeTeste();
        public void Dispose() => _banco.Dispose();

        [Fact] // TC04
        public void Venda_com_um_lote_debita_o_lote_e_recalcula_o_estoque()
        {
            int produto = Dados.NovoProduto();
            int lote = Dados.NovoLote(produto, quantidade: 20, diasValidade: 10);

            var consumos = LoteService.ConsumirFEFO(produto, 5);

            var c = Assert.Single(consumos);
            Assert.Equal(lote, c.LoteId);
            Assert.Equal(5, c.Quantidade);
            Assert.Equal(15, Dados.QuantidadeDoLote(lote));
            Assert.Equal(15, Dados.EstoqueDoProduto(produto));
        }

        [Fact] // TC05
        public void Venda_maior_que_o_lote_mais_proximo_divide_o_consumo_entre_lotes()
        {
            int produto = Dados.NovoProduto();
            int loteProximo = Dados.NovoLote(produto, quantidade: 6, diasValidade: 3);
            int loteSeguinte = Dados.NovoLote(produto, quantidade: 10, diasValidade: 10);

            var consumos = LoteService.ConsumirFEFO(produto, 10);

            Assert.Collection(consumos,
                c => { Assert.Equal(loteProximo, c.LoteId); Assert.Equal(6, c.Quantidade); },
                c => { Assert.Equal(loteSeguinte, c.LoteId); Assert.Equal(4, c.Quantidade); });
            Assert.Equal(0, Dados.QuantidadeDoLote(loteProximo));
            Assert.Equal(6, Dados.QuantidadeDoLote(loteSeguinte));
            Assert.Equal(6, Dados.EstoqueDoProduto(produto));
        }

        [Fact]
        public void Prioriza_a_validade_e_nao_a_ordem_de_entrada()
        {
            int produto = Dados.NovoProduto();
            // lote que chegou primeiro, mas vence depois
            int loteAntigo = Dados.NovoLote(produto, quantidade: 10, diasValidade: 20, diasEntrada: -5);
            // lote que chegou depois, mas vence antes
            int loteNovo = Dados.NovoLote(produto, quantidade: 10, diasValidade: 5, diasEntrada: 0);

            var consumos = LoteService.ConsumirFEFO(produto, 3);

            var c = Assert.Single(consumos);
            Assert.Equal(loteNovo, c.LoteId);
            Assert.Equal(10, Dados.QuantidadeDoLote(loteAntigo));
        }

        [Fact]
        public void Ignora_lotes_inativos()
        {
            int produto = Dados.NovoProduto();
            int loteInativo = Dados.NovoLote(produto, quantidade: 10, diasValidade: 2, ativo: false);
            int loteAtivo = Dados.NovoLote(produto, quantidade: 10, diasValidade: 9);

            var consumos = LoteService.ConsumirFEFO(produto, 4);

            Assert.Equal(loteAtivo, Assert.Single(consumos).LoteId);
            Assert.Equal(10, Dados.QuantidadeDoLote(loteInativo));
        }

        [Fact]
        public void Ignora_lotes_sem_saldo()
        {
            int produto = Dados.NovoProduto();
            Dados.NovoLote(produto, quantidade: 0, diasValidade: 2);
            int loteComSaldo = Dados.NovoLote(produto, quantidade: 10, diasValidade: 9);

            var consumos = LoteService.ConsumirFEFO(produto, 2);

            Assert.Equal(loteComSaldo, Assert.Single(consumos).LoteId);
        }

        [Fact]
        public void Quantidade_sem_lote_suficiente_fica_registrada_sem_lote()
        {
            int produto = Dados.NovoProduto();
            int lote = Dados.NovoLote(produto, quantidade: 5, diasValidade: 10);

            var consumos = LoteService.ConsumirFEFO(produto, 8);

            Assert.Collection(consumos,
                c => { Assert.Equal(lote, c.LoteId); Assert.Equal(5, c.Quantidade); },
                c => { Assert.Equal(0, c.LoteId); Assert.Equal(3, c.Quantidade); });
            Assert.Equal(0, Dados.EstoqueDoProduto(produto));
        }

        [Fact]
        public void Nao_consome_lote_ja_vencido()
        {
            int produto = Dados.NovoProduto();
            int loteVencido = Dados.NovoLote(produto, quantidade: 5, diasValidade: -1);
            int loteValido = Dados.NovoLote(produto, quantidade: 10, diasValidade: 5);

            var consumos = LoteService.ConsumirFEFO(produto, 3);

            Assert.Equal(loteValido, Assert.Single(consumos).LoteId);
            Assert.Equal(5, Dados.QuantidadeDoLote(loteVencido));
        }

        [Fact]
        public void ListarProximosValidade_traz_so_lotes_na_janela_de_alerta_e_com_saldo()
        {
            int produto = Dados.NovoProduto();
            int vence2 = Dados.NovoLote(produto, quantidade: 4, diasValidade: 2);
            int vence6 = Dados.NovoLote(produto, quantidade: 4, diasValidade: 6);
            Dados.NovoLote(produto, quantidade: 4, diasValidade: 15); // fora da janela
            Dados.NovoLote(produto, quantidade: 0, diasValidade: 5);  // sem saldo

            var lotes = LoteService.ListarProximosValidade();

            Assert.Equal(new[] { vence2, vence6 }, lotes.Select(l => l.Id).ToArray());
        }
    }
}
