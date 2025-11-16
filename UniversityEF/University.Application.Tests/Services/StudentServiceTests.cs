using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Application.Services;
using University.Domain.Entities;
using Xunit;

namespace University.Application.Tests.Services;

public class StudentServiceTests : ServiceTestBase
{
    [Fact]
    public async Task CreateStudentAsync_ShouldAssignIndexAndSave()
    {
        // Arrange
        var indexSvc = new Mock<IIndexCounterService>();
        var studentRepo = new Mock<IStudentRepository>();

        indexSvc.Setup(i => i.GetNextIndexAsync("S", It.IsAny<bool>())).ReturnsAsync("S1001");

        var svc = new StudentService(studentRepo.Object, indexSvc.Object, _mockUnit.Object);

        // Act
        var student = await svc.CreateStudentAsync("John", "Doe", 1, new Address());

        // Assert
        Assert.Equal("S1001", student.UniversityIndex);
        studentRepo.Verify(r => r.AddStudentAsync(It.IsAny<Student>()), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateMasterStudentAsync_ShouldAssignIndexAndSupervisor()
    {
        // Arrange
        var indexSvc = new Mock<IIndexCounterService>();
        var studentRepo = new Mock<IStudentRepository>();

        indexSvc.Setup(i => i.GetNextIndexAsync("S", It.IsAny<bool>())).ReturnsAsync("S2001");

        var svc = new StudentService(studentRepo.Object, indexSvc.Object, _mockUnit.Object);

        // Act
        var ms = await svc.CreateMasterStudentAsync("Jane", "Smith", 4, new Address(), "Thesis", 7);

        // Assert
        Assert.Equal("S2001", ms.UniversityIndex);
        Assert.Equal(7, ms.SupervisorId);
        studentRepo.Verify(r => r.AddStudentAsync(It.IsAny<MasterStudent>()), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllStudentsAsync_ReturnsList()
    {
        // Arrange
        var studentRepo = new Mock<IStudentRepository>();
        studentRepo
            .Setup(r => r.GetAllStudentsAsync())
            .ReturnsAsync(
                new List<Student>
                {
                    new Student { Id = 1, FirstName = "A" },
                }
            );
        var svc = new StudentService(
            studentRepo.Object,
            Mock.Of<IIndexCounterService>(),
            _mockUnit.Object
        );

        // Act
        var res = await svc.GetAllStudentsAsync();

        // Assert
        Assert.Single(res);
    }

    [Fact]
    public async Task GetStudentByIdAsync_ReturnsStudent()
    {
        // Arrange
        var studentRepo = new Mock<IStudentRepository>();
        var student = new Student { Id = 10 };
        studentRepo.Setup(r => r.GetStudentByIdAsync(10)).ReturnsAsync(student);
        var svc = new StudentService(
            studentRepo.Object,
            Mock.Of<IIndexCounterService>(),
            _mockUnit.Object
        );

        // Act
        var res = await svc.GetStudentByIdAsync(10);

        // Assert
        Assert.Equal(student, res);
    }

    [Fact]
    public async Task UpdateStudentAsync_CallsUpdateAndSaves()
    {
        // Arrange
        var studentRepo = new Mock<IStudentRepository>();
        var svc = new StudentService(
            studentRepo.Object,
            Mock.Of<IIndexCounterService>(),
            _mockUnit.Object
        );
        var student = new Student { Id = 1, FirstName = "A" };

        // Act
        await svc.UpdateStudentAsync(student);

        // Assert
        studentRepo.Verify(r => r.UpdateStudentAsync(student), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ShouldDecrementIndexAndDelete()
    {
        // Arrange
        var indexSvc = new Mock<IIndexCounterService>();
        var studentRepo = new Mock<IStudentRepository>();
        var student = new Student { Id = 5, UniversityIndex = "S1002" };
        studentRepo.Setup(r => r.GetStudentByIdAsync(5)).ReturnsAsync(student);
        indexSvc
            .Setup(i => i.TryDecrementIndexAsync("S", "S1002", It.IsAny<bool>()))
            .ReturnsAsync(true);

        var svc = new StudentService(studentRepo.Object, indexSvc.Object, _mockUnit.Object);

        // Act
        await svc.DeleteStudentAsync(5);

        // Assert
        indexSvc.Verify(i => i.TryDecrementIndexAsync("S", "S1002", It.IsAny<bool>()), Times.Once);
        studentRepo.Verify(r => r.DeleteStudentAsync(student), Times.Once);
        _mockUnit.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // Transaction-related tests moved and merged from the transaction-only file
    [Fact]
    public async Task CreateStudentAsync_ShouldRollbackWhenStudentSaveFails()
    {
        // Arrange
        var indexRepo = new Mock<IIndexCounterRepository>();
        var studentRepo = new Mock<IStudentRepository>();
        var unit = new Mock<IUnitOfWork>();

        // Simulate counter present and increment
        var counter = new IndexCounter { Prefix = "S", CurrentValue = 100 };
        indexRepo.Setup(r => r.GetIndexCounterAsync("S")).ReturnsAsync(counter);

        // SaveChanges should throw when trying to save the student
        unit.Setup(u => u.SaveChangesAsync())
            .Throws(new InvalidOperationException("Save student failed"));

        unit.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        var indexService = new IndexCounterService(indexRepo.Object, unit.Object);
        var svc = new StudentService(studentRepo.Object, indexService, unit.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateStudentAsync("A", "B", 1, new Address())
        );

        // Transaction rollback should have been called
        unit.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ShouldRollbackWhenDeleteFails()
    {
        // Arrange
        var indexRepo = new Mock<IIndexCounterRepository>();
        var studentRepo = new Mock<IStudentRepository>();
        var unit = new Mock<IUnitOfWork>();

        var counter = new IndexCounter { Prefix = "S", CurrentValue = 101 };
        indexRepo.Setup(r => r.GetIndexCounterAsync("S")).ReturnsAsync(counter);

        var student = new Student { Id = 1, UniversityIndex = "S101" };
        studentRepo.Setup(r => r.GetStudentByIdAsync(1)).ReturnsAsync(student);

        // SaveChanges should throw when trying to delete the student
        unit.Setup(u => u.SaveChangesAsync())
            .Throws(new InvalidOperationException("Delete failed"));

        unit.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        var indexService = new IndexCounterService(indexRepo.Object, unit.Object);
        var svc = new StudentService(studentRepo.Object, indexService, unit.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteStudentAsync(1));

        // Transaction rollback should have been called
        unit.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateStudentAsync_ShouldCommitTransaction_WhenSucceeds()
    {
        // Arrange
        var indexRepo = new Mock<IIndexCounterRepository>();
        var studentRepo = new Mock<IStudentRepository>();
        var unit = new Mock<IUnitOfWork>();

        var counter = new IndexCounter { Prefix = "S", CurrentValue = 100 };
        indexRepo.Setup(r => r.GetIndexCounterAsync("S")).ReturnsAsync(counter);

        unit.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        var indexService = new IndexCounterService(indexRepo.Object, unit.Object);
        var svc = new StudentService(studentRepo.Object, indexService, unit.Object);

        // Act
        var result = await svc.CreateStudentAsync("A", "B", 1, new Address());

        // Assert
        Assert.NotNull(result);
        unit.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ShouldCommitTransaction_WhenSucceeds()
    {
        // Arrange
        var indexRepo = new Mock<IIndexCounterRepository>();
        var studentRepo = new Mock<IStudentRepository>();
        var unit = new Mock<IUnitOfWork>();

        var counter = new IndexCounter { Prefix = "S", CurrentValue = 101 };
        indexRepo.Setup(r => r.GetIndexCounterAsync("S")).ReturnsAsync(counter);

        var student = new Student { Id = 1, UniversityIndex = "S101" };
        studentRepo.Setup(r => r.GetStudentByIdAsync(1)).ReturnsAsync(student);

        unit.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        unit.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        var indexService = new IndexCounterService(indexRepo.Object, unit.Object);
        var svc = new StudentService(studentRepo.Object, indexService, unit.Object);

        // Act
        await svc.DeleteStudentAsync(1);

        // Assert
        unit.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }
}
