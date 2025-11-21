using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetEnrollmentByIdAsync(int id);
    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
    Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync();
    Task AddEnrollmentAsync(Enrollment enrollment);
    Task AddEnrollmentsAsync(IEnumerable<Enrollment> enrollments);
    Task UpdateEnrollmentAsync(Enrollment enrollment);
    Task DeleteEnrollmentAsync(Enrollment enrollment);
}
