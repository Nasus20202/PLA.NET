using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUniversityRepository _repository;
    private readonly IIndexCounterService _indexCounterService;

    public StudentService(IUniversityRepository repository, IIndexCounterService indexCounterService)
    {
        _repository = repository;
        _indexCounterService = indexCounterService;
    }

    public async Task<Student> CreateStudentAsync(string imie, string nazwisko, int rokStudiow, Address adres)
    {
        // Pobierz kolejny indeks (w transakcji)
        var indeks = await _indexCounterService.GetNextIndexAsync("S");

        var student = new Student
        {
            FirstName = imie,
            LastName = nazwisko,
            UniversityIndex = indeks,
            YearOfStudy = rokStudiow,
            ResidenceAddress = adres
        };

        await _repository.AddStudentAsync(student);
        await _repository.SaveChangesAsync();

        return student;
    }

    public async Task<MasterStudent> CreateMasterStudentAsync(string imie, string nazwisko, int rokStudiow, Address adres, string? tematPracy = null, int? promotorId = null)
    {
        // Pobierz kolejny indeks (w transakcji)
        var indeks = await _indexCounterService.GetNextIndexAsync("S");

        var student = new MasterStudent
        {
            FirstName = imie,
            LastName = nazwisko,
            UniversityIndex = indeks,
            YearOfStudy = rokStudiow,
            ResidenceAddress = adres,
            ThesisTitle = tematPracy,
            SupervisorId = promotorId
        };

        await _repository.AddStudentAsync(student);
        await _repository.SaveChangesAsync();

        return student;
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _repository.GetStudentByIdAsync(id);
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await _repository.GetAllStudentsAsync();
    }

    public async Task UpdateStudentAsync(Student student)
    {
        await _repository.UpdateStudentAsync(student);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _repository.GetStudentByIdAsync(id);
        if (student == null)
            throw new InvalidOperationException($"Student o ID {id} nie istnieje.");

        // Próbuj zmniejszyć licznik jeśli to ostatni student
        await _indexCounterService.TryDecrementIndexAsync("S", student.UniversityIndex);

        await _repository.DeleteStudentAsync(student);
        await _repository.SaveChangesAsync();
    }
}
