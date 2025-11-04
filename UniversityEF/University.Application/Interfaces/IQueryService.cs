using University.Application.DTOs;

namespace University.Application.Interfaces;

/// <summary>
/// Serwis dla zaawansowanych zapytań LINQ
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Znajduje profesora, który prowadzi kursy z największą łączną liczbą studentów
    /// </summary>
    Task<ProfessorStudentCountDto?> GetProfessorWithMostStudentsAsync();

    /// <summary>
    /// Zwraca średnią ocen dla każdego kursu na danym wydziale
    /// </summary>
    Task<IEnumerable<CourseAverageDto>> GetCourseAveragesForFacultyAsync(int wydzialId);

    /// <summary>
    /// Znajduje studenta z najtrudniejszym planem zajęć
    /// (suma ECTS kursów + suma ECTS ich prererekwizytów, bez powtórzeń)
    /// </summary>
    Task<StudentDifficultyDto?> GetStudentWithHardestScheduleAsync();
}
