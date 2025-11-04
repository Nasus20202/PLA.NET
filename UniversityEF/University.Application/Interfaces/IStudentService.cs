using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IStudentService
{
    Task<Student> CreateStudentAsync(
        string firstName,
        string lastName,
        int yearOfStudy,
        Address address
    );
    Task<MasterStudent> CreateMasterStudentAsync(
        string firstName,
        string lastName,
        int yearOfStudy,
        Address address,
        string? thesisTopic = null,
        int? supervisorId = null
    );
    Task<Student?> GetStudentByIdAsync(int id);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(int id);
}
