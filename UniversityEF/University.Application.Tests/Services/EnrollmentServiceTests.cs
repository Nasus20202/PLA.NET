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

public class EnrollmentServiceTests : ServiceTestBase
{
    private readonly Mock<IEnrollmentRepository> _mockRepo;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _mockRepo = new Mock<IEnrollmentRepository>();
        _service = new EnrollmentService(_mockRepo.Object, _mockUnit.Object);
    }

    [Fact]
    public async Task EnrollStudentAsync_WhenAlreadyEnrolled_Throws()
    {
        // Arrange
        var existing = new Enrollment { CourseId = 1 };
        _mockRepo
            .Setup(r => r.GetEnrollmentsByStudentIdAsync(10))
            .ReturnsAsync(new List<Enrollment> { existing });
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.EnrollStudentAsync(10, 1, 1)
        );
    }

    [Fact]
    public async Task EnrollStudentAsync_AddsAndSaves_WhenNotEnrolled()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetEnrollmentsByStudentIdAsync(10))
            .ReturnsAsync(new List<Enrollment>());
        _mockRepo
            .Setup(r => r.AddEnrollmentAsync(It.IsAny<Enrollment>()))
            .Returns(Task.CompletedTask);
        // Act
        var res = await _service.EnrollStudentAsync(10, 1, 1);

        Assert.Equal(10, res.StudentId);
        Assert.Equal(1, res.CourseId);
        _mockRepo.Verify(r => r.AddEnrollmentAsync(It.IsAny<Enrollment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGradeAsync_WhenMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetEnrollmentByIdAsync(5)).ReturnsAsync((Enrollment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateGradeAsync(5, 4.5)
        );
    }

    [Fact]
    public async Task UpdateGradeAsync_UpdatesGradeAndSaves()
    {
        // Arrange
        var enrollment = new Enrollment { Id = 5, Grade = null };
        _mockRepo.Setup(r => r.GetEnrollmentByIdAsync(5)).ReturnsAsync(enrollment);
        _mockRepo
            .Setup(r => r.UpdateEnrollmentAsync(It.IsAny<Enrollment>()))
            .Returns(Task.CompletedTask);
        // Act
        await _service.UpdateGradeAsync(5, 4.5);

        Assert.Equal(4.5, enrollment.Grade);
        _mockRepo.Verify(
            r => r.UpdateEnrollmentAsync(It.Is<Enrollment>(en => en.Grade == 4.5)),
            Times.Once
        );
        _mockUnit.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnenrollStudentAsync_WhenMissing_Throws()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetEnrollmentByIdAsync(99)).ReturnsAsync((Enrollment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UnenrollStudentAsync(99)
        );
    }

    [Fact]
    public async Task UnenrollStudentAsync_DeletesAndSaves()
    {
        // Arrange
        var enrollment = new Enrollment { Id = 2 };
        _mockRepo.Setup(r => r.GetEnrollmentByIdAsync(2)).ReturnsAsync(enrollment);
        _mockRepo.Setup(r => r.DeleteEnrollmentAsync(enrollment)).Returns(Task.CompletedTask);

        // Act
        await _service.UnenrollStudentAsync(2);

        // Assert
        _mockRepo.Verify(r => r.DeleteEnrollmentAsync(enrollment), Times.Once);
    }

    [Fact]
    public async Task GetStudentEnrollmentsAsync_ReturnsEnrollments()
    {
        // Arrange
        var list = new List<Enrollment> { new Enrollment { Id = 11 } };
        _mockRepo.Setup(r => r.GetEnrollmentsByStudentIdAsync(10)).ReturnsAsync(list);

        // Act
        var res = await _service.GetStudentEnrollmentsAsync(10);

        Assert.Equal(list, res);
    }

    [Fact]
    public async Task GetCourseEnrollmentsAsync_ReturnsEnrollments()
    {
        // Arrange
        var list = new List<Enrollment> { new Enrollment { Id = 11 } };
        _mockRepo.Setup(r => r.GetEnrollmentsByCourseIdAsync(1)).ReturnsAsync(list);

        // Act
        var res = await _service.GetCourseEnrollmentsAsync(1);

        Assert.Equal(list, res);
    }
}
