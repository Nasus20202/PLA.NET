using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly UniversityDbContext _context;

    public CourseRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _context
            .Courses.Include(k => k.Department)
            .Include(k => k.Professor)
            .Include(k => k.Prerequisites)
            .Include(k => k.Enrollments)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _context
            .Courses.Include(k => k.Department)
            .Include(k => k.Professor)
            .ToListAsync();
    }

    public Task AddCourseAsync(Course course)
    {
        _context.Courses.Add(course);
        return Task.CompletedTask;
    }

    public Task AddCoursesAsync(IEnumerable<Course> courses)
    {
        _context.Courses.AddRange(courses);
        return Task.CompletedTask;
    }

    public Task UpdateCourseAsync(Course course)
    {
        _context.Courses.Update(course);
        return Task.CompletedTask;
    }

    public Task DeleteCourseAsync(Course course)
    {
        _context.Courses.Remove(course);
        return Task.CompletedTask;
    }
}
