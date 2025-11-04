using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(string nazwa, string kodCourseu, int punktyECTS, int wydzialId, int? profesorId = null);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task UpdateCourseAsync(Course kurs);
    Task DeleteCourseAsync(int id);
    Task AddPrerequisiteAsync(int kursId, int prerequisiteId);
    Task RemovePrerequisiteAsync(int kursId, int prerequisiteId);
}
