using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class IndexCounterRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddIndexCounter_SaveAndGet()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "Z", CurrentValue = 2 };

        // Act
        await repo.AddIndexCounterAsync(counter);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new IndexCounterRepository(ctx2);
        var fetched = await repo2.GetIndexCounterAsync("Z");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(2, fetched!.CurrentValue);
    }
}
