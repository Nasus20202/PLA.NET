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

public class ProfessorServiceTests : ServiceTestBase
{
    private readonly Mock<IProfessorRepository> _mockRepo;
    private readonly Mock<IOfficeRepository> _mockOfficeRepo;
    private readonly Mock<IIndexCounterService> _mockIndex;
    private readonly ProfessorService _service;

    public ProfessorServiceTests()
    {
        _mockRepo = new Mock<IProfessorRepository>();
        _mockOfficeRepo = new Mock<IOfficeRepository>();
        _mockIndex = new Mock<IIndexCounterService>();
        _service = new ProfessorService(
            _mockRepo.Object,
            _mockOfficeRepo.Object,
            _mockIndex.Object,
            _mockUnit.Object
        );
    }

    [Fact]
    public async Task CreateProfessorAsync_CreatesAndSaves()
    {
        // Arrange
        _mockIndex.Setup(i => i.GetNextIndexAsync("P", It.IsAny<bool>())).ReturnsAsync("P101");
        _mockRepo
            .Setup(r => r.AddProfessorAsync(It.IsAny<Professor>()))
            .Returns(Task.CompletedTask);

        var address = new Address
        {
            Street = "s",
            City = "c",
            PostalCode = "p",
        };

        // Act
        var prof = await _service.CreateProfessorAsync("A", "B", "Prof", address);

        Assert.Equal("A", prof.FirstName);
        Assert.Equal("P101", prof.UniversityIndex);
        _mockRepo.Verify(r => r.AddProfessorAsync(It.IsAny<Professor>()), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignOfficeAsync_WhenProfessorMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetProfessorByIdAsync(1)).ReturnsAsync((Professor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AssignOfficeAsync(1, "10", "A")
        );
    }

    [Fact]
    public async Task AssignOfficeAsync_WhenHasOffice_Throws()
    {
        // Arrange
        var professor = new Professor
        {
            Id = 1,
            Office = new Office { Id = 5 },
        };
        _mockRepo.Setup(r => r.GetProfessorByIdAsync(1)).ReturnsAsync(professor);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AssignOfficeAsync(1, "10", "A")
        );
    }

    [Fact]
    public async Task AssignOfficeAsync_AddsOfficeWhenValid()
    {
        // Arrange
        var professor = new Professor { Id = 1, Office = null };
        _mockRepo.Setup(r => r.GetProfessorByIdAsync(1)).ReturnsAsync(professor);
        _mockOfficeRepo
            .Setup(r => r.AddOfficeAsync(It.IsAny<Office>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AssignOfficeAsync(1, "10", "A");

        // Assert
        _mockOfficeRepo.Verify(r => r.AddOfficeAsync(It.IsAny<Office>()), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProfessorAsync_WhenMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetProfessorByIdAsync(1)).ReturnsAsync((Professor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteProfessorAsync(1));
    }

    [Fact]
    public async Task DeleteProfessorAsync_WhenExists_DeletesAndDecrements()
    {
        // Arrange
        var professor = new Professor { Id = 1, UniversityIndex = "P200" };
        _mockRepo.Setup(r => r.GetProfessorByIdAsync(1)).ReturnsAsync(professor);
        _mockRepo.Setup(r => r.DeleteProfessorAsync(professor)).Returns(Task.CompletedTask);
        _mockIndex
            .Setup(i => i.TryDecrementIndexAsync("P", "P200", It.IsAny<bool>()))
            .ReturnsAsync(true);

        // Act
        await _service.DeleteProfessorAsync(1);

        // Assert
        _mockIndex.Verify(i => i.TryDecrementIndexAsync("P", "P200", It.IsAny<bool>()), Times.Once);
        _mockRepo.Verify(r => r.DeleteProfessorAsync(professor), Times.Once);
    }

    [Fact]
    public async Task GetProfessorByIdAsync_ReturnsProfessor()
    {
        // Arrange
        var prof = new Professor { Id = 7 };
        _mockRepo.Setup(r => r.GetProfessorByIdAsync(7)).ReturnsAsync(prof);

        // Act
        var res = await _service.GetProfessorByIdAsync(7);

        // Assert
        Assert.Equal(prof, res);
    }

    [Fact]
    public async Task GetAllProfessorsAsync_ReturnsAllProfessors()
    {
        // Arrange
        var professors = new List<Professor>
        {
            new Professor { Id = 1, FirstName = "John" },
            new Professor { Id = 2, FirstName = "Jane" },
        };
        _mockRepo.Setup(r => r.GetAllProfessorsAsync()).ReturnsAsync(professors);

        // Act
        var result = await _service.GetAllProfessorsAsync();

        // Assert
        Assert.Equal(professors, result);
        _mockRepo.Verify(r => r.GetAllProfessorsAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProfessorAsync_CallsUpdateAndSave()
    {
        // Arrange
        var prof = new Professor { Id = 4 };
        _mockRepo.Setup(r => r.UpdateProfessorAsync(prof)).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateProfessorAsync(prof);

        // Assert
        _mockRepo.Verify(r => r.UpdateProfessorAsync(prof), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
