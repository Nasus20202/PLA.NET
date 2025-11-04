using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IStudentService
{
    Task<Student> CreateStudentAsync(string imie, string nazwisko, int rokStudiow, Address adres);
    Task<MasterStudent> CreateMasterStudentAsync(string imie, string nazwisko, int rokStudiow, Address adres, string? tematPracy = null, int? promotorId = null);
    Task<Student?> GetStudentByIdAsync(int id);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(int id);
}
