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

    public Task AddOfficeAsync(Office office)
    {
        _context.Offices.Add(office);
        return Task.CompletedTask;
    }
}
