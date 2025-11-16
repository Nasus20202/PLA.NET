using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class ProfessorRepository : IProfessorRepository
{
    private readonly UniversityDbContext _context;

    public ProfessorRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<Professor?> GetProfessorByIdAsync(int id)
    {
        return await _context
            .Professors.Include(p => p.Office)
            .Include(p => p.TaughtCourses)
            .Include(p => p.SupervisedStudents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Professor>> GetAllProfessorsAsync()
    {
        return await _context.Professors.Include(p => p.Office).ToListAsync();
    }

    public Task AddProfessorAsync(Professor professor)
    {
        _context.Professors.Add(professor);
        return Task.CompletedTask;
    }

    public Task AddProfessorsAsync(IEnumerable<Professor> professors)
    {
        _context.Professors.AddRange(professors);
        return Task.CompletedTask;
    }

    public Task UpdateProfessorAsync(Professor professor)
    {
        _context.Professors.Update(professor);
        return Task.CompletedTask;
    }

    public Task DeleteProfessorAsync(Professor professor)
    {
        _context.Professors.Remove(professor);
        return Task.CompletedTask;
    }
}
