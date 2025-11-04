using University.Domain.Entities;

namespace University.Application.Interfaces;

public interface IUniversityRepository
{
    Task<Student?> GetStudentByIdAsync(int id);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task AddStudentAsync(Student student);
    Task UpdateStudentAsync(Student student);
    Task DeleteStudentAsync(Student student);

    Task<Professor?> GetProfessorByIdAsync(int id);
    Task<IEnumerable<Professor>> GetAllProfessorsAsync();
    Task AddProfessorAsync(Professor professor);
    Task UpdateProfessorAsync(Professor professor);
    Task DeleteProfessorAsync(Professor professor);

    Task<Course?> GetCourseByIdAsync(int id);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task AddCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(Course course);

    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task AddDepartmentAsync(Department department);
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(Department department);

    Task<Enrollment?> GetEnrollmentByIdAsync(int id);
    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
    Task AddEnrollmentAsync(Enrollment enrollment);
    Task UpdateEnrollmentAsync(Enrollment enrollment);
    Task DeleteEnrollmentAsync(Enrollment enrollment);

    Task AddOfficeAsync(Office office);

    Task<IndexCounter?> GetIndexCounterAsync(string prefix);
    Task<IEnumerable<IndexCounter>> GetAllIndexCountersAsync();
    Task AddIndexCounterAsync(IndexCounter counter);
    Task UpdateIndexCounterAsync(IndexCounter counter);
    Task DeleteIndexCounterAsync(IndexCounter counter);

    Task<IEnumerable<T>> ExecuteQueryAsync<T>(Func<IQueryable<Student>, IQueryable<T>> query);
    Task<IEnumerable<T>> ExecuteProfessorQueryAsync<T>(
        Func<IQueryable<Professor>, IQueryable<T>> query
    );
    Task<IEnumerable<T>> ExecuteCourseQueryAsync<T>(Func<IQueryable<Course>, IQueryable<T>> query);

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveChangesAsync();
}
