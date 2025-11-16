using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using University.Infrastructure.Data;

namespace University.Infrastructure.Tests.Repositories;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly DbContextOptions<UniversityDbContext> Options;

    public RepositoryTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        Options = new DbContextOptionsBuilder<UniversityDbContext>().UseSqlite(_connection).Options;

        using var context = new UniversityDbContext(Options);
        context.Database.EnsureCreated();
    }

    protected UniversityDbContext NewContext() => new UniversityDbContext(Options);

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
