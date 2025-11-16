using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IQueryRepository
{
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(Func<IQueryable<Student>, IQueryable<T>> query);
    Task<IEnumerable<T>> ExecuteProfessorQueryAsync<T>(
        Func<IQueryable<Professor>, IQueryable<T>> query
    );
    Task<IEnumerable<T>> ExecuteCourseQueryAsync<T>(Func<IQueryable<Course>, IQueryable<T>> query);
}
