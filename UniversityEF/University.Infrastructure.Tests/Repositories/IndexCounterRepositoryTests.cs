using System;
using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class IndexCounterRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetCounterAsync_ReturnsCounter_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "Z", CurrentValue = 2 };
        await repo.AddCounterAsync(counter);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new IndexCounterRepository(ctx2);

        // Act
        var fetched = await repo2.GetCounterAsync("Z");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Z", fetched!.Prefix);
        Assert.Equal(2, fetched.CurrentValue);
    }

    [Fact]
    public async Task GetCounterAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);

        // Act
        var fetched = await repo.GetCounterAsync("NONEXISTENT");

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllIndexCountersAsync_ReturnsAllCounters()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter1 = new IndexCounter { Prefix = "A", CurrentValue = 100 };
        var counter2 = new IndexCounter { Prefix = "B", CurrentValue = 200 };
        await repo.AddCounterAsync(counter1);
        await repo.AddCounterAsync(counter2);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new IndexCounterRepository(ctx2);

        // Act
        var allCounters = (await repo2.GetAllIndexCountersAsync()).ToList();

        // Assert
        Assert.Equal(2, allCounters.Count);
        Assert.Contains(allCounters, c => c.Prefix == "A");
        Assert.Contains(allCounters, c => c.Prefix == "B");
    }

    [Fact]
    public async Task AddCounterAsync_AddsCounter()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "T", CurrentValue = 500 };

        // Act
        await repo.AddCounterAsync(counter);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new IndexCounterRepository(ctx2);
        var fetched = await repo2.GetCounterAsync("T");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("T", fetched!.Prefix);
        Assert.Equal(500, fetched.CurrentValue);
    }

    [Fact]
    public async Task UpdateIndexCounterAsync_UpdatesCounter()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "U", CurrentValue = 10 };
        await repo.AddCounterAsync(counter);
        await ctx.SaveChangesAsync();

        // Act
        counter.CurrentValue = 20;
        await repo.UpdateIndexCounterAsync(counter);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new IndexCounterRepository(ctx2);
        var fetched = await repo2.GetCounterAsync("U");

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(20, fetched!.CurrentValue);
    }

    [Fact]
    public async Task DeleteIndexCounterAsync_DeletesCounter()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "D", CurrentValue = 100 };
        await repo.AddCounterAsync(counter);
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteIndexCounterAsync(counter);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new IndexCounterRepository(ctx2);
        var fetched = await repo2.GetCounterAsync("D");

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task ReserveBatchAsync_ReturnsCorrectRange_AndUpdatesCounter()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "T", CurrentValue = 100 };
        await repo.AddCounterAsync(counter);
        await ctx.SaveChangesAsync();

        // Act
        var (startIndex, endIndex) = await repo.ReserveBatchAsync("T", 10);
        await ctx.SaveChangesAsync();

        // Assert
        Assert.Equal(101, startIndex);
        Assert.Equal(110, endIndex);

        // Verify counter was updated
        var updatedCounter = await repo.GetCounterAsync("T");
        Assert.Equal(110, updatedCounter!.CurrentValue);
    }

    [Fact]
    public async Task ReserveBatchAsync_ThrowsException_WhenCounterDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.ReserveBatchAsync("NONEXISTENT", 5)
        );
    }

    [Fact]
    public async Task ReserveBatchAsync_ConsecutiveCalls_ReserveNonOverlappingRanges()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new IndexCounterRepository(ctx);
        var counter = new IndexCounter { Prefix = "C", CurrentValue = 50 };
        await repo.AddCounterAsync(counter);
        await ctx.SaveChangesAsync();

        // Act - First reservation
        var (start1, end1) = await repo.ReserveBatchAsync("C", 5);
        await ctx.SaveChangesAsync();

        // Act - Second reservation
        var (start2, end2) = await repo.ReserveBatchAsync("C", 3);
        await ctx.SaveChangesAsync();

        // Assert
        Assert.Equal(51, start1);
        Assert.Equal(55, end1);
        Assert.Equal(56, start2);
        Assert.Equal(58, end2);

        // Verify final counter value
        var finalCounter = await repo.GetCounterAsync("C");
        Assert.Equal(58, finalCounter!.CurrentValue);
    }
}
