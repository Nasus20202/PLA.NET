using University.Application.DTOs;

namespace University.Application.Interfaces;

public interface IQueryService
{
    Task<ProfessorStudentCountDto?> GetProfessorWithMostStudentsAsync();

    Task<IEnumerable<CourseAverageDto>> GetCourseAveragesForFacultyAsync(int wydzialId);

    Task<StudentDifficultyDto?> GetStudentWithHardestScheduleAsync();
}
