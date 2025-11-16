using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class IndexCounterService : IIndexCounterService
{
    private readonly IIndexCounterRepository _indexRepo;
    private readonly IUnitOfWork _unitOfWork;

    public IndexCounterService(IIndexCounterRepository indexRepo, IUnitOfWork unitOfWork)
    {
        _indexRepo = indexRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GetNextIndexAsync(string prefix, bool manageTransaction = true)
    {
        if (manageTransaction)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            var counter = await _indexRepo.GetCounterAsync(prefix);

            if (counter == null)
            {
                throw new InvalidOperationException(
                    $"Counter for prefix '{prefix}' does not exist. Initialize the counter first."
                );
            }

            counter.CurrentValue++;
            await _indexRepo.UpdateIndexCounterAsync(counter);
            if (manageTransaction)
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }

            return $"{prefix}{counter.CurrentValue}";
        }
        catch
        {
            if (manageTransaction)
                await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> TryDecrementIndexAsync(
        string prefix,
        string currentIndex,
        bool manageTransaction = true
    )
    {
        if (manageTransaction)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            var counter = await _indexRepo.GetCounterAsync(prefix);

            if (counter == null)
                return false;

            var numberPart = currentIndex.Substring(prefix.Length);
            if (!int.TryParse(numberPart, out int indexNumber))
                return false;

            if (indexNumber == counter.CurrentValue)
            {
                counter.CurrentValue--;
                await _indexRepo.UpdateIndexCounterAsync(counter);
                if (manageTransaction)
                {
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                return true;
            }

            if (manageTransaction)
                await _unitOfWork.CommitTransactionAsync();
            return false;
        }
        catch
        {
            if (manageTransaction)
                await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task InitializeCounterAsync(string prefix, int startValue)
    {
        var existingCounter = await _indexRepo.GetCounterAsync(prefix);

        if (existingCounter != null)
        {
            throw new InvalidOperationException($"Counter for prefix '{prefix}' already exists.");
        }

        var counter = new IndexCounter { Prefix = prefix, CurrentValue = startValue };

        await _indexRepo.AddCounterAsync(counter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IndexCounter?> GetCounterAsync(string prefix)
    {
        return await _indexRepo.GetCounterAsync(prefix);
    }

    public async Task<IEnumerable<IndexCounter>> GetAllCountersAsync()
    {
        return await _indexRepo.GetAllIndexCountersAsync();
    }

    public async Task DeleteCounterAsync(string prefix)
    {
        var counter = await _indexRepo.GetCounterAsync(prefix);

        if (counter != null)
        {
            await _indexRepo.DeleteIndexCounterAsync(counter);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
