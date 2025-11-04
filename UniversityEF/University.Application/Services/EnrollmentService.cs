using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUniversityRepository _repository;

    public EnrollmentService(IUniversityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Enrollment> EnrollStudentAsync(int studentId, int kursId, int semestr)
    {
        var enrollments = await _repository.GetEnrollmentsByStudentIdAsync(studentId);
        var existing = enrollments.FirstOrDefault(e => e.CourseId == kursId);

        if (existing != null)
            throw new InvalidOperationException("Student jest już zapisany na ten kurs.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = kursId,
            Semester = semestr
        };

        await _repository.AddEnrollmentAsync(enrollment);
        await _repository.SaveChangesAsync();

        return enrollment;
    }

    public async Task UpdateGradeAsync(int enrollmentId, double ocena)
    {
        var enrollment = await _repository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new InvalidOperationException($"Enrollment o ID {enrollmentId} nie istnieje.");

        enrollment.Grade = ocena;
        await _repository.UpdateEnrollmentAsync(enrollment);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
    {
        return await _repository.GetEnrollmentsByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int kursId)
    {
        return await _repository.GetEnrollmentsByCourseIdAsync(kursId);
    }

    public async Task UnenrollStudentAsync(int enrollmentId)
    {
        var enrollment = await _repository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new InvalidOperationException($"Enrollment o ID {enrollmentId} nie istnieje.");

        await _repository.DeleteEnrollmentAsync(enrollment);
        await _repository.SaveChangesAsync();
    }
}
