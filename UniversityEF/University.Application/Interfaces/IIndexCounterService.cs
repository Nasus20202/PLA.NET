using University.Domain.Entities;

namespace University.Application.Interfaces;

/// <summary>
/// Interfejs dla serwisu zarządzającego licznikami indeksów
/// </summary>
public interface IIndexCounterService
{
    Task<string> GetNextIndexAsync(string prefix);
    Task<bool> TryDecrementIndexAsync(string prefix, string currentIndex);
    Task InitializeCounterAsync(string prefix, int startValue);
    Task<IndexCounter?> GetCounterAsync(string prefix);
    Task<IEnumerable<IndexCounter>> GetAllCountersAsync();
    Task DeleteCounterAsync(string prefix);
}
