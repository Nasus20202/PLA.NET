using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class IndexCounterRepository : IIndexCounterRepository
{
    private readonly UniversityDbContext _context;

    public IndexCounterRepository(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<IndexCounter?> GetIndexCounterAsync(string prefix)
    {
        return await _context.IndexCounters.FirstOrDefaultAsync(c => c.Prefix == prefix);
    }

    public async Task<IEnumerable<IndexCounter>> GetAllIndexCountersAsync()
    {
        return await _context.IndexCounters.ToListAsync();
    }

    public Task AddIndexCounterAsync(IndexCounter counter)
    {
        _context.IndexCounters.Add(counter);
        return Task.CompletedTask;
    }

    public Task UpdateIndexCounterAsync(IndexCounter counter)
    {
        _context.IndexCounters.Update(counter);
        return Task.CompletedTask;
    }

    public Task DeleteIndexCounterAsync(IndexCounter counter)
    {
        _context.IndexCounters.Remove(counter);
        return Task.CompletedTask;
    }
}
