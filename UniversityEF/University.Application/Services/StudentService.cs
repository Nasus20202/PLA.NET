using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUniversityRepository _repository;
    private readonly IIndexCounterService _indexCounterService;

    public StudentService(
        IUniversityRepository repository,
        IIndexCounterService indexCounterService
    )
    {
        _repository = repository;
        _indexCounterService = indexCounterService;
    }

    public async Task<Student> CreateStudentAsync(
        string firstName,
        string lastName,
        int yearOfStudy,
        Address address
    )
    {
        // Pobierz kolejny indeks (w transakcji)
        var indeks = await _indexCounterService.GetNextIndexAsync("S");

        var student = new Student
        {
            FirstName = firstName,
            LastName = lastName,
            UniversityIndex = indeks,
            YearOfStudy = yearOfStudy,
            ResidenceAddress = address,
        };

        await _repository.AddStudentAsync(student);
        await _repository.SaveChangesAsync();

        return student;
    }

    public async Task<MasterStudent> CreateMasterStudentAsync(
        string firstName,
        string lastName,
        int yearOfStudy,
        Address address,
        string? thesisTopic = null,
        int? supervisorId = null
    )
    {
        // Pobierz kolejny indeks (w transakcji)
        var indeks = await _indexCounterService.GetNextIndexAsync("S");

        var student = new MasterStudent
        {
            FirstName = firstName,
            LastName = lastName,
            UniversityIndex = indeks,
            YearOfStudy = yearOfStudy,
            ResidenceAddress = address,
            ThesisTitle = thesisTopic,
            SupervisorId = supervisorId,
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
            throw new InvalidOperationException($"Student with ID {id} does not exist.");

        // Próbuj zmniejszyć licznik jeśli to ostatni student
        await _indexCounterService.TryDecrementIndexAsync("S", student.UniversityIndex);

        await _repository.DeleteStudentAsync(student);
        await _repository.SaveChangesAsync();
    }
}
