using Bogus;
using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

/// <summary>
/// Test data generator using Bogus
/// Uses business logic to generate UniversityIndex
/// </summary>
public class DataGeneratorService
{
    private readonly IDepartmentService _departmentService;
    private readonly IProfessorService _professorService;
    private readonly IStudentService _studentService;
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IIndexCounterService _indexCounterService;

    public DataGeneratorService(
        IDepartmentService departmentService,
        IProfessorService professorService,
        IStudentService studentService,
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        IIndexCounterService indexCounterService
    )
    {
        _departmentService = departmentService;
        _professorService = professorService;
        _studentService = studentService;
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _indexCounterService = indexCounterService;
    }

    public async Task GenerateDataAsync(
        int numberOfProfessors = 10,
        int numberOfStudents = 50,
        int numberOfMasterStudents = 20
    )
    {
        await InitializeCountersAsync();
        var departments = await GenerateDepartmentsAsync();
        var professors = await GenerateProfessorsAsync(numberOfProfessors);
        var courses = await GenerateCoursesAsync(departments, professors);
        await AddPrerequisitesAsync(courses);
        var students = await GenerateStudentsAsync(numberOfStudents);
        var masterStudents = await GenerateMasterStudentsAsync(numberOfMasterStudents, professors);
        await EnrollStudentsAsync(students.Concat(masterStudents).ToList(), courses);
    }

    private async Task InitializeCountersAsync()
    {
        // Initialize counters if they don't exist
        var counterS = await _indexCounterService.GetCounterAsync("S");
        if (counterS == null)
        {
            await _indexCounterService.InitializeCounterAsync("S", 1000);
        }

        var counterP = await _indexCounterService.GetCounterAsync("P");
        if (counterP == null)
        {
            await _indexCounterService.InitializeCounterAsync("P", 100);
        }
    }

    private async Task<List<Department>> GenerateDepartmentsAsync()
    {
        var departments = new List<Department>();
        var departmentNames = new[]
        {
            "Faculty of Computer Science",
            "Faculty of Mathematics",
            "Faculty of Physics",
            "Faculty of Chemistry",
            "Faculty of Biology",
        };

        foreach (var name in departmentNames)
        {
            var department = await _departmentService.CreateDepartmentAsync(name);
            departments.Add(department);
        }

        return departments;
    }

    private async Task<List<Professor>> GenerateProfessorsAsync(int count)
    {
        var professors = new List<Professor>();
        var titles = new[] { "PhD", "PhD Habil.", "Prof." };

        var faker = new Faker("en");

        for (int i = 0; i < count; i++)
        {
            var address = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
            };

            var professor = await _professorService.CreateProfessorAsync(
                faker.Name.FirstName(),
                faker.Name.LastName(),
                faker.PickRandom(titles),
                address
            );

            // Assign an office to every second professor
            if (i % 2 == 0)
            {
                await _professorService.AssignOfficeAsync(
                    professor.Id,
                    faker.Random.Number(100, 500).ToString(),
                    faker.PickRandom("A", "B", "C", "D")
                );
            }

            professors.Add(professor);
        }

        return professors;
    }

    private async Task<List<Student>> GenerateStudentsAsync(int count)
    {
        var students = new List<Student>();
        var faker = new Faker("en");

        for (int i = 0; i < count; i++)
        {
            var address = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
            };

            var student = await _studentService.CreateStudentAsync(
                faker.Name.FirstName(),
                faker.Name.LastName(),
                faker.Random.Number(1, 3),
                address
            );

            students.Add(student);
        }

        return students;
    }

    private async Task<List<Student>> GenerateMasterStudentsAsync(
        int count,
        List<Professor> professors
    )
    {
        var students = new List<Student>();
        var faker = new Faker("en");

        for (int i = 0; i < count; i++)
        {
            var address = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
            };

            var supervisor = faker.PickRandom(professors);

            var student = await _studentService.CreateMasterStudentAsync(
                faker.Name.FirstName(),
                faker.Name.LastName(),
                faker.Random.Number(4, 5),
                address,
                faker.Lorem.Sentence(5),
                supervisor.Id
            );

            students.Add(student);
        }

        return students;
    }

    private async Task<List<Course>> GenerateCoursesAsync(
        List<Department> departments,
        List<Professor> professors
    )
    {
        var courses = new List<Course>();
        var faker = new Faker("en");

        var subjects = new[]
        {
            "Programming",
            "Algorithms",
            "Databases",
            "Computer Networks",
            "Operating Systems",
            "Software Engineering",
            "Artificial Intelligence",
            "Mathematical Analysis",
            "Linear Algebra",
            "Statistics",
            "Physics",
            "Organic Chemistry",
            "Molecular Biology",
        };

        var levels = new[] { "I", "II", "Advanced" };

        foreach (var department in departments)
        {
            for (int i = 0; i < 8; i++)
            {
                var subject = faker.PickRandom(subjects);
                var level = faker.PickRandom(levels);
                var name = $"{subject} {level}";

                // Generate a short department prefix robustly (first 3 letters from name)
                var lettersOnly = new string(department.Name.Where(char.IsLetter).ToArray());
                var prefix =
                    lettersOnly.Length >= 3
                        ? lettersOnly.Substring(0, 3).ToUpper()
                        : lettersOnly.ToUpper();
                var code = $"{prefix}{faker.Random.Number(100, 999)}";

                var professor = faker.PickRandom(professors);

                var course = await _courseService.CreateCourseAsync(
                    name,
                    code,
                    faker.Random.Number(3, 6),
                    department.Id,
                    professor.Id
                );

                courses.Add(course);
            }
        }

        return courses;
    }

    private async Task AddPrerequisitesAsync(List<Course> courses)
    {
        var faker = new Faker();

        // Add random prerequisites (~30% of courses have prerequisites)
        foreach (var course in courses.Where(k => faker.Random.Bool(0.3f)))
        {
            var prerequisitesCount = faker.Random.Number(1, 2);
            var potentialPrerequisites = courses
                .Where(k => k.Id != course.Id && k.DepartmentId == course.DepartmentId)
                .OrderBy(_ => faker.Random.Number())
                .Take(prerequisitesCount);

            foreach (var prerequisite in potentialPrerequisites)
            {
                try
                {
                    await _courseService.AddPrerequisiteAsync(course.Id, prerequisite.Id);
                }
                catch
                {
                    // Ignore if already exists
                }
            }
        }
    }

    private async Task EnrollStudentsAsync(List<Student> students, List<Course> courses)
    {
        var faker = new Faker();
        var grades = new[] { 3.0, 3.5, 4.0, 4.5, 5.0 };

        foreach (var student in students)
        {
            // Each student enrolls in 3-6 courses
            var numberOfCourses = faker.Random.Number(3, 6);
            var selectedCourses = courses.OrderBy(_ => faker.Random.Number()).Take(numberOfCourses);

            foreach (var course in selectedCourses)
            {
                try
                {
                    var enrollment = await _enrollmentService.EnrollStudentAsync(
                        student.Id,
                        course.Id,
                        faker.Random.Number(1, 2)
                    );

                    // 80% of students get a grade
                    if (faker.Random.Bool(0.8f))
                    {
                        await _enrollmentService.UpdateGradeAsync(
                            enrollment.Id,
                            faker.PickRandom(grades)
                        );
                    }
                }
                catch
                {
                    // Ignore if already enrolled
                }
            }
        }
    }
}
