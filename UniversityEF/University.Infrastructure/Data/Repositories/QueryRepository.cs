using Microsoft.EntityFrameworkCore;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Infrastructure.Data.Repositories;

public class QueryRepository : IQueryRepository
{
    private readonly UniversityDbContext _context;

    public QueryRepository(UniversityDbContext context)
    {
        _context = context;
    }

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
}
