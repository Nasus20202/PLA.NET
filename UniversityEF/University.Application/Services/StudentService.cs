using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IIndexCounterService _indexCounterService;
    private readonly IUnitOfWork _unitOfWork;

    public StudentService(
        IStudentRepository studentRepository,
        IIndexCounterService indexCounterService,
        IUnitOfWork unitOfWork
    )
    {
        _studentRepository = studentRepository;
        _indexCounterService = indexCounterService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Student> CreateStudentAsync(
        string firstName,
        string lastName,
        int yearOfStudy,
        Address address
    )
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var indeks = await _indexCounterService.GetNextIndexAsync(
                "S",
                manageTransaction: false
            );

            var student = new Student
            {
                FirstName = firstName,
                LastName = lastName,
                UniversityIndex = indeks,
                YearOfStudy = yearOfStudy,
                ResidenceAddress = address,
            };

            await _studentRepository.AddStudentAsync(student);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return student;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
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
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var indeks = await _indexCounterService.GetNextIndexAsync(
                "S",
                manageTransaction: false
            );

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

            await _studentRepository.AddStudentAsync(student);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return student;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _studentRepository.GetStudentByIdAsync(id);
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await _studentRepository.GetAllStudentsAsync();
    }

    public async Task UpdateStudentAsync(Student student)
    {
        await _studentRepository.UpdateStudentAsync(student);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _studentRepository.GetStudentByIdAsync(id);
        if (student == null)
            throw new InvalidOperationException($"Student with ID {id} does not exist.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _indexCounterService.TryDecrementIndexAsync(
                "S",
                student.UniversityIndex,
                manageTransaction: false
            );
            await _studentRepository.DeleteStudentAsync(student);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
