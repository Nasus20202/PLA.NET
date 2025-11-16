using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IIndexCounterService
{
    Task<string> GetNextIndexAsync(string prefix, bool manageTransaction = true);
    Task<bool> TryDecrementIndexAsync(
        string prefix,
        string currentIndex,
        bool manageTransaction = true
    );
    Task InitializeCounterAsync(string prefix, int startValue);
    Task<IndexCounter?> GetCounterAsync(string prefix);
    Task<IEnumerable<IndexCounter>> GetAllCountersAsync();
    Task DeleteCounterAsync(string prefix);
}
