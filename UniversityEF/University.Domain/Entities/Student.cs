namespace University.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public int YearOfStudy { get; set; }

    public Address ResidenceAddress { get; set; } = new Address();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
