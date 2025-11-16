using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests : RepositoryTestBase
{
    [Fact]
    public async Task TransactionCommit_Persists()
    {
        // Arrange
        using var ctx = NewContext();
        var unit = new UnitOfWork(ctx);
        await unit.BeginTransactionAsync();
        var student = new Student { FirstName = "T" };
        ctx.Students.Add(student);

        // Act
        await unit.SaveChangesAsync();
        await unit.CommitTransactionAsync();

        // Assert
        using var ctx2 = NewContext();
        var fetched = await ctx2.Students.FindAsync(student.Id);
        Assert.NotNull(fetched);
    }

    [Fact]
    public async Task TransactionRollback_DoesNotPersist()
    {
        // Arrange
        using var ctx = NewContext();
        var unit = new UnitOfWork(ctx);
        await unit.BeginTransactionAsync();
        var student = new Student { FirstName = "TR" };
        ctx.Students.Add(student);

        // Act
        await unit.SaveChangesAsync();
        await unit.RollbackTransactionAsync();

        // Assert
        using var ctx2 = NewContext();
        var fetched = await ctx2.Students.FindAsync(student.Id);
        Assert.Null(fetched);
    }
}
