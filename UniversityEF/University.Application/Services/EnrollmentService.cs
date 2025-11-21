using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(IEnrollmentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Enrollment> EnrollStudentAsync(int studentId, int kursId, int semestr)
    {
        var enrollments = await _repository.GetEnrollmentsByStudentIdAsync(studentId);
        var existing = enrollments.FirstOrDefault(e => e.CourseId == kursId);

        if (existing != null)
            throw new InvalidOperationException("Student is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = kursId,
            Semester = semestr,
        };

        await _repository.AddEnrollmentAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return enrollment;
    }

    public async Task<Enrollment> EnrollStudentAsync(int studentId, int courseId, string semester)
    {
        if (!int.TryParse(semester, out int semesterNum))
            throw new ArgumentException("Semester must be a valid number.", nameof(semester));

        var enrollments = await _repository.GetEnrollmentsByStudentIdAsync(studentId);
        var existing = enrollments.FirstOrDefault(e => e.CourseId == courseId);

        if (existing != null)
            throw new InvalidOperationException("Student is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            Semester = semesterNum,
        };

        await _repository.AddEnrollmentAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return enrollment;
    }

    public async Task UpdateGradeAsync(int enrollmentId, double ocena)
    {
        var enrollment = await _repository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new InvalidOperationException(
                $"Enrollment with ID {enrollmentId} does not exist."
            );

        enrollment.Grade = ocena;
        await _repository.UpdateEnrollmentAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
    {
        return await _repository.GetEnrollmentsByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int kursId)
    {
        return await _repository.GetEnrollmentsByCourseIdAsync(kursId);
    }

    public async Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync()
    {
        return await _repository.GetAllEnrollmentsAsync();
    }

    public async Task UnenrollStudentAsync(int enrollmentId)
    {
        var enrollment = await _repository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new InvalidOperationException(
                $"Enrollment with ID {enrollmentId} does not exist."
            );

        await _repository.DeleteEnrollmentAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }
}
