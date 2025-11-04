using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IProfessorService
{
    Task<Professor> CreateProfessorAsync(string imie, string nazwisko, string tytulNaukowy, Address adres);
    Task<Professor?> GetProfessorByIdAsync(int id);
    Task<IEnumerable<Professor>> GetAllProfessorsAsync();
    Task UpdateProfessorAsync(Professor profesor);
    Task DeleteProfessorAsync(int id);
    Task AssignOfficeAsync(int profesorId, string numerOfficeu, string budynek);
}
