using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class IndexCounterService : IIndexCounterService
{
    private readonly IIndexCounterRepository _indexRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IProfessorRepository _professorRepo;
    private readonly IUnitOfWork _unitOfWork;

    public IndexCounterService(
        IIndexCounterRepository indexRepo,
        IStudentRepository studentRepo,
        IProfessorRepository professorRepo,
        IUnitOfWork unitOfWork
    )
    {
        _indexRepo = indexRepo;
        _studentRepo = studentRepo;
        _professorRepo = professorRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GetNextIndexAsync(string prefix, bool manageTransaction = true)
    {
        // Normalize prefix to uppercase
        prefix = prefix.ToUpper();

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
        // Normalize prefix to uppercase
        prefix = prefix.ToUpper();

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
        // Normalize prefix to uppercase
        prefix = prefix.ToUpper();

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
        // Normalize prefix to uppercase
        prefix = prefix.ToUpper();

        return await _indexRepo.GetCounterAsync(prefix);
    }

    public async Task<IEnumerable<IndexCounter>> GetAllCountersAsync()
    {
        return await _indexRepo.GetAllIndexCountersAsync();
    }

    public async Task UpdateCounterAsync(string prefix, int newValue)
    {
        // Normalize prefix to uppercase
        prefix = prefix.ToUpper();

        var counter = await _indexRepo.GetCounterAsync(prefix);

        if (counter == null)
        {
            throw new InvalidOperationException(
                $"Counter for prefix '{prefix}' does not exist. Initialize the counter first."
            );
        }

        // Check highest existing index number for this prefix
        var highestStudentIndex = await _studentRepo.GetHighestIndexNumberForPrefixAsync(prefix);
        var highestProfessorIndex = await _professorRepo.GetHighestIndexNumberForPrefixAsync(
            prefix
        );

        var highestExisting = Math.Max(highestStudentIndex ?? 0, highestProfessorIndex ?? 0);

        if (newValue < highestExisting)
        {
            throw new InvalidOperationException(
                $"Cannot set counter to {newValue}. The highest existing index with prefix '{prefix}' is {highestExisting}."
            );
        }

        counter.CurrentValue = newValue;
        await _indexRepo.UpdateIndexCounterAsync(counter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCounterAsync(string prefix)
    {
        // Normalize prefix to uppercase
        prefix = prefix.ToUpper();

        var counter = await _indexRepo.GetCounterAsync(prefix);

        if (counter != null)
        {
            await _indexRepo.DeleteIndexCounterAsync(counter);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
