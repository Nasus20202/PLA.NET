using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class CourseRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetCourseByIdAsync_ReturnsCourse_WhenExists()
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
        await repo.AddCourseAsync(course);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new CourseRepository(ctx2);

        // Act
        var fetched = await repo2.GetCourseByIdAsync(course.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("M101", fetched!.CourseCode);
        Assert.Equal("Math", fetched.Name);
    }

    [Fact]
    public async Task GetCourseByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new CourseRepository(ctx);

        // Act
        var fetched = await repo.GetCourseByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllCoursesAsync_ReturnsAllCourses()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new CourseRepository(ctx);
        var dep = new Department { Name = "CS" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();

        var course1 = new Course
        {
            Name = "C1",
            CourseCode = "CS101",
            DepartmentId = dep.Id,
        };
        var course2 = new Course
        {
            Name = "C2",
            CourseCode = "CS102",
            DepartmentId = dep.Id,
        };
        await repo.AddCourseAsync(course1);
        await repo.AddCourseAsync(course2);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new CourseRepository(ctx2);

        // Act
        var allCourses = (await repo2.GetAllCoursesAsync()).ToList();

        // Assert
        Assert.Equal(2, allCourses.Count);
        Assert.Contains(allCourses, c => c.CourseCode == "CS101");
        Assert.Contains(allCourses, c => c.CourseCode == "CS102");
    }

    [Fact]
    public async Task AddCourseAsync_AddsCourse()
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

    [Fact]
    public async Task AddCoursesAsync_AddsMultipleCourses()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new CourseRepository(ctx);
        var dep = new Department { Name = "CS" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();

        var courses = new List<Course>
        {
            new Course
            {
                Name = "Algorithms",
                CourseCode = "CS101",
                DepartmentId = dep.Id,
            },
            new Course
            {
                Name = "Data Structures",
                CourseCode = "CS102",
                DepartmentId = dep.Id,
            },
            new Course
            {
                Name = "Databases",
                CourseCode = "CS201",
                DepartmentId = dep.Id,
            },
        };

        // Act
        await repo.AddCoursesAsync(courses);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new CourseRepository(ctx2);
        var allCourses = (await repo2.GetAllCoursesAsync()).ToList();

        // Assert
        Assert.Equal(3, allCourses.Count);
        Assert.Contains(allCourses, c => c.CourseCode == "CS101");
        Assert.Contains(allCourses, c => c.CourseCode == "CS102");
        Assert.Contains(allCourses, c => c.CourseCode == "CS201");
    }

    [Fact]
    public async Task UpdateCourseAsync_UpdatesCourse()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new CourseRepository(ctx);
        var dep = new Department { Name = "D1" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();

        var course = new Course
        {
            Name = "Original",
            CourseCode = "C100",
            DepartmentId = dep.Id,
        };
        await repo.AddCourseAsync(course);
        await ctx.SaveChangesAsync();

        // Act
        course.Name = "Updated";
        await repo.UpdateCourseAsync(course);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new CourseRepository(ctx2);
        var fetched = await repo2.GetCourseByIdAsync(course.Id);

        // Assert
        Assert.Equal("Updated", fetched!.Name);
    }

    [Fact]
    public async Task DeleteCourseAsync_DeletesCourse()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new CourseRepository(ctx);
        var dep = new Department { Name = "D1" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();

        var course = new Course
        {
            Name = "ToDelete",
            CourseCode = "D100",
            DepartmentId = dep.Id,
        };
        await repo.AddCourseAsync(course);
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteCourseAsync(course);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new CourseRepository(ctx2);
        var fetched = await repo2.GetCourseByIdAsync(course.Id);

        // Assert
        Assert.Null(fetched);
    }
}
