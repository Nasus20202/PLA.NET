namespace University.Domain.Entities;

public class MasterStudent : Student
{
    public string? ThesisTitle { get; set; }
    
    public int? SupervisorId { get; set; }
    public Professor? Supervisor { get; set; }
}
