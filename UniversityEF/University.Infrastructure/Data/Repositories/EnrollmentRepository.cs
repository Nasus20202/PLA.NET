using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly UniversityDbContext _context;

    public EnrollmentRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(int id)
    {
        return await _context.Enrollments.FindAsync(id);
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        return await _context
            .Enrollments.Include(e => e.Course)
            .ThenInclude(k => k.Department)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseIdAsync(int kursId)
    {
        return await _context
            .Enrollments.Include(e => e.Student)
            .Where(e => e.CourseId == kursId)
            .ToListAsync();
    }

    public Task AddEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        return Task.CompletedTask;
    }

    public Task UpdateEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        return Task.CompletedTask;
    }

    public Task DeleteEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        return Task.CompletedTask;
    }
}
