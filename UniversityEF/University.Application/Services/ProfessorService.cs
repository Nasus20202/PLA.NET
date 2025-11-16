using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class ProfessorService : IProfessorService
{
    private readonly IUniversityRepository _repository;
    private readonly IIndexCounterService _indexCounterService;

    public ProfessorService(
        IUniversityRepository repository,
        IIndexCounterService indexCounterService
    )
    {
        _repository = repository;
        _indexCounterService = indexCounterService;
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
        await _repository.SaveChangesAsync();

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
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteProfessorAsync(int id)
    {
        var professor = await _repository.GetProfessorByIdAsync(id);
        if (professor == null)
            throw new InvalidOperationException($"Professor with ID {id} does not exist.");

        await _indexCounterService.TryDecrementIndexAsync("P", professor.UniversityIndex);

        await _repository.DeleteProfessorAsync(professor);
        await _repository.SaveChangesAsync();
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

        await _repository.AddOfficeAsync(office);
        await _repository.SaveChangesAsync();
    }
}
