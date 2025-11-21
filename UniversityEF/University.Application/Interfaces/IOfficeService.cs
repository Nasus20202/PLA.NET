using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IOfficeService
{
    Task<Office> CreateOfficeAsync(int professorId, string officeNumber, string building);
    Task<Office?> GetOfficeByIdAsync(int id);
    Task<Office?> GetOfficeByProfessorIdAsync(int professorId);
    Task<IEnumerable<Office>> GetAllOfficesAsync();
    Task UpdateOfficeAsync(Office office);
    Task DeleteOfficeAsync(int id);
}
