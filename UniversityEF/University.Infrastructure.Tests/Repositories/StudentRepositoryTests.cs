using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class StudentRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddStudent_SaveAndGetById_ReturnsStudent()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student = new Student { FirstName = "Alice", LastName = "B" };

        // Act
        await repo.AddStudentAsync(student);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);
        var fetched = await repo2.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Alice", fetched!.FirstName);
    }

    [Fact]
    public async Task UpdateStudent_SavesChange()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new StudentRepository(ctx);
        var student = new Student { FirstName = "X" };
        await repo.AddStudentAsync(student);
        await ctx.SaveChangesAsync();

        // Act
        student.FirstName = "Y";
        await repo.UpdateStudentAsync(student);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new StudentRepository(ctx2);
        var fetched = await repo2.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.Equal("Y", fetched!.FirstName);
    }
}
