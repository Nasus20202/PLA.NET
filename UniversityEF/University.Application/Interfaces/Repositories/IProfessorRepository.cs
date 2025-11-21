using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IProfessorRepository
{
    Task<Professor?> GetProfessorByIdAsync(int id);
    Task<IEnumerable<Professor>> GetAllProfessorsAsync();
    Task<int?> GetHighestIndexNumberForPrefixAsync(string prefix);
    Task AddProfessorAsync(Professor professor);
    Task AddProfessorsAsync(IEnumerable<Professor> professors);
    Task UpdateProfessorAsync(Professor professor);
    Task DeleteProfessorAsync(Professor professor);
}
