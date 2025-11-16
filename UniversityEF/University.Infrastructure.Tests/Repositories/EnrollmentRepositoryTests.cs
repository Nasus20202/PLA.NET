using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class EnrollmentRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetEnrollmentByIdAsync_ReturnsEnrollment_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A", UniversityIndex = "S001" };
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
        await repo.AddEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);

        // Act
        var fetched = await repo2.GetEnrollmentByIdAsync(e.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(s.Id, fetched!.StudentId);
        Assert.Equal(c.Id, fetched.CourseId);
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new EnrollmentRepository(ctx);

        // Act
        var fetched = await repo.GetEnrollmentByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetEnrollmentsByStudentIdAsync_ReturnsStudentEnrollments()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A", UniversityIndex = "S002" };
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
        await repo.AddEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);

        // Act
        var byStudent = (await repo2.GetEnrollmentsByStudentIdAsync(s.Id)).ToList();

        // Assert
        Assert.Single(byStudent);
        Assert.Equal(e.StudentId, byStudent.First().StudentId);
    }

    [Fact]
    public async Task GetEnrollmentsByCourseIdAsync_ReturnsCourseEnrollments()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A", UniversityIndex = "S003" };
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
        await repo.AddEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);

        // Act
        var byCourse = (await repo2.GetEnrollmentsByCourseIdAsync(c.Id)).ToList();

        // Assert
        Assert.Single(byCourse);
        Assert.Equal(c.Id, byCourse.First().CourseId);
    }

    [Fact]
    public async Task AddEnrollmentAsync_AddsEnrollment()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A", UniversityIndex = "S004" };
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

    [Fact]
    public async Task AddEnrollmentsAsync_AddsMultipleEnrollments()
    {
        // Arrange
        using var ctx = NewContext();
        var student1 = new Student { FirstName = "Alice", UniversityIndex = "S001" };
        var student2 = new Student { FirstName = "Bob", UniversityIndex = "S002" };
        var dep = new Department { Name = "CS" };
        ctx.Faculties.Add(dep);
        await ctx.SaveChangesAsync();

        var course = new Course
        {
            Name = "Programming",
            CourseCode = "CS101",
            DepartmentId = dep.Id,
        };
        ctx.Courses.Add(course);
        ctx.Students.AddRange(student1, student2);
        await ctx.SaveChangesAsync();

        var repo = new EnrollmentRepository(ctx);
        var enrollments = new List<Enrollment>
        {
            new Enrollment
            {
                StudentId = student1.Id,
                CourseId = course.Id,
                Semester = 1,
            },
            new Enrollment
            {
                StudentId = student2.Id,
                CourseId = course.Id,
                Semester = 1,
            },
        };

        // Act
        await repo.AddEnrollmentsAsync(enrollments);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);
        var allEnrollments = (await repo2.GetEnrollmentsByCourseIdAsync(course.Id)).ToList();

        // Assert
        Assert.Equal(2, allEnrollments.Count);
        Assert.Contains(allEnrollments, e => e.StudentId == student1.Id);
        Assert.Contains(allEnrollments, e => e.StudentId == student2.Id);
    }

    [Fact]
    public async Task UpdateEnrollmentAsync_UpdatesEnrollment()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A", UniversityIndex = "S005" };
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
            Grade = 3.0,
        };
        await repo.AddEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        // Act
        e.Grade = 5.0;
        await repo.UpdateEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);
        var fetched = await repo2.GetEnrollmentByIdAsync(e.Id);

        // Assert
        Assert.Equal(5.0, fetched!.Grade);
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_DeletesEnrollment()
    {
        // Arrange
        using var ctx = NewContext();
        var s = new Student { FirstName = "A", UniversityIndex = "S006" };
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
        await repo.AddEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteEnrollmentAsync(e);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new EnrollmentRepository(ctx2);
        var fetched = await repo2.GetEnrollmentByIdAsync(e.Id);

        // Assert
        Assert.Null(fetched);
    }
}
