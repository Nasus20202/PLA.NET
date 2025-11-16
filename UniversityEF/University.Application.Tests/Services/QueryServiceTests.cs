using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using University.Application.DTOs;
using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Application.Services;
using University.Domain.Entities;
using Xunit;

#nullable enable

namespace University.Application.Tests.Services;

public class QueryServiceTests
{
    private readonly Mock<IQueryRepository> _mockRepo;
    private readonly QueryService _service;

    public QueryServiceTests()
    {
        _mockRepo = new Mock<IQueryRepository>();
        _service = new QueryService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetProfessorWithMostStudentsAsync_ReturnsDtoForTopProfessor()
    {
        // Arrange

        var professor1 = new Professor
        {
            Id = 1,
            FirstName = "A",
            LastName = "A",
            UniversityIndex = "P1",
        };
        var professor2 = new Professor
        {
            Id = 2,
            FirstName = "B",
            LastName = "B",
            UniversityIndex = "P2",
        };

        var course1 = new Course
        {
            Id = 10,
            Professor = professor1,
            Enrollments = new List<Enrollment>
            {
                new Enrollment { StudentId = 1 },
                new Enrollment { StudentId = 2 },
            },
        };
        var course2 = new Course
        {
            Id = 11,
            Professor = professor1,
            Enrollments = new List<Enrollment> { new Enrollment { StudentId = 2 } },
        };
        var course3 = new Course
        {
            Id = 12,
            Professor = professor2,
            Enrollments = new List<Enrollment> { new Enrollment { StudentId = 9 } },
        };

        professor1.TaughtCourses = new List<Course> { course1, course2 };
        professor2.TaughtCourses = new List<Course> { course3 };

        var professors = new List<Professor> { professor1, professor2 };

        _mockRepo
            .Setup(r =>
                r.ExecuteProfessorQueryAsync(
                    It.IsAny<Func<IQueryable<Professor>, IQueryable<ProfessorStudentCountDto>>>()
                )
            )
            .ReturnsAsync(
                (Func<IQueryable<Professor>, IQueryable<ProfessorStudentCountDto>> f) =>
                    f(professors.AsQueryable()).ToList()
            );

        // Act
        var result = await _service.GetProfessorWithMostStudentsAsync();

        Assert.NotNull(result);
        Assert.Equal(professor1.Id, result!.ProfessorId);
        Assert.Equal(2, result.TotalStudentCount);
    }

    [Fact]
    public async Task GetCourseAveragesForFacultyAsync_ReturnsAverages()
    {
        // Arrange

        var course1 = new Course
        {
            Id = 1,
            DepartmentId = 5,
            Name = "C1",
            CourseCode = "C1",
            Enrollments = new List<Enrollment>
            {
                new Enrollment { Grade = 4.0 },
                new Enrollment { Grade = 5.0 },
            },
        };
        var course2 = new Course
        {
            Id = 2,
            DepartmentId = 6,
            Name = "C2",
            CourseCode = "C2",
            Enrollments = new List<Enrollment> { new Enrollment { Grade = 5.0 } },
        };

        var courses = new List<Course> { course1, course2 };

        _mockRepo
            .Setup(r =>
                r.ExecuteCourseQueryAsync(
                    It.IsAny<Func<IQueryable<Course>, IQueryable<CourseAverageDto>>>()
                )
            )
            .ReturnsAsync(
                (Func<IQueryable<Course>, IQueryable<CourseAverageDto>> f) =>
                    f(courses.AsQueryable()).ToList()
            );

        // Act
        var result = await _service.GetCourseAveragesForFacultyAsync(5);

        Assert.Single(result);
        Assert.Equal(course1.Id, result.First().CourseId);
        Assert.Equal(4.5, result.First().AverageGrade);
    }

    [Fact]
    public async Task GetStudentWithHardestScheduleAsync_ReturnsTopStudent()
    {
        // Arrange

        var course1 = new Course { Id = 1, ECTSPoints = 5 };
        var course2 = new Course
        {
            Id = 2,
            ECTSPoints = 6,
            Prerequisites = new List<Course> { course1 },
        };

        var student1 = new Student
        {
            Id = 1,
            FirstName = "A",
            LastName = "A",
            UniversityIndex = "S1",
            Enrollments = new List<Enrollment> { new Enrollment { Course = course1 } },
        };
        var student2 = new Student
        {
            Id = 2,
            FirstName = "B",
            LastName = "B",
            UniversityIndex = "S2",
            Enrollments = new List<Enrollment> { new Enrollment { Course = course2 } },
        };

        var students = new List<Student> { student1, student2 };

        _mockRepo
            .Setup(r =>
                r.ExecuteQueryAsync(
                    It.IsAny<Func<IQueryable<Student>, IQueryable<StudentDifficultyDto>>>()
                )
            )
            .ReturnsAsync(
                (Func<IQueryable<Student>, IQueryable<StudentDifficultyDto>> f) =>
                    f(students.AsQueryable()).ToList()
            );

        // Act
        var res = await _service.GetStudentWithHardestScheduleAsync();

        Assert.NotNull(res);
        Assert.Equal(student2.Id, res!.StudentId);
    }
}
