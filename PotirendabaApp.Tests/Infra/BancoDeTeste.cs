using System;
using System.IO;
using Microsoft.Data.Sqlite;
using PotirendabaApp.Data;

namespace PotirendabaApp.Tests.Infra
{
    /// <summary>
    /// Cria um banco SQLite novo e vazio para cada teste de integração.
    /// O DatabaseHelper usa "Data Source=potirendaba.db", um caminho relativo à
    /// pasta de trabalho do processo. Trocando a pasta de trabalho para uma pasta
    /// temporária, cada teste usa o seu próprio banco, sem tocar no banco real
    /// e sem nenhuma alteração no código do sistema.
    /// </summary>
    public sealed class BancoDeTeste : IDisposable
    {
        private readonly string _pastaOriginal;

        public string Pasta { get; }
        public string CaminhoBanco => Path.Combine(Pasta, "potirendaba.db");

        public BancoDeTeste()
        {
            _pastaOriginal = Directory.GetCurrentDirectory();
            Pasta = Path.Combine(Path.GetTempPath(), "potirendaba_teste_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Pasta);
            Directory.SetCurrentDirectory(Pasta);
            DatabaseHelper.InicializarBanco();
        }

        /// <summary>Conexão direta ao banco do teste, para conferir tabelas que o sistema não expõe (ex.: ItensVenda.LoteId).</summary>
        public SqliteConnection AbrirConexao()
        {
            var con = new SqliteConnection("Data Source=" + CaminhoBanco);
            con.Open();
            return con;
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_pastaOriginal);
            SqliteConnection.ClearAllPools(); // libera o arquivo para poder apagar
            try { Directory.Delete(Pasta, recursive: true); } catch { /* pasta temporária: ignorar */ }
        }
    }
}
