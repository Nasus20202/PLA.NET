using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using University.Domain.Entities;
using University.Infrastructure.Data;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class UniversityRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<UniversityDbContext> _options;

    public UniversityRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<UniversityDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new UniversityDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private UniversityRepository CreateRepository()
    {
        var context = new UniversityDbContext(_options);
        return new UniversityRepository(context);
    }

    [Fact]
    public async Task AddStudent_SaveAndGetById_ReturnsStudent()
    {
        // Arrange
        var repo = CreateRepository();
        var student = new Student
        {
            FirstName = "A",
            LastName = "B",
            UniversityIndex = "S1000",
        };

        // Act
        await repo.AddStudentAsync(student);
        await repo.SaveChangesAsync();

        var fetched = await repo.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(student.FirstName, fetched!.FirstName);
    }

    [Fact]
    public async Task AddIndexCounter_SaveAndGet_Roundtrip()
    {
        // Arrange
        var repo = CreateRepository();
        var counter = new IndexCounter { Prefix = "T", CurrentValue = 1 };

        // Act
        await repo.AddIndexCounterAsync(counter);
        await repo.SaveChangesAsync();

        var fetched = await repo.GetIndexCounterAsync("T");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(1, fetched!.CurrentValue);
    }

    [Fact]
    public async Task Transaction_BeginCommitPersists()
    {
        // Arrange
        var repo = CreateRepository();
        var counter = new IndexCounter { Prefix = "R", CurrentValue = 5 };
        await repo.AddIndexCounterAsync(counter);
        await repo.SaveChangesAsync();

        // Act
        await repo.BeginTransactionAsync();
        var c = await repo.GetIndexCounterAsync("R");
        c!.CurrentValue++;
        await repo.UpdateIndexCounterAsync(c);
        await repo.SaveChangesAsync();
        await repo.CommitTransactionAsync();

        // Assert
        var newRepo = CreateRepository();
        var reFetched = await newRepo.GetIndexCounterAsync("R");
        Assert.Equal(6, reFetched!.CurrentValue);
    }

    [Fact]
    public async Task Transaction_RollbackDoesNotPersist()
    {
        // Arrange
        var repo = CreateRepository();
        var counter = new IndexCounter { Prefix = "R2", CurrentValue = 7 };
        await repo.AddIndexCounterAsync(counter);
        await repo.SaveChangesAsync();

        // Act
        await repo.BeginTransactionAsync();
        var c = await repo.GetIndexCounterAsync("R2");
        c!.CurrentValue = 100;
        await repo.UpdateIndexCounterAsync(c);
        await repo.SaveChangesAsync();
        await repo.RollbackTransactionAsync();

        // Assert
        var newRepo = CreateRepository();
        var reFetched = await newRepo.GetIndexCounterAsync("R2");
        Assert.Equal(7, reFetched!.CurrentValue);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ReturnsProjectedStudents()
    {
        // Arrange
        var repo = CreateRepository();
        var student = new Student { FirstName = "X" };
        await repo.AddStudentAsync(student);
        await repo.SaveChangesAsync();

        // Act
        var res = await repo.ExecuteQueryAsync(students => students.Select(s => s.FirstName));

        // Assert
        Assert.Contains("X", res);
    }
}
