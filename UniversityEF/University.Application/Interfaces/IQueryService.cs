using University.Application.DTOs;

namespace University.Application.Interfaces;

/// <summary>
/// Service for advanced LINQ queries
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Finds the professor who teaches courses with the largest total number of students
    /// </summary>
    Task<ProfessorStudentCountDto?> GetProfessorWithMostStudentsAsync();

    /// <summary>
    /// Returns average grades for each course in a given department
    /// </summary>
    Task<IEnumerable<CourseAverageDto>> GetCourseAveragesForFacultyAsync(int wydzialId);

    /// <summary>
    /// Finds the student with the hardest schedule
    /// (sum of ECTS of courses + sum of ECTS of their prerequisites, without duplicates)
    /// </summary>
    Task<StudentDifficultyDto?> GetStudentWithHardestScheduleAsync();
}
