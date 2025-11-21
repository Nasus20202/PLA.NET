using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class ProfessorService : IProfessorService
{
    private readonly IProfessorRepository _repository;
    private readonly IOfficeRepository _officeRepository;
    private readonly IIndexCounterService _indexCounterService;
    private readonly IUnitOfWork _unitOfWork;

    public ProfessorService(
        IProfessorRepository repository,
        IOfficeRepository officeRepository,
        IIndexCounterService indexCounterService,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _officeRepository = officeRepository;
        _indexCounterService = indexCounterService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Professor> CreateProfessorAsync(
        string firstName,
        string lastName,
        string academicTitle,
        Address address
    )
    {
        return await CreateProfessorAsync(firstName, lastName, academicTitle, address, "P");
    }

    public async Task<Professor> CreateProfessorAsync(
        string firstName,
        string lastName,
        string academicTitle,
        Address address,
        string prefix
    )
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var indeks = await _indexCounterService.GetNextIndexAsync(
                prefix,
                manageTransaction: false
            );

            var professor = new Professor
            {
                FirstName = firstName,
                LastName = lastName,
                UniversityIndex = indeks,
                AcademicTitle = academicTitle,
                ResidenceAddress = address,
            };

            await _repository.AddProfessorAsync(professor);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return professor;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Professor?> GetProfessorByIdAsync(int id)
    {
        return await _repository.GetProfessorByIdAsync(id);
    }

    public async Task<IEnumerable<Professor>> GetAllProfessorsAsync()
    {
        return await _repository.GetAllProfessorsAsync();
    }

    public async Task UpdateProfessorAsync(Professor professor)
    {
        await _repository.UpdateProfessorAsync(professor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteProfessorAsync(int id)
    {
        var professor = await _repository.GetProfessorByIdAsync(id);
        if (professor == null)
            throw new InvalidOperationException($"Professor with ID {id} does not exist.");

        // Extract prefix from UniversityIndex (e.g., "P101" -> "P", "PD5" -> "PD")
        var prefix = ExtractPrefix(professor.UniversityIndex);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _indexCounterService.TryDecrementIndexAsync(
                prefix,
                professor.UniversityIndex,
                manageTransaction: false
            );
            await _repository.DeleteProfessorAsync(professor);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static string ExtractPrefix(string universityIndex)
    {
        // Extract non-digit prefix from index (e.g., "P101" -> "P", "PH123" -> "PH")
        int i = 0;
        while (i < universityIndex.Length && !char.IsDigit(universityIndex[i]))
        {
            i++;
        }
        return universityIndex.Substring(0, i).ToUpper();
    }

    public async Task AssignOfficeAsync(int professorId, string officeNumber, string building)
    {
        var professor = await _repository.GetProfessorByIdAsync(professorId);

        if (professor == null)
            throw new InvalidOperationException($"Professor with ID {professorId} does not exist.");

        if (professor.Office != null)
            throw new InvalidOperationException($"Professor already has an assigned office.");

        var office = new Office
        {
            OfficeNumber = officeNumber,
            Building = building,
            ProfessorId = professorId,
        };

        await _officeRepository.AddOfficeAsync(office);
        await _unitOfWork.SaveChangesAsync();
    }
}
