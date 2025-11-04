namespace University.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int ECTSPoints { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int? ProfessorId { get; set; }
    public Professor? Professor { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<Course> Prerequisites { get; set; } = new List<Course>();
    public ICollection<Course> RequiredByCourses { get; set; } = new List<Course>();
}
