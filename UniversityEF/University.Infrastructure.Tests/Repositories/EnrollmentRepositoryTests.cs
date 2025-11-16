using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class EnrollmentRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddEnrollment_AndGetByStudentAndCourse()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A" };
        var dep = new Department { Name = "D" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();
        var c = new Course
        {
            Name = "C",
            CourseCode = "C1",
            DepartmentId = dep.Id,
        };
        ctx.Courses.Add(c);
        ctx.Students.Add(s);
        await ctx.SaveChangesAsync();

        var repo = new EnrollmentRepository(ctx);
        var e = new Enrollment
        {
            StudentId = s.Id,
            CourseId = c.Id,
            Semester = 1,
        };

        // Act
        await repo.AddEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);
        var byStudent = (await repo2.GetEnrollmentsByStudentIdAsync(s.Id)).ToList();
        var byCourse = (await repo2.GetEnrollmentsByCourseIdAsync(c.Id)).ToList();

        // Assert
        Assert.Single(byStudent);
        Assert.Single(byCourse);
        Assert.Equal(e.StudentId, byStudent.First().StudentId);
    }
}
