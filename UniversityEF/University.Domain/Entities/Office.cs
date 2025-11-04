namespace University.Domain.Entities;

public class Office
{
    public int Id { get; set; }
    public string OfficeNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    
    public int ProfessorId { get; set; }
    public Professor Professor { get; set; } = null!;
}
