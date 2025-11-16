using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class CourseRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddCourse_GetRoundtrip()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new CourseRepository(ctx);
        var dep = new Department { Name = "D1" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();

        var course = new Course
        {
            Name = "Math",
            CourseCode = "M101",
            DepartmentId = dep.Id,
        };

        // Act
        await repo.AddCourseAsync(course);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new CourseRepository(ctx2);
        var fetched = await repo2.GetCourseByIdAsync(course.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("M101", fetched!.CourseCode);
    }
}
