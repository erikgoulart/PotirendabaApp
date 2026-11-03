using System.Collections.Generic;
using PotirendabaApp.Models;
using PotirendabaApp.Services;
using Xunit;

namespace PotirendabaApp.Tests.Unitarios
{
    /// <summary>Testes unitários do cálculo de itens e da validação da venda (VendaService).</summary>
    [Trait("Categoria", "Unitario")]
    public class VendaServiceCalculoTests
    {
        [Theory]
        [InlineData("", 20.00)]     // sem desconto
        [InlineData("10%", 18.00)]  // desconto percentual
        [InlineData("5", 15.00)]    // desconto em valor
        [InlineData("5,50", 14.50)] // vírgula como separador decimal
        [InlineData("50", 0.00)]    // desconto maior que o valor não deixa o item negativo
        [InlineData("abc", 20.00)]  // texto inválido é ignorado
        public void CalcularValorItem_aplica_o_desconto_informado(string desconto, double esperado)
        {
            decimal valor = VendaService.CalcularValorItem(10.00m, 2, desconto);

            Assert.Equal((decimal)esperado, valor);
        }

        [Fact]
        public void Validar_exige_pelo_menos_um_item()
        {
            var r = VendaService.Validar(new List<ItemVenda>(), "Aluno", "Pix");

            Assert.False(r.Valido);
            Assert.Equal("itens", r.Campo);
        }

        [Fact]
        public void Validar_exige_aluno()
        {
            var r = VendaService.Validar(UmItem(), "", "Pix");

            Assert.False(r.Valido);
            Assert.Equal("aluno", r.Campo);
        }

        [Fact]
        public void Validar_exige_forma_de_pagamento()
        {
            var r = VendaService.Validar(UmItem(), "Aluno", " ");

            Assert.False(r.Valido);
            Assert.Equal("pagamento", r.Campo);
        }

        [Fact]
        public void Validar_aceita_venda_completa()
        {
            var r = VendaService.Validar(UmItem(), "Aluno", "Pix");

            Assert.True(r.Valido);
        }

        private static List<ItemVenda> UmItem() => new()
        {
            new ItemVenda { ProdutoId = 1, NomeProduto = "Iogurte", Quantidade = 1, ValorUnit = 3.50m }
        };
    }
}
