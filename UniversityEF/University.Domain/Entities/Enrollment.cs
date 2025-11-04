namespace University.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public double? Grade { get; set; }
    public int Semester { get; set; }
}
