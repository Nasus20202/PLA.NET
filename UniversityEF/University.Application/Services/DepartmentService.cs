using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IDepartmentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Department> CreateDepartmentAsync(string nazwa)
    {
        var department = new Department { Name = nazwa };
        await _repository.AddDepartmentAsync(department);
        await _unitOfWork.SaveChangesAsync();
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
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _repository.GetDepartmentByIdAsync(id);
        if (department == null)
            throw new InvalidOperationException($"Department with ID {id} does not exist.");

        await _repository.DeleteDepartmentAsync(department);
        await _unitOfWork.SaveChangesAsync();
    }
}
