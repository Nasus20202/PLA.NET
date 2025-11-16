using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly UniversityDbContext _context;

    public DepartmentRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _context
            .Faculties.Include(w => w.Courses)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _context.Faculties.ToListAsync();
    }

    public Task AddDepartmentAsync(Department department)
    {
        _context.Faculties.Add(department);
        return Task.CompletedTask;
    }

    public Task AddDepartmentsAsync(IEnumerable<Department> departments)
    {
        _context.Faculties.AddRange(departments);
        return Task.CompletedTask;
    }

    public Task UpdateDepartmentAsync(Department department)
    {
        _context.Faculties.Update(department);
        return Task.CompletedTask;
    }

    public Task DeleteDepartmentAsync(Department department)
    {
        _context.Faculties.Remove(department);
        return Task.CompletedTask;
    }
}
