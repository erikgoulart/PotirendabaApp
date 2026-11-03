# PotirendabaApp

Sistema desktop de controle de estoque, lotes e validade para a cantina da Escola Maestro Antonio Amato, em Potirendaba (SP).

Desenvolvido como Trabalho de Conclusão de Curso em Ciência da Computação na Universidade Paulista (UNIP), campus São José do Rio Preto, 2026.

**Autores:** Erik Carobolante Goulart e Julio Cesar Satim
**Orientador:** Prof. Me. Cícero Oliveira

## O que o sistema faz

- Cadastro de produtos, alunos e vendas
- Cada remessa recebida é registrada como um **lote**, com quantidade, data de entrada e data de validade
- O estoque do produto é calculado pela soma dos lotes ativos
- Na venda, a baixa segue o método **FEFO** (*First Expired, First Out*): sai primeiro o lote que vence primeiro, e lotes vencidos não são vendidos
- Cada item vendido fica ligado ao lote de origem (rastreabilidade)
- Alerta de validade ao abrir o sistema e destaque por cores na tela de lotes:
  - **4 a 7 dias** para vencer: sugestão de promoção
  - **3 dias ou menos**, ou vencido: sugestão de descarte

## Tecnologias

- C# e .NET 8
- Windows Forms
- SQLite (Microsoft.Data.Sqlite)
- xUnit para testes automatizados

## Estrutura

```
PotirendabaApp/
├── Data/          acesso ao banco SQLite (DatabaseHelper)
├── Models/        classes de dados (Produto, Lote, Venda, Aluno)
├── Services/      regras de negócio (LoteService, VendaService...)
├── Forms/         telas do sistema
└── PotirendabaApp.Tests/
    ├── Unitarios/     23 testes sem banco de dados
    └── Integracao/    11 testes com banco SQLite temporário
```

## Como executar

Requer o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) no Windows.

```
dotnet run --project PotirendabaApp.csproj
```

O banco `potirendaba.db` é criado automaticamente na primeira execução.

## Como rodar os testes

```
dotnet test PotirendabaApp.Tests
```

Os testes criam um banco temporário para cada caso e não alteram o banco real.

## Como gerar o executável

Gera um único `.exe` que roda em qualquer Windows 64 bits, sem precisar instalar o .NET:

```
dotnet publish PotirendabaApp.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none
```

O arquivo fica em `bin/Release/net8.0-windows/win-x64/publish/`.
