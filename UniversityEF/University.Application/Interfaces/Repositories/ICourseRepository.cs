using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetCourseByIdAsync(int id);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task AddCourseAsync(Course course);
    Task AddCoursesAsync(IEnumerable<Course> courses);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(Course course);
}
