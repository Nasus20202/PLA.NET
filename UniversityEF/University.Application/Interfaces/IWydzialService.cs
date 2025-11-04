using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IDepartmentService
{
    Task<Department> CreateDepartmentAsync(string nazwa);
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task UpdateDepartmentAsync(Department wydzial);
    Task DeleteDepartmentAsync(int id);
}
