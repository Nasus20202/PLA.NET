using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetStudentByIdAsync(int id);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task AddStudentAsync(Student student);
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(Student student);
}
