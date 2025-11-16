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
        var indeks = await _indexCounterService.GetNextIndexAsync("P");

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

        return professor;
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

        await _indexCounterService.TryDecrementIndexAsync("P", professor.UniversityIndex);
        await _repository.DeleteProfessorAsync(professor);
        await _unitOfWork.SaveChangesAsync();
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
