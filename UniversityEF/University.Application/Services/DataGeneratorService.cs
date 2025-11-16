using Bogus;
using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class DataGeneratorService : IDataGeneratorService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IProfessorRepository _professorRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IIndexCounterRepository _indexCounterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DataGeneratorService(
        IDepartmentRepository departmentRepository,
        IProfessorRepository professorRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IIndexCounterRepository indexCounterRepository,
        IUnitOfWork unitOfWork
    )
    {
        _departmentRepository = departmentRepository;
        _professorRepository = professorRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _indexCounterRepository = indexCounterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task GenerateDataAsync(
        int numberOfProfessors = 10,
        int numberOfStudents = 50,
        int numberOfMasterStudents = 20
    )
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await InitializeCountersAsync();
            var departments = await GenerateDepartmentsAsync();
            var professors = await GenerateProfessorsAsync(numberOfProfessors);
            var courses = await GenerateCoursesAsync(departments, professors);
            await AddPrerequisitesAsync(courses);
            var students = await GenerateStudentsAsync(numberOfStudents);
            var masterStudents = await GenerateMasterStudentsAsync(
                numberOfMasterStudents,
                professors
            );
            await EnrollStudentsAsync(students.Concat(masterStudents).ToList(), courses);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task InitializeCountersAsync()
    {
        var counterS = await _indexCounterRepository.GetCounterAsync("S");
        if (counterS == null)
        {
            var newCounterS = new IndexCounter { Prefix = "S", CurrentValue = 1000 };
            await _indexCounterRepository.AddCounterAsync(newCounterS);
        }

        var counterP = await _indexCounterRepository.GetCounterAsync("P");
        if (counterP == null)
        {
            var newCounterP = new IndexCounter { Prefix = "P", CurrentValue = 100 };
            await _indexCounterRepository.AddCounterAsync(newCounterP);
        }

        await _unitOfWork.SaveChangesAsync();
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
            var department = new Department { Name = name };
            departments.Add(department);
        }

        await _departmentRepository.AddDepartmentsAsync(departments);
        await _unitOfWork.SaveChangesAsync();

        return departments;
    }

    private async Task<List<Professor>> GenerateProfessorsAsync(int count)
    {
        var professors = new List<Professor>();
        var titles = new[] { "PhD", "PhD Habil.", "Prof." };
        var faker = new Faker("en");

        // Atomically reserve a batch of indexes (thread-safe)
        var (startIndex, _) = await _indexCounterRepository.ReserveBatchAsync("P", count);

        for (int i = 0; i < count; i++)
        {
            var address = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
            };

            var office =
                i % 2 == 0
                    ? new Office
                    {
                        OfficeNumber = faker.Random.Number(100, 500).ToString(),
                        Building = faker.PickRandom("A", "B", "C", "D"),
                    }
                    : null;

            var professor = new Professor
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                AcademicTitle = faker.PickRandom(titles),
                UniversityIndex = $"P{startIndex + i}",
                ResidenceAddress = address,
                Office = office,
            };

            professors.Add(professor);
        }

        await _professorRepository.AddProfessorsAsync(professors);
        await _unitOfWork.SaveChangesAsync();

        return professors;
    }

    private async Task<List<Student>> GenerateStudentsAsync(int count)
    {
        var students = new List<Student>();
        var faker = new Faker("en");

        // Atomically reserve a batch of indexes (thread-safe)
        var (startIndex, _) = await _indexCounterRepository.ReserveBatchAsync("S", count);

        for (int i = 0; i < count; i++)
        {
            var address = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
            };

            var student = new Student
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                UniversityIndex = $"S{startIndex + i}",
                YearOfStudy = faker.Random.Number(1, 3),
                ResidenceAddress = address,
            };

            students.Add(student);
        }

        await _studentRepository.AddStudentsAsync(students);
        await _unitOfWork.SaveChangesAsync();

        return students;
    }

    private async Task<List<Student>> GenerateMasterStudentsAsync(
        int count,
        List<Professor> professors
    )
    {
        var students = new List<Student>();
        var faker = new Faker("en");

        // Atomically reserve a batch of indexes (thread-safe, continues from regular students)
        var (startIndex, _) = await _indexCounterRepository.ReserveBatchAsync("S", count);

        for (int i = 0; i < count; i++)
        {
            var address = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
            };

            var supervisor = faker.PickRandom(professors);

            var student = new MasterStudent
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                UniversityIndex = $"S{startIndex + i}",
                YearOfStudy = faker.Random.Number(4, 5),
                ResidenceAddress = address,
                ThesisTitle = faker.Lorem.Sentence(5),
                SupervisorId = supervisor.Id,
            };

            students.Add(student);
        }

        await _studentRepository.AddStudentsAsync(students);
        await _unitOfWork.SaveChangesAsync();

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

                var lettersOnly = new string(department.Name.Where(char.IsLetter).ToArray());
                var prefix =
                    lettersOnly.Length >= 3
                        ? lettersOnly.Substring(0, 3).ToUpper()
                        : lettersOnly.ToUpper();
                var code = $"{prefix}{faker.Random.Number(100, 999)}";

                var professor = faker.PickRandom(professors);

                var course = new Course
                {
                    Name = name,
                    CourseCode = code,
                    ECTSPoints = faker.Random.Number(3, 6),
                    DepartmentId = department.Id,
                    ProfessorId = professor.Id,
                };

                courses.Add(course);
            }
        }

        await _courseRepository.AddCoursesAsync(courses);
        await _unitOfWork.SaveChangesAsync();

        return courses;
    }

    private async Task AddPrerequisitesAsync(List<Course> courses)
    {
        var faker = new Faker();

        foreach (var course in courses.Where(k => faker.Random.Bool(0.3f)))
        {
            var prerequisitesCount = faker.Random.Number(1, 2);
            var potentialPrerequisites = courses
                .Where(k => k.Id != course.Id && k.DepartmentId == course.DepartmentId)
                .OrderBy(_ => faker.Random.Number())
                .Take(prerequisitesCount);

            foreach (var prerequisite in potentialPrerequisites)
            {
                if (!course.Prerequisites.Any(p => p.Id == prerequisite.Id))
                {
                    course.Prerequisites.Add(prerequisite);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnrollStudentsAsync(List<Student> students, List<Course> courses)
    {
        var faker = new Faker();
        var grades = new[] { 3.0, 3.5, 4.0, 4.5, 5.0 };
        var enrollments = new List<Enrollment>();
        var enrollmentKeys = new HashSet<(int studentId, int courseId)>();

        foreach (var student in students)
        {
            var numberOfCourses = faker.Random.Number(3, 6);
            var selectedCourses = courses.OrderBy(_ => faker.Random.Number()).Take(numberOfCourses);

            foreach (var course in selectedCourses)
            {
                var key = (student.Id, course.Id);

                // Skip if already enrolled
                if (enrollmentKeys.Contains(key))
                    continue;

                enrollmentKeys.Add(key);

                var enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    Semester = faker.Random.Number(1, 2),
                    Grade = faker.Random.Bool(0.8f) ? faker.PickRandom(grades) : null,
                };

                enrollments.Add(enrollment);
            }
        }

        await _enrollmentRepository.AddEnrollmentsAsync(enrollments);
        await _unitOfWork.SaveChangesAsync();
    }
}
