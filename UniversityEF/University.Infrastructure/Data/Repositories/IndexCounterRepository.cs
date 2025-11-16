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

    public async Task<IndexCounter?> GetCounterAsync(string prefix)
    {
        return await _context.IndexCounters.FirstOrDefaultAsync(c => c.Prefix == prefix);
    }

    public async Task<IEnumerable<IndexCounter>> GetAllIndexCountersAsync()
    {
        return await _context.IndexCounters.ToListAsync();
    }

    public Task AddCounterAsync(IndexCounter counter)
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

    public async Task<(int startIndex, int endIndex)> ReserveBatchAsync(string prefix, int count)
    {
        // SQLite doesn't support FOR UPDATE, but since we're already in a transaction
        // from DataGeneratorService, this will be serialized at the transaction level
        var counter = await _context.IndexCounters.FirstOrDefaultAsync(c => c.Prefix == prefix);

        if (counter == null)
        {
            throw new InvalidOperationException(
                $"Counter for prefix '{prefix}' does not exist. Initialize the counter first."
            );
        }

        int startIndex = counter.CurrentValue + 1;
        int endIndex = counter.CurrentValue + count;

        counter.CurrentValue = endIndex;

        return (startIndex, endIndex);
    }
}
