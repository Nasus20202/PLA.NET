namespace University.Domain.Entities;

public class Professor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public string AcademicTitle { get; set; } = string.Empty;
    
    public Address ResidenceAddress { get; set; } = new Address();
    
    public Office? Office { get; set; }
    
    public ICollection<Course> TaughtCourses { get; set; } = new List<Course>();
    
    public ICollection<MasterStudent> SupervisedStudents { get; set; } = new List<MasterStudent>();
}
