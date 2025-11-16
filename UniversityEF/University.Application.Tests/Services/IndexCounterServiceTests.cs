using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Application.Services;
using University.Domain.Entities;
using Xunit;

#nullable enable

namespace University.Application.Tests.Services;

public class IndexCounterServiceTests
{
    private readonly Mock<IIndexCounterRepository> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUnit;
    private readonly IndexCounterService _service;

    public IndexCounterServiceTests()
    {
        _mockRepo = new Mock<IIndexCounterRepository>();
        _mockUnit = new Mock<IUnitOfWork>();
        _service = new IndexCounterService(_mockRepo.Object, _mockUnit.Object);

        // Default behaviors for transaction-related calls
        _mockUnit.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnit.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnit.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnit.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task GetNextIndexAsync_ShouldIncrementAndReturnNextIndex_WhenCounterExists()
    {
        // Arrange
        var prefix = "S";
        var counter = new IndexCounter { Prefix = prefix, CurrentValue = 100 };
        _mockRepo.Setup(r => r.GetCounterAsync(prefix)).ReturnsAsync(counter);

        // Act
        var result = await _service.GetNextIndexAsync(prefix);

        // Assert
        Assert.Equal("S101", result);
        _mockRepo.Verify(
            r => r.UpdateIndexCounterAsync(It.Is<IndexCounter>(c => c.CurrentValue == 101)),
            Times.Once
        );
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockUnit.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _mockUnit.Verify(u => u.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task GetNextIndexAsync_ShouldThrowInvalidOperation_WhenCounterNotFound()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetCounterAsync(It.IsAny<string>()))
            .ReturnsAsync((IndexCounter?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetNextIndexAsync("S"));
        _mockUnit.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task TryDecrementIndexAsync_ShouldReturnFalse_WhenCounterNotFound()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetCounterAsync(It.IsAny<string>()))
            .ReturnsAsync((IndexCounter?)null);

        // Act
        var result = await _service.TryDecrementIndexAsync("S", "S101");

        // Assert
        Assert.False(result);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Never);
        _mockUnit.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task TryDecrementIndexAsync_ShouldReturnFalse_WhenCurrentIndexNotParsable()
    {
        // Arrange
        var prefix = "S";
        var counter = new IndexCounter { Prefix = prefix, CurrentValue = 100 };
        _mockRepo.Setup(r => r.GetCounterAsync(prefix)).ReturnsAsync(counter);

        // Act
        var result = await _service.TryDecrementIndexAsync(prefix, "Sabc");

        // Assert
        Assert.False(result);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Never);
        _mockUnit.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task TryDecrementIndexAsync_ShouldDecrementAndReturnTrue_WhenMatchesCurrentValue()
    {
        // Arrange
        var prefix = "S";
        var counter = new IndexCounter { Prefix = prefix, CurrentValue = 101 };
        _mockRepo.Setup(r => r.GetCounterAsync(prefix)).ReturnsAsync(counter);

        // Act
        var result = await _service.TryDecrementIndexAsync(prefix, "S101");

        // Assert
        Assert.True(result);
        _mockRepo.Verify(
            r => r.UpdateIndexCounterAsync(It.Is<IndexCounter>(c => c.CurrentValue == 100)),
            Times.Once
        );
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockUnit.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeCounterAsync_ShouldThrow_WhenCounterExists()
    {
        // Arrange
        var prefix = "S";
        _mockRepo
            .Setup(r => r.GetCounterAsync(prefix))
            .ReturnsAsync(new IndexCounter { Prefix = prefix, CurrentValue = 1 });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.InitializeCounterAsync(prefix, 1)
        );
    }

    [Fact]
    public async Task InitializeCounterAsync_ShouldAddCounterAndSave_WhenNotExists()
    {
        // Arrange
        var prefix = "S";
        _mockRepo.Setup(r => r.GetCounterAsync(prefix)).ReturnsAsync((IndexCounter?)null);

        // Act
        await _service.InitializeCounterAsync(prefix, 1);

        // Assert
        _mockRepo.Verify(
            r =>
                r.AddCounterAsync(
                    It.Is<IndexCounter>(c => c.Prefix == prefix && c.CurrentValue == 1)
                ),
            Times.Once
        );
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCounterAsync_ShouldReturnCounter()
    {
        // Arrange
        var prefix = "S";
        var counter = new IndexCounter { Prefix = prefix, CurrentValue = 50 };
        _mockRepo.Setup(r => r.GetCounterAsync(prefix)).ReturnsAsync(counter);

        // Act
        var result = await _service.GetCounterAsync(prefix);

        // Assert
        Assert.Equal(counter, result);
    }

    [Fact]
    public async Task GetAllCountersAsync_ShouldReturnAllCounters()
    {
        // Arrange
        var counters = new List<IndexCounter>
        {
            new IndexCounter { Prefix = "S", CurrentValue = 1 },
            new IndexCounter { Prefix = "P", CurrentValue = 2 },
        };
        _mockRepo.Setup(r => r.GetAllIndexCountersAsync()).ReturnsAsync(counters);

        // Act
        var result = await _service.GetAllCountersAsync();

        // Assert
        Assert.Equal(counters, result);
    }

    [Fact]
    public async Task DeleteCounterAsync_ShouldDeleteAndSave_WhenExists()
    {
        // Arrange
        var prefix = "S";
        var counter = new IndexCounter { Prefix = prefix, CurrentValue = 1 };
        _mockRepo.Setup(r => r.GetCounterAsync(prefix)).ReturnsAsync(counter);

        // Act
        await _service.DeleteCounterAsync(prefix);

        // Assert
        _mockRepo.Verify(r => r.DeleteIndexCounterAsync(counter), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCounterAsync_ShouldNotSave_WhenNotExists()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetCounterAsync(It.IsAny<string>()))
            .ReturnsAsync((IndexCounter?)null);

        // Act
        await _service.DeleteCounterAsync("S");

        // Assert
        _mockRepo.Verify(r => r.DeleteIndexCounterAsync(It.IsAny<IndexCounter>()), Times.Never);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
