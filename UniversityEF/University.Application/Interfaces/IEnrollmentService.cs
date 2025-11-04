using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IEnrollmentService
{
    Task<Enrollment> EnrollStudentAsync(int studentId, int kursId, int semestr);
    Task UpdateGradeAsync(int enrollmentId, double ocena);
    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int kursId);
    Task UnenrollStudentAsync(int enrollmentId);
}
