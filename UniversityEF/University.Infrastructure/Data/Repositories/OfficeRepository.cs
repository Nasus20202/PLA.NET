using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class OfficeRepository : IOfficeRepository
{
    private readonly UniversityDbContext _context;

    public OfficeRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<Office?> GetOfficeByIdAsync(int id)
    {
        return await _context
            .Offices.Include(o => o.Professor)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Office?> GetOfficeByProfessorIdAsync(int professorId)
    {
        return await _context
            .Offices.Include(o => o.Professor)
            .FirstOrDefaultAsync(o => o.ProfessorId == professorId);
    }

    public async Task<IEnumerable<Office>> GetAllOfficesAsync()
    {
        return await _context.Offices.Include(o => o.Professor).ToListAsync();
    }

    public Task AddOfficeAsync(Office office)
    {
        _context.Offices.Add(office);
        return Task.CompletedTask;
    }

    public Task UpdateOfficeAsync(Office office)
    {
        _context.Offices.Update(office);
        return Task.CompletedTask;
    }

    public Task DeleteOfficeAsync(Office office)
    {
        _context.Offices.Remove(office);
        return Task.CompletedTask;
    }
}
