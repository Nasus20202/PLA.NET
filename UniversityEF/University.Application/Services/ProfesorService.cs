using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class ProfessorService : IProfessorService
{
    private readonly IUniversityRepository _repository;
    private readonly IIndexCounterService _indexCounterService;

    public ProfessorService(IUniversityRepository repository, IIndexCounterService indexCounterService)
    {
        _repository = repository;
        _indexCounterService = indexCounterService;
    }

    public async Task<Professor> CreateProfessorAsync(string imie, string nazwisko, string tytulNaukowy, Address adres)
    {
        var indeks = await _indexCounterService.GetNextIndexAsync("P");

        var profesor = new Professor
        {
            FirstName = imie,
            LastName = nazwisko,
            UniversityIndex = indeks,
            AcademicTitle = tytulNaukowy,
            ResidenceAddress = adres
        };

        await _repository.AddProfessorAsync(profesor);
        await _repository.SaveChangesAsync();

        return profesor;
    }

    public async Task<Professor?> GetProfessorByIdAsync(int id)
    {
        return await _repository.GetProfessorByIdAsync(id);
    }

    public async Task<IEnumerable<Professor>> GetAllProfessorsAsync()
    {
        return await _repository.GetAllProfessorsAsync();
    }

    public async Task UpdateProfessorAsync(Professor profesor)
    {
        await _repository.UpdateProfessorAsync(profesor);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteProfessorAsync(int id)
    {
        var profesor = await _repository.GetProfessorByIdAsync(id);
        if (profesor == null)
            throw new InvalidOperationException($"Professor o ID {id} nie istnieje.");

        await _indexCounterService.TryDecrementIndexAsync("P", profesor.UniversityIndex);

        await _repository.DeleteProfessorAsync(profesor);
        await _repository.SaveChangesAsync();
    }

    public async Task AssignOfficeAsync(int profesorId, string numerOfficeu, string budynek)
    {
        var profesor = await _repository.GetProfessorByIdAsync(profesorId);

        if (profesor == null)
            throw new InvalidOperationException($"Professor o ID {profesorId} nie istnieje.");

        if (profesor.Office != null)
            throw new InvalidOperationException($"Professor już ma przypisany gabinet.");

        var gabinet = new Office
        {
            OfficeNumber = numerOfficeu,
            Building = budynek,
            ProfessorId = profesorId
        };

        await _repository.AddOfficeAsync(gabinet);
        await _repository.SaveChangesAsync();
    }
}
