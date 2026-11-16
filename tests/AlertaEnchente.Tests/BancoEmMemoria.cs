using AlertaEnchente.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AlertaEnchente.Tests;

/// <summary>Banco SQLite em memória, novo para cada teste (rápido e isolado).</summary>
public sealed class BancoEmMemoria : IDisposable
{
    private readonly SqliteConnection _conexao;

    public AppDbContext Contexto { get; }

    public BancoEmMemoria()
    {
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options;
        Contexto = new AppDbContext(opcoes);
        Contexto.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Contexto.Dispose();
        _conexao.Dispose();
    }
}
