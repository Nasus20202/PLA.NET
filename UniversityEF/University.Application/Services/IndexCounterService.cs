using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

/// <summary>
/// Serwis zarządzający licznikami indeksów
/// </summary>
public class IndexCounterService : IIndexCounterService
{
    private readonly IUniversityRepository _repository;

    public IndexCounterService(IUniversityRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Pobiera kolejny numer indeksu dla danego prefiksu (w transakcji)
    /// </summary>
    public async Task<string> GetNextIndexAsync(string prefix)
    {
        await _repository.BeginTransactionAsync();
        try
        {
            // Pobierz licznik (z blokowaniem dla zapewnienia spójności)
            var counter = await _repository.GetIndexCounterAsync(prefix);

            if (counter == null)
            {
                throw new InvalidOperationException($"Licznik dla prefiksu '{prefix}' nie istnieje. Najpierw zainicjalizuj licznik.");
            }

            // Zwiększ wartość
            counter.CurrentValue++;
            await _repository.UpdateIndexCounterAsync(counter);
            await _repository.SaveChangesAsync();
            await _repository.CommitTransactionAsync();

            // Zwróć sformatowany indeks
            return $"{prefix}{counter.CurrentValue}";
        }
        catch
        {
            await _repository.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Próbuje zmniejszyć licznik jeśli usuwany indeks jest ostatni w sekwencji
    /// </summary>
    public async Task<bool> TryDecrementIndexAsync(string prefix, string currentIndex)
    {
        await _repository.BeginTransactionAsync();
        try
        {
            var counter = await _repository.GetIndexCounterAsync(prefix);

            if (counter == null)
                return false;

            // Wyciągnij numer z indeksu (np. "S1002" -> 1002)
            var numberPart = currentIndex.Substring(prefix.Length);
            if (!int.TryParse(numberPart, out int indexNumber))
                return false;

            // Sprawdź czy to ostatni numer w sekwencji
            if (indexNumber == counter.CurrentValue)
            {
                counter.CurrentValue--;
                await _repository.UpdateIndexCounterAsync(counter);
                await _repository.SaveChangesAsync();
                await _repository.CommitTransactionAsync();
                return true;
            }

            await _repository.CommitTransactionAsync();
            return false;
        }
        catch
        {
            await _repository.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Inicjalizuje nowy licznik
    /// </summary>
    public async Task InitializeCounterAsync(string prefix, int startValue)
    {
        var existingCounter = await _repository.GetIndexCounterAsync(prefix);

        if (existingCounter != null)
        {
            throw new InvalidOperationException($"Licznik dla prefiksu '{prefix}' już istnieje.");
        }

        var counter = new IndexCounter
        {
            Prefix = prefix,
            CurrentValue = startValue
        };

        await _repository.AddIndexCounterAsync(counter);
        await _repository.SaveChangesAsync();
    }

    public async Task<IndexCounter?> GetCounterAsync(string prefix)
    {
        return await _repository.GetIndexCounterAsync(prefix);
    }

    public async Task<IEnumerable<IndexCounter>> GetAllCountersAsync()
    {
        return await _repository.GetAllIndexCountersAsync();
    }

    public async Task DeleteCounterAsync(string prefix)
    {
        var counter = await _repository.GetIndexCounterAsync(prefix);

        if (counter != null)
        {
            await _repository.DeleteIndexCounterAsync(counter);
            await _repository.SaveChangesAsync();
        }
    }
}
