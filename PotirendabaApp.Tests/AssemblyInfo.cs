using Xunit;

// Os testes de integração trocam a pasta de trabalho do processo para usar um
// banco SQLite temporário. Por isso os testes rodam um de cada vez.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
