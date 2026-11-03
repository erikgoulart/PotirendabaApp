using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using PotirendabaApp.Models;
using PotirendabaApp.Services;
using PotirendabaApp.Tests.Infra;
using Xunit;

namespace PotirendabaApp.Tests.Integracao
{
    /// <summary>
    /// Testes de integração do estoque derivado (TC01) e da gravação da venda
    /// com rastreabilidade por lote (VendaService.Finalizar + ItensVenda.LoteId).
    /// </summary>
    [Trait("Categoria", "Integracao")]
    public class EstoqueEVendaTests : IDisposable
    {
        private readonly BancoDeTeste _banco = new BancoDeTeste();
        public void Dispose() => _banco.Dispose();

        [Fact] // TC01
        public void Estoque_do_produto_e_a_soma_dos_lotes_ativos()
        {
            int produto = Dados.NovoProduto();

            Dados.NovoLote(produto, quantidade: 10, diasValidade: 30);
            Assert.Equal(10, Dados.EstoqueDoProduto(produto));

            Dados.NovoLote(produto, quantidade: 5, diasValidade: 20);
            Assert.Equal(15, Dados.EstoqueDoProduto(produto));

            Dados.NovoLote(produto, quantidade: 7, diasValidade: 20, ativo: false);
            Assert.Equal(15, Dados.EstoqueDoProduto(produto)); // lote inativo não conta
        }

        [Fact]
        public void Desativar_um_lote_retira_o_saldo_dele_do_estoque()
        {
            int produto = Dados.NovoProduto();
            int lote = Dados.NovoLote(produto, quantidade: 10, diasValidade: 30);
            Dados.NovoLote(produto, quantidade: 5, diasValidade: 20);

            var l = LoteService.Listar(produto).Find(x => x.Id == lote);
            l.Ativo = false;
            LoteService.Atualizar(l);

            Assert.Equal(5, Dados.EstoqueDoProduto(produto));
        }

        [Fact]
        public void Finalizar_venda_grava_uma_linha_por_lote_consumido_com_LoteId()
        {
            int produto = Dados.NovoProduto();
            int loteProximo = Dados.NovoLote(produto, quantidade: 6, diasValidade: 3);
            int loteSeguinte = Dados.NovoLote(produto, quantidade: 10, diasValidade: 10);
            var itens = new List<ItemVenda>
            {
                new ItemVenda { ProdutoId = produto, NomeProduto = "Iogurte 170 g", Quantidade = 10, ValorUnit = 3.50m, Desconto = 0m }
            };

            int vendaId = VendaService.Finalizar(itens, "Aluno Teste", "Pix");

            var linhas = LerItensVenda(vendaId);
            Assert.Collection(linhas,
                x => { Assert.Equal(loteProximo, x.LoteId); Assert.Equal(6, x.Quantidade); },
                x => { Assert.Equal(loteSeguinte, x.LoteId); Assert.Equal(4, x.Quantidade); });
            Assert.Equal(6, Dados.EstoqueDoProduto(produto));
        }

        private List<(int? LoteId, int Quantidade)> LerItensVenda(int vendaId)
        {
            var lista = new List<(int? LoteId, int Quantidade)>();
            using var con = _banco.AbrirConexao();
            using var cmd = new SqliteCommand(
                "SELECT LoteId, Quantidade FROM ItensVenda WHERE VendaId=@V ORDER BY Id;", con);
            cmd.Parameters.AddWithValue("@V", vendaId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add((r.IsDBNull(0) ? (int?)null : r.GetInt32(0), r.GetInt32(1)));
            return lista;
        }
    }
}
