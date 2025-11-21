using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetStudentByIdAsync(int id);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task<int?> GetHighestIndexNumberForPrefixAsync(string prefix);
    Task AddStudentAsync(Student student);
    Task AddStudentsAsync(IEnumerable<Student> students);
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(Student student);
}
