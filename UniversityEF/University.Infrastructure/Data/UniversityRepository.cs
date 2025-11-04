using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Infrastructure.Data;

public class UniversityRepository : IUniversityRepository
{
    private readonly UniversityDbContext _context;
    private IDbContextTransaction? _transaction;

    public UniversityRepository(UniversityDbContext context)
    {
        _context = context;
    }

    // Students
    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context
            .Students.Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await _context.Students.Include(s => s.Enrollments).ToListAsync();
    }

    public Task AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        return Task.CompletedTask;
    }

    public Task UpdateStudentAsync(Student student)
    {
        _context.Students.Update(student);
        return Task.CompletedTask;
    }

    public Task DeleteStudentAsync(Student student)
    {
        _context.Students.Remove(student);
        return Task.CompletedTask;
    }

    // Professors
    public async Task<Professor?> GetProfessorByIdAsync(int id)
    {
        return await _context
            .Professors.Include(p => p.Office)
            .Include(p => p.TaughtCourses)
            .Include(p => p.SupervisedStudents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Professor>> GetAllProfessorsAsync()
    {
        return await _context.Professors.Include(p => p.Office).ToListAsync();
    }

    public Task AddProfessorAsync(Professor professor)
    {
        _context.Professors.Add(professor);
        return Task.CompletedTask;
    }

    public Task UpdateProfessorAsync(Professor professor)
    {
        _context.Professors.Update(professor);
        return Task.CompletedTask;
    }

    public Task DeleteProfessorAsync(Professor professor)
    {
        _context.Professors.Remove(professor);
        return Task.CompletedTask;
    }

    // Courses
    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _context
            .Courses.Include(k => k.Department)
            .Include(k => k.Professor)
            .Include(k => k.Prerequisites)
            .Include(k => k.Enrollments)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _context
            .Courses.Include(k => k.Department)
            .Include(k => k.Professor)
            .ToListAsync();
    }

    public Task AddCourseAsync(Course course)
    {
        _context.Courses.Add(course);
        return Task.CompletedTask;
    }

    public Task UpdateCourseAsync(Course course)
    {
        _context.Courses.Update(course);
        return Task.CompletedTask;
    }

    public Task DeleteCourseAsync(Course course)
    {
        _context.Courses.Remove(course);
        return Task.CompletedTask;
    }

    // Faculties
    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _context
            .Faculties.Include(w => w.Courses)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _context.Faculties.ToListAsync();
    }

    public Task AddDepartmentAsync(Department department)
    {
        _context.Faculties.Add(department);
        return Task.CompletedTask;
    }

    public Task UpdateDepartmentAsync(Department department)
    {
        _context.Faculties.Update(department);
        return Task.CompletedTask;
    }

    public Task DeleteDepartmentAsync(Department department)
    {
        _context.Faculties.Remove(department);
        return Task.CompletedTask;
    }

    // Enrollments
    public async Task<Enrollment?> GetEnrollmentByIdAsync(int id)
    {
        return await _context.Enrollments.FindAsync(id);
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        return await _context
            .Enrollments.Include(e => e.Course)
            .ThenInclude(k => k.Department)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseIdAsync(int kursId)
    {
        return await _context
            .Enrollments.Include(e => e.Student)
            .Where(e => e.CourseId == kursId)
            .ToListAsync();
    }

    public Task AddEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        return Task.CompletedTask;
    }

    public Task UpdateEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        return Task.CompletedTask;
    }

    public Task DeleteEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        return Task.CompletedTask;
    }

    // Offices
    // Offices

    public Task AddOfficeAsync(Office office)
    {
        _context.Offices.Add(office);
        return Task.CompletedTask;
    }

    // Index Counters
    public async Task<IndexCounter?> GetIndexCounterAsync(string prefix)
    {
        return await _context.IndexCounters.FirstOrDefaultAsync(c => c.Prefix == prefix);
    }

    public async Task<IEnumerable<IndexCounter>> GetAllIndexCountersAsync()
    {
        return await _context.IndexCounters.ToListAsync();
    }

    public Task AddIndexCounterAsync(IndexCounter counter)
    {
        _context.IndexCounters.Add(counter);
        return Task.CompletedTask;
    }

    public Task UpdateIndexCounterAsync(IndexCounter counter)
    {
        _context.IndexCounters.Update(counter);
        return Task.CompletedTask;
    }

    public Task DeleteIndexCounterAsync(IndexCounter counter)
    {
        _context.IndexCounters.Remove(counter);
        return Task.CompletedTask;
    }

    // Query services
    public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(
        Func<IQueryable<Student>, IQueryable<T>> query
    )
    {
        return await query(_context.Students.AsNoTracking()).ToListAsync();
    }

    public async Task<IEnumerable<T>> ExecuteProfessorQueryAsync<T>(
        Func<IQueryable<Professor>, IQueryable<T>> query
    )
    {
        return await query(_context.Professors.AsNoTracking()).ToListAsync();
    }

    public async Task<IEnumerable<T>> ExecuteCourseQueryAsync<T>(
        Func<IQueryable<Course>, IQueryable<T>> query
    )
    {
        return await query(_context.Courses.AsNoTracking()).ToListAsync();
    }

    // Transaction support
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
