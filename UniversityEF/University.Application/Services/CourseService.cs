using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(ICourseRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Course> CreateCourseAsync(
        string name,
        string courseCode,
        int ectsPoints,
        int departmentId,
        int? professorId = null
    )
    {
        var course = new Course
        {
            Name = name,
            CourseCode = courseCode,
            ECTSPoints = ectsPoints,
            DepartmentId = departmentId,
            ProfessorId = professorId,
        };

        await _repository.AddCourseAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return course;
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _repository.GetCourseByIdAsync(id);
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _repository.GetAllCoursesAsync();
    }

    public async Task UpdateCourseAsync(Course course)
    {
        await _repository.UpdateCourseAsync(course);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int id)
    {
        var course = await _repository.GetCourseByIdAsync(id);
        if (course == null)
            throw new InvalidOperationException($"Course with ID {id} does not exist.");

        await _repository.DeleteCourseAsync(course);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddPrerequisiteAsync(int courseId, int prerequisiteId)
    {
        var course = await _repository.GetCourseByIdAsync(courseId);
        var prerequisite = await _repository.GetCourseByIdAsync(prerequisiteId);

        if (course == null || prerequisite == null)
            throw new InvalidOperationException("Course or prerequisite does not exist.");

        if (!course.Prerequisites.Contains(prerequisite))
        {
            course.Prerequisites.Add(prerequisite);
            await _repository.UpdateCourseAsync(course);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task RemovePrerequisiteAsync(int courseId, int prerequisiteId)
    {
        var course = await _repository.GetCourseByIdAsync(courseId);
        var prerequisite = await _repository.GetCourseByIdAsync(prerequisiteId);

        if (course == null || prerequisite == null)
            throw new InvalidOperationException("Course or prerequisite does not exist.");

        if (course.Prerequisites.Contains(prerequisite))
        {
            course.Prerequisites.Remove(prerequisite);
            await _repository.UpdateCourseAsync(course);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
