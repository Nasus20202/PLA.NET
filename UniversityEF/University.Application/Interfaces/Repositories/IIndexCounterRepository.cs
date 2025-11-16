using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IIndexCounterRepository
{
    Task<IndexCounter?> GetIndexCounterAsync(string prefix);
    Task<IEnumerable<IndexCounter>> GetAllIndexCountersAsync();
    Task AddIndexCounterAsync(IndexCounter counter);
    Task UpdateIndexCounterAsync(IndexCounter counter);
    Task DeleteIndexCounterAsync(IndexCounter counter);
}
