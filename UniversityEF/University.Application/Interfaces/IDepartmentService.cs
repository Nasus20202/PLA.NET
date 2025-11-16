using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IDepartmentService
{
    Task<Department> CreateDepartmentAsync(string name);
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(int id);
}
