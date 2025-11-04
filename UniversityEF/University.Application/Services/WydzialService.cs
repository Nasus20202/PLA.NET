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
        var wydzial = new Department { Name = nazwa };
        await _repository.AddDepartmentAsync(wydzial);
        await _repository.SaveChangesAsync();
        return wydzial;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _repository.GetDepartmentByIdAsync(id);
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _repository.GetAllDepartmentsAsync();
    }

    public async Task UpdateDepartmentAsync(Department wydzial)
    {
        await _repository.UpdateDepartmentAsync(wydzial);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var wydzial = await _repository.GetDepartmentByIdAsync(id);
        if (wydzial == null)
            throw new InvalidOperationException($"Wydział o ID {id} nie istnieje.");

        await _repository.DeleteDepartmentAsync(wydzial);
        await _repository.SaveChangesAsync();
    }
}
