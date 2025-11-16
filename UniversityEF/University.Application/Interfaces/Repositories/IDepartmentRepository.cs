using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task AddDepartmentAsync(Department department);
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(Department department);
}
