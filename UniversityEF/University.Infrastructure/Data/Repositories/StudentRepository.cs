using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly UniversityDbContext _context;

    public StudentRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context
            .Students.Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await _context.Students.Include(s => s.Enrollments).ToListAsync();
    }

    public Task AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        return Task.CompletedTask;
    }

    public Task AddStudentsAsync(IEnumerable<Student> students)
    {
        _context.Students.AddRange(students);
        return Task.CompletedTask;
    }

    public Task UpdateStudentAsync(Student student)
    {
        _context.Students.Update(student);
        return Task.CompletedTask;
    }

    public Task DeleteStudentAsync(Student student)
    {
        _context.Students.Remove(student);
        return Task.CompletedTask;
    }
}
