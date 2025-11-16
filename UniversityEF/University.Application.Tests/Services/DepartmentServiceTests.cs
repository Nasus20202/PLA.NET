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

public class DepartmentServiceTests : ServiceTestBase
{
    private readonly Mock<IDepartmentRepository> _mockRepo;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _mockRepo = new Mock<IDepartmentRepository>();
        _service = new DepartmentService(_mockRepo.Object, _mockUnit.Object);
    }

    [Fact]
    public async Task CreateDepartmentAsync_CreatesAndSaves()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.AddDepartmentAsync(It.IsAny<Department>()))
            .Returns(Task.CompletedTask);
        // Act
        var res = await _service.CreateDepartmentAsync("Dep");

        Assert.Equal("Dep", res.Name);
        _mockRepo.Verify(r => r.AddDepartmentAsync(It.IsAny<Department>()), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WhenMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetDepartmentByIdAsync(1)).ReturnsAsync((Department?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteDepartmentAsync(1)
        );
    }

    [Fact]
    public async Task UpdateDepartmentAsync_CallsUpdateAndSave()
    {
        // Arrange
        var dep = new Department { Id = 1, Name = "X" };
        _mockRepo.Setup(r => r.UpdateDepartmentAsync(dep)).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateDepartmentAsync(dep);

        // Assert
        _mockRepo.Verify(r => r.UpdateDepartmentAsync(dep), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ReturnsDepartment()
    {
        // Arrange
        var dep = new Department { Id = 10, Name = "D" };
        _mockRepo.Setup(r => r.GetDepartmentByIdAsync(10)).ReturnsAsync(dep);

        // Act
        var res = await _service.GetDepartmentByIdAsync(10);

        Assert.Equal(dep, res);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsAll()
    {
        // Arrange
        var list = new List<Department> { new Department { Id = 1 } };
        _mockRepo.Setup(r => r.GetAllDepartmentsAsync()).ReturnsAsync(list);

        // Act
        var res = await _service.GetAllDepartmentsAsync();

        Assert.Equal(list, res);
    }
}
