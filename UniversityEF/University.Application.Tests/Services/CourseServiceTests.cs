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

public class CourseServiceTests : ServiceTestBase
{
    private readonly Mock<ICourseRepository> _mockRepo;
    private readonly CourseService _service;

    public CourseServiceTests()
    {
        _mockRepo = new Mock<ICourseRepository>();
        _service = new CourseService(_mockRepo.Object, _mockUnit.Object);
    }

    [Fact]
    public async Task CreateCourseAsync_CreatesAndSaves()
    {
        // Arrange
        _mockRepo.Setup(r => r.AddCourseAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);

        // Act
        var course = await _service.CreateCourseAsync("DB", "DB101", 5, 1, null);

        Assert.Equal("DB", course.Name);
        _mockRepo.Verify(r => r.AddCourseAsync(It.IsAny<Course>()), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_WhenMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetCourseByIdAsync(1)).ReturnsAsync((Course?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteCourseAsync(1));
    }

    [Fact]
    public async Task AddPrerequisiteAsync_AddsAndSaves_WhenNotPresent()
    {
        // Arrange
        var course = new Course { Id = 1, Prerequisites = new List<Course>() };
        var prereq = new Course { Id = 2 };
        _mockRepo.Setup(r => r.GetCourseByIdAsync(1)).ReturnsAsync(course);
        _mockRepo.Setup(r => r.GetCourseByIdAsync(2)).ReturnsAsync(prereq);
        _mockRepo.Setup(r => r.UpdateCourseAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);

        // Act
        await _service.AddPrerequisiteAsync(1, 2);

        Assert.Contains(prereq, course.Prerequisites);
        _mockRepo.Verify(r => r.UpdateCourseAsync(course), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCourseById_ReturnsCourse()
    {
        // Arrange
        var course = new Course { Id = 3, Name = "X" };
        _mockRepo.Setup(r => r.GetCourseByIdAsync(3)).ReturnsAsync(course);

        // Act
        var res = await _service.GetCourseByIdAsync(3);

        Assert.Equal(course, res);
    }

    [Fact]
    public async Task GetAllCoursesAsync_ReturnsAll()
    {
        // Arrange
        var list = new List<Course> { new Course { Id = 1 } };
        _mockRepo.Setup(r => r.GetAllCoursesAsync()).ReturnsAsync(list);

        // Act
        var res = await _service.GetAllCoursesAsync();

        Assert.Equal(list, res);
    }

    [Fact]
    public async Task UpdateCourseAsync_CallsUpdateAndSave()
    {
        // Arrange
        var course = new Course { Id = 2 };
        _mockRepo.Setup(r => r.UpdateCourseAsync(course)).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateCourseAsync(course);

        // Assert
        _mockRepo.Verify(r => r.UpdateCourseAsync(course), Times.Once);
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddPrerequisiteAsync_WhenCourseOrPrereqMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetCourseByIdAsync(1)).ReturnsAsync((Course?)null);
        _mockRepo.Setup(r => r.GetCourseByIdAsync(2)).ReturnsAsync((Course?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddPrerequisiteAsync(1, 2)
        );
    }

    [Fact]
    public async Task RemovePrerequisiteAsync_RemovesAndSaves_WhenPresent()
    {
        // Arrange
        var prereq = new Course { Id = 2 };
        var course = new Course
        {
            Id = 1,
            Prerequisites = new List<Course> { prereq },
        };

        _mockRepo.Setup(r => r.GetCourseByIdAsync(1)).ReturnsAsync(course);
        _mockRepo.Setup(r => r.GetCourseByIdAsync(2)).ReturnsAsync(prereq);
        _mockRepo.Setup(r => r.UpdateCourseAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);

        // Act
        await _service.RemovePrerequisiteAsync(1, 2);

        // Assert
        Assert.DoesNotContain(prereq, course.Prerequisites);
        _mockRepo.Verify(r => r.UpdateCourseAsync(course), Times.Once);
    }
}
