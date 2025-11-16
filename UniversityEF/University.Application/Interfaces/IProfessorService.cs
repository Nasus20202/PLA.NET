using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IProfessorService
{
    Task<Professor> CreateProfessorAsync(
        string firstName,
        string lastName,
        string academicTitle,
        Address address
    );
    Task<Professor?> GetProfessorByIdAsync(int id);
    Task<IEnumerable<Professor>> GetAllProfessorsAsync();
    Task UpdateProfessorAsync(Professor professor);
    Task DeleteProfessorAsync(int id);
    Task AssignOfficeAsync(int professorId, string officeNumber, string building);
}
