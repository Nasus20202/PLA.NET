using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUniversityRepository _repository;

    public CourseService(IUniversityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Course> CreateCourseAsync(string nazwa, string kodCourseu, int punktyECTS, int wydzialId, int? profesorId = null)
    {
        var kurs = new Course
        {
            Name = nazwa,
            CourseCode = kodCourseu,
            ECTSPoints = punktyECTS,
            DepartmentId = wydzialId,
            ProfessorId = profesorId
        };

        await _repository.AddCourseAsync(kurs);
        await _repository.SaveChangesAsync();

        return kurs;
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _repository.GetCourseByIdAsync(id);
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _repository.GetAllCoursesAsync();
    }

    public async Task UpdateCourseAsync(Course kurs)
    {
        await _repository.UpdateCourseAsync(kurs);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int id)
    {
        var kurs = await _repository.GetCourseByIdAsync(id);
        if (kurs == null)
            throw new InvalidOperationException($"Course o ID {id} nie istnieje.");

        await _repository.DeleteCourseAsync(kurs);
        await _repository.SaveChangesAsync();
    }

    public async Task AddPrerequisiteAsync(int kursId, int prerequisiteId)
    {
        var kurs = await _repository.GetCourseByIdAsync(kursId);
        var prerequisite = await _repository.GetCourseByIdAsync(prerequisiteId);

        if (kurs == null || prerequisite == null)
            throw new InvalidOperationException("Course lub prererekwizyt nie istnieje.");

        if (!kurs.Prerequisites.Contains(prerequisite))
        {
            kurs.Prerequisites.Add(prerequisite);
            await _repository.UpdateCourseAsync(kurs);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task RemovePrerequisiteAsync(int kursId, int prerequisiteId)
    {
        var kurs = await _repository.GetCourseByIdAsync(kursId);
        var prerequisite = await _repository.GetCourseByIdAsync(prerequisiteId);

        if (kurs == null || prerequisite == null)
            throw new InvalidOperationException("Course lub prererekwizyt nie istnieje.");

        if (kurs.Prerequisites.Contains(prerequisite))
        {
            kurs.Prerequisites.Remove(prerequisite);
            await _repository.UpdateCourseAsync(kurs);
            await _repository.SaveChangesAsync();
        }
    }
}
