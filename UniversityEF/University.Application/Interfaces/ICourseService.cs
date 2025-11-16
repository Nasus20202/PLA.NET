using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(
        string name,
        string courseCode,
        int ectsPoints,
        int departmentId,
        int? professorId = null
    );
    Task<Course?> GetCourseByIdAsync(int id);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(int id);
    Task AddPrerequisiteAsync(int courseId, int prerequisiteId);
    Task RemovePrerequisiteAsync(int courseId, int prerequisiteId);
}
