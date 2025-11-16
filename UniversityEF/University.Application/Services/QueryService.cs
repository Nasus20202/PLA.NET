using University.Application.DTOs;
using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class QueryService : IQueryService
{
    private readonly IQueryRepository _repository;

    public QueryService(IQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProfessorStudentCountDto?> GetProfessorWithMostStudentsAsync()
    {
        var results = await _repository.ExecuteProfessorQueryAsync(profesors =>
            profesors
                .Select(p => new
                {
                    Professor = p,
                    TotalStudents = p
                        .TaughtCourses.SelectMany(k => k.Enrollments)
                        .Select(e => e.StudentId)
                        .Distinct()
                        .Count(),
                })
                .Where(x => x.TotalStudents > 0)
                .OrderByDescending(x => x.TotalStudents)
                .Select(x => new ProfessorStudentCountDto
                {
                    ProfessorId = x.Professor.Id,
                    FullName = x.Professor.FirstName + " " + x.Professor.LastName,
                    UniversityIndex = x.Professor.UniversityIndex,
                    TotalStudentCount = x.TotalStudents,
                    CourseCount = x.Professor.TaughtCourses.Count,
                })
                .Take(1)
        );

        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<CourseAverageDto>> GetCourseAveragesForFacultyAsync(int wydzialId)
    {
        return await _repository.ExecuteCourseQueryAsync(courses =>
            courses
                .Where(k => k.DepartmentId == wydzialId)
                .Select(k => new
                {
                    Course = k,
                    Enrollments = k.Enrollments.Where(e => e.Grade.HasValue).ToList(),
                })
                .Where(x => x.Enrollments.Any())
                .Select(x => new CourseAverageDto
                {
                    CourseId = x.Course.Id,
                    CourseName = x.Course.Name,
                    CourseCode = x.Course.CourseCode,
                    AverageGrade = x.Enrollments.Average(e => e.Grade!.Value),
                    StudentCount = x.Enrollments.Count,
                })
        );
    }

    public async Task<StudentDifficultyDto?> GetStudentWithHardestScheduleAsync()
    {
        var results = await _repository.ExecuteQueryAsync(students =>
            students
                .Where(s => s.Enrollments.Any())
                .Select(s => new
                {
                    Student = s,
                    CourseEcts = s.Enrollments.Sum(e => e.Course.ECTSPoints),
                    PrerequisiteEcts = s
                        .Enrollments.SelectMany(e => e.Course.Prerequisites)
                        .Select(p => p.Id)
                        .Distinct()
                        .Sum(id =>
                            students
                                .SelectMany(st => st.Enrollments)
                                .Select(e => e.Course)
                                .Where(k => k.Id == id)
                                .Select(k => k.ECTSPoints)
                                .FirstOrDefault()
                        ),
                })
                .Select(x => new StudentDifficultyDto
                {
                    StudentId = x.Student.Id,
                    FullName = x.Student.FirstName + " " + x.Student.LastName,
                    UniversityIndex = x.Student.UniversityIndex,
                    CourseECTS = x.CourseEcts,
                    PrerequisiteECTS = x.PrerequisiteEcts,
                    TotalDifficulty = x.CourseEcts + x.PrerequisiteEcts,
                })
                .OrderByDescending(x => x.TotalDifficulty)
                .Take(1)
        );

        return results.FirstOrDefault();
    }
}
