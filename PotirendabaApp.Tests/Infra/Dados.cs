using System;
using System.Linq;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Tests.Infra
{
    /// <summary>Atalhos para preparar os dados dos testes.</summary>
    public static class Dados
    {
        /// <summary>Data no formato ISO 8601 (yyyy-MM-dd), "dias" a partir de hoje (negativo = passado).</summary>
        public static string Dia(int dias) => DateTime.Today.AddDays(dias).ToString("yyyy-MM-dd");

        public static int NovoProduto(string nome = "Iogurte 170 g")
            => ProdutoService.Inserir(new Produto
            {
                Nome = nome,
                Data = DateTime.Today.ToString("dd/MM/yyyy"),
                Valor = 3.50m,
                Estoque = 0,
                Ativo = true
            });

        public static int NovoLote(int produtoId, int quantidade, int diasValidade,
                                   bool ativo = true, int diasEntrada = 0)
            => LoteService.Inserir(new Lote
            {
                ProdutoId = produtoId,
                Quantidade = quantidade,
                DataValidade = Dia(diasValidade),
                DataEntrada = Dia(diasEntrada),
                Ativo = ativo
            });

        public static int QuantidadeDoLote(int loteId)
            => LoteService.Listar(somenteAtivos: false).Single(l => l.Id == loteId).Quantidade;

        public static int EstoqueDoProduto(int produtoId)
            => ProdutoService.BuscarPorId(produtoId).Estoque;
    }
}
