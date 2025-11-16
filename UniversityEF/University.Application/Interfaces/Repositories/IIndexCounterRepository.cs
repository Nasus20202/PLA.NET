using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IIndexCounterRepository
{
    Task<IndexCounter?> GetCounterAsync(string prefix);
    Task<IEnumerable<IndexCounter>> GetAllIndexCountersAsync();
    Task AddCounterAsync(IndexCounter counter);
    Task UpdateIndexCounterAsync(IndexCounter counter);
    Task DeleteIndexCounterAsync(IndexCounter counter);
    Task<(int startIndex, int endIndex)> ReserveBatchAsync(string prefix, int count);
}
