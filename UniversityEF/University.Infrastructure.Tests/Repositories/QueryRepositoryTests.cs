using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class QueryRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task ExecuteStudentQuery_ReturnsProjection()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new QueryRepository(ctx);
        var s = new Student { FirstName = "Q" };
        ctx.Students.Add(s);
        await ctx.SaveChangesAsync();

        // Act
        var result = (
            await repo.ExecuteQueryAsync(students => students.Select(st => st.FirstName))
        ).ToList();

        // Assert
        Assert.Contains("Q", result);
    }

    [Fact]
    public async Task ExecuteProfessorQuery_ReturnsProjection()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new QueryRepository(ctx);
        var p = new Professor { FirstName = "PR" };
        ctx.Professors.Add(p);
        await ctx.SaveChangesAsync();

        // Act
        var result = (
            await repo.ExecuteProfessorQueryAsync(profs => profs.Select(pr => pr.FirstName))
        ).ToList();

        // Assert
        Assert.Contains("PR", result);
    }

    [Fact]
    public async Task ExecuteCourseQuery_ReturnsProjection()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new QueryRepository(ctx);
        var dep = new Department { Name = "D" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();
        var course = new Course
        {
            Name = "C",
            CourseCode = "C1",
            DepartmentId = dep.Id,
        };
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        // Act
        var result = (
            await repo.ExecuteCourseQueryAsync(courses => courses.Select(c => c.CourseCode))
        ).ToList();

        // Assert
        Assert.Contains("C1", result);
    }
}
