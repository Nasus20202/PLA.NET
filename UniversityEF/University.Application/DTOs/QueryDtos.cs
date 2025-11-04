namespace University.Application.DTOs;

public class ProfessorStudentCountDto
{
    public int ProfessorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public int TotalStudentCount { get; set; }
    public int CourseCount { get; set; }
}

public class CourseAverageDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public double AverageGrade { get; set; }
    public int StudentCount { get; set; }
}

public class StudentDifficultyDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UniversityIndex { get; set; } = string.Empty;
    public int CourseECTS { get; set; }
    public int PrerequisiteECTS { get; set; }
    public int TotalDifficulty { get; set; }
}
