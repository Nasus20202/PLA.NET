using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class StudentRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetStudentByIdAsync_ReturnsStudent_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student = new Student
        {
            FirstName = "Alice",
            LastName = "Brown",
            UniversityIndex = "S100",
        };
        await repo.AddStudentAsync(student);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);

        // Act
        var fetched = await repo2.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Alice", fetched!.FirstName);
        Assert.Equal("Brown", fetched.LastName);
    }

    [Fact]
    public async Task GetStudentByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);

        // Act
        var fetched = await repo.GetStudentByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllStudentsAsync_ReturnsAllStudents()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student1 = new Student { FirstName = "Alice", UniversityIndex = "S101" };
        var student2 = new Student { FirstName = "Bob", UniversityIndex = "S102" };
        await repo.AddStudentAsync(student1);
        await repo.AddStudentAsync(student2);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);

        // Act
        var allStudents = (await repo2.GetAllStudentsAsync()).ToList();

        // Assert
        Assert.Equal(2, allStudents.Count);
        Assert.Contains(allStudents, s => s.FirstName == "Alice");
        Assert.Contains(allStudents, s => s.FirstName == "Bob");
    }

    [Fact]
    public async Task AddStudentAsync_AddsStudent()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student = new Student
        {
            FirstName = "Charlie",
            LastName = "Davis",
            UniversityIndex = "S200",
        };

        // Act
        await repo.AddStudentAsync(student);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);
        var fetched = await repo2.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Charlie", fetched!.FirstName);
    }

    [Fact]
    public async Task AddStudentsAsync_AddsMultipleStudents()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var students = new List<Student>
        {
            new Student
            {
                FirstName = "John",
                LastName = "Doe",
                UniversityIndex = "S1001",
            },
            new Student
            {
                FirstName = "Jane",
                LastName = "Smith",
                UniversityIndex = "S1002",
            },
            new Student
            {
                FirstName = "Bob",
                LastName = "Johnson",
                UniversityIndex = "S1003",
            },
        };

        // Act
        await repo.AddStudentsAsync(students);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);
        var allStudents = (await repo2.GetAllStudentsAsync()).ToList();

        // Assert
        Assert.Equal(3, allStudents.Count);
        Assert.Contains(allStudents, s => s.FirstName == "John");
        Assert.Contains(allStudents, s => s.FirstName == "Jane");
        Assert.Contains(allStudents, s => s.FirstName == "Bob");
    }

    [Fact]
    public async Task UpdateStudentAsync_UpdatesStudent()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student = new Student { FirstName = "Original", UniversityIndex = "S300" };
        await repo.AddStudentAsync(student);
        await ctx.SaveChangesAsync();

        // Act
        student.FirstName = "Updated";
        await repo.UpdateStudentAsync(student);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);
        var fetched = await repo2.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.Equal("Updated", fetched!.FirstName);
    }

    [Fact]
    public async Task DeleteStudentAsync_DeletesStudent()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student = new Student { FirstName = "ToDelete", UniversityIndex = "S400" };
        await repo.AddStudentAsync(student);
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteStudentAsync(student);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);
        var fetched = await repo2.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.Null(fetched);
    }
}
