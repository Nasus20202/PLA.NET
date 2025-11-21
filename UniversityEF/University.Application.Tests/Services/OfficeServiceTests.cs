using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using University.Application.Interfaces.Repositories;
using University.Application.Services;
using University.Domain.Entities;
using Xunit;

#nullable enable

namespace University.Application.Tests.Services;

public class OfficeServiceTests : ServiceTestBase
{
    private readonly Mock<IOfficeRepository> _mockOfficeRepo;
    private readonly Mock<IProfessorRepository> _mockProfRepo;
    private readonly OfficeService _service;

    public OfficeServiceTests()
    {
        _mockOfficeRepo = new Mock<IOfficeRepository>();
        _mockProfRepo = new Mock<IProfessorRepository>();
        _service = new OfficeService(
            _mockOfficeRepo.Object,
            _mockProfRepo.Object,
            _mockUnit.Object
        );
    }

    [Fact]
    public async Task CreateOfficeAsync_CreatesOffice_WhenProfessorExistsAndHasNoOffice()
    {
        // Arrange
        var professorId = 1;
        var professor = new Professor
        {
            Id = professorId,
            FirstName = "John",
            LastName = "Doe",
            UniversityIndex = "P1",
            AcademicTitle = "Dr",
        };

        _mockProfRepo.Setup(r => r.GetProfessorByIdAsync(professorId)).ReturnsAsync(professor);
        _mockOfficeRepo
            .Setup(r => r.GetOfficeByProfessorIdAsync(professorId))
            .ReturnsAsync((Office?)null);

        // Act
        var result = await _service.CreateOfficeAsync(professorId, "101", "Building A");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(professorId, result.ProfessorId);
        Assert.Equal("101", result.OfficeNumber);
        Assert.Equal("Building A", result.Building);
        _mockOfficeRepo.Verify(r => r.AddOfficeAsync(It.IsAny<Office>()), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateOfficeAsync_ThrowsException_WhenProfessorDoesNotExist()
    {
        // Arrange
        _mockProfRepo
            .Setup(r => r.GetProfessorByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Professor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateOfficeAsync(999, "101", "Building A")
        );
        _mockOfficeRepo.Verify(r => r.AddOfficeAsync(It.IsAny<Office>()), Times.Never);
    }

    [Fact]
    public async Task CreateOfficeAsync_ThrowsException_WhenProfessorAlreadyHasOffice()
    {
        // Arrange
        var professorId = 1;
        var professor = new Professor
        {
            Id = professorId,
            FirstName = "Jane",
            LastName = "Smith",
            UniversityIndex = "P2",
            AcademicTitle = "Prof",
        };
        var existingOffice = new Office
        {
            Id = 1,
            ProfessorId = professorId,
            OfficeNumber = "202",
            Building = "Building B",
        };

        _mockProfRepo.Setup(r => r.GetProfessorByIdAsync(professorId)).ReturnsAsync(professor);
        _mockOfficeRepo
            .Setup(r => r.GetOfficeByProfessorIdAsync(professorId))
            .ReturnsAsync(existingOffice);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateOfficeAsync(professorId, "303", "Building C")
        );
        _mockOfficeRepo.Verify(r => r.AddOfficeAsync(It.IsAny<Office>()), Times.Never);
    }

    [Fact]
    public async Task GetOfficeByIdAsync_ReturnsOffice_WhenExists()
    {
        // Arrange
        var office = new Office
        {
            Id = 1,
            ProfessorId = 1,
            OfficeNumber = "101",
            Building = "Building A",
        };
        _mockOfficeRepo.Setup(r => r.GetOfficeByIdAsync(1)).ReturnsAsync(office);

        // Act
        var result = await _service.GetOfficeByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("101", result.OfficeNumber);
    }

    [Fact]
    public async Task GetOfficeByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        _mockOfficeRepo
            .Setup(r => r.GetOfficeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Office?)null);

        // Act
        var result = await _service.GetOfficeByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOfficeByProfessorIdAsync_ReturnsOffice_WhenExists()
    {
        // Arrange
        var office = new Office
        {
            Id = 1,
            ProfessorId = 5,
            OfficeNumber = "505",
            Building = "Building E",
        };
        _mockOfficeRepo.Setup(r => r.GetOfficeByProfessorIdAsync(5)).ReturnsAsync(office);

        // Act
        var result = await _service.GetOfficeByProfessorIdAsync(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result!.ProfessorId);
        Assert.Equal("505", result.OfficeNumber);
    }

    [Fact]
    public async Task GetAllOfficesAsync_ReturnsAllOffices()
    {
        // Arrange
        var offices = new List<Office>
        {
            new Office
            {
                Id = 1,
                ProfessorId = 1,
                OfficeNumber = "101",
                Building = "A",
            },
            new Office
            {
                Id = 2,
                ProfessorId = 2,
                OfficeNumber = "202",
                Building = "B",
            },
        };
        _mockOfficeRepo.Setup(r => r.GetAllOfficesAsync()).ReturnsAsync(offices);

        // Act
        var result = await _service.GetAllOfficesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, ((List<Office>)result).Count);
    }

    [Fact]
    public async Task UpdateOfficeAsync_UpdatesOffice_WhenExists()
    {
        // Arrange
        var office = new Office
        {
            Id = 1,
            ProfessorId = 1,
            OfficeNumber = "101",
            Building = "A",
        };
        _mockOfficeRepo.Setup(r => r.GetOfficeByIdAsync(1)).ReturnsAsync(office);

        office.OfficeNumber = "102";
        office.Building = "B";

        // Act
        await _service.UpdateOfficeAsync(office);

        // Assert
        _mockOfficeRepo.Verify(r => r.UpdateOfficeAsync(office), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateOfficeAsync_ThrowsException_WhenNotExists()
    {
        // Arrange
        var office = new Office
        {
            Id = 999,
            ProfessorId = 1,
            OfficeNumber = "999",
            Building = "Z",
        };
        _mockOfficeRepo.Setup(r => r.GetOfficeByIdAsync(999)).ReturnsAsync((Office?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateOfficeAsync(office)
        );
        _mockOfficeRepo.Verify(r => r.UpdateOfficeAsync(It.IsAny<Office>()), Times.Never);
    }

    [Fact]
    public async Task DeleteOfficeAsync_DeletesOffice_WhenExists()
    {
        // Arrange
        var office = new Office
        {
            Id = 1,
            ProfessorId = 1,
            OfficeNumber = "101",
            Building = "A",
        };
        _mockOfficeRepo.Setup(r => r.GetOfficeByIdAsync(1)).ReturnsAsync(office);

        // Act
        await _service.DeleteOfficeAsync(1);

        // Assert
        _mockOfficeRepo.Verify(r => r.DeleteOfficeAsync(office), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteOfficeAsync_ThrowsException_WhenNotExists()
    {
        // Arrange
        _mockOfficeRepo
            .Setup(r => r.GetOfficeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Office?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteOfficeAsync(999));
        _mockOfficeRepo.Verify(r => r.DeleteOfficeAsync(It.IsAny<Office>()), Times.Never);
    }
}
