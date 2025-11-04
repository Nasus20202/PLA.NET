using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUniversityRepository _repository;

    public DepartmentService(IUniversityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Department> CreateDepartmentAsync(string nazwa)
    {
        var department = new Department { Name = nazwa };
        await _repository.AddDepartmentAsync(department);
        await _repository.SaveChangesAsync();
        return department;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _repository.GetDepartmentByIdAsync(id);
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _repository.GetAllDepartmentsAsync();
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        await _repository.UpdateDepartmentAsync(department);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _repository.GetDepartmentByIdAsync(id);
        if (department == null)
            throw new InvalidOperationException($"Department with ID {id} does not exist.");

        await _repository.DeleteDepartmentAsync(department);
        await _repository.SaveChangesAsync();
    }
}
