using Bogus;
using University.Application.Interfaces;
using University.Domain.Entities;

namespace University.Application.Services;

/// <summary>
/// Generator testowych danych używający Bogus
/// Wykorzystuje logikę biznesową do generowania UniversityIndex
/// </summary>
public class DataGeneratorService
{
    private readonly IDepartmentService _wydzialService;
    private readonly IProfessorService _profesorService;
    private readonly IStudentService _studentService;
    private readonly ICourseService _kursService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IIndexCounterService _indexCounterService;

    public DataGeneratorService(
        IDepartmentService wydzialService,
        IProfessorService profesorService,
        IStudentService studentService,
        ICourseService kursService,
        IEnrollmentService enrollmentService,
        IIndexCounterService indexCounterService)
    {
        _wydzialService = wydzialService;
        _profesorService = profesorService;
        _studentService = studentService;
        _kursService = kursService;
        _enrollmentService = enrollmentService;
        _indexCounterService = indexCounterService;
    }

    public async Task GenerateDataAsync(int numberOfProfessors = 10, int numberOfStudents = 50, int numberOfMasterStudents = 20)
    {
        await InitializeCountersAsync();
        var wydzialy = await GenerateDepartmentsAsync();
        var profesors = await GenerateProfessorsAsync(numberOfProfessors);
        var kursy = await GenerateCoursesAsync(wydzialy, profesors);
        await AddPrerequisitesAsync(kursy);
        var students = await GenerateStudentsAsync(numberOfStudents);
        var masterStudents = await GenerateMasterStudentsAsync(numberOfMasterStudents, profesors);
        await EnrollStudentsAsync(students.Concat(masterStudents).ToList(), kursy);
    }

    private async Task InitializeCountersAsync()
    {
        // Inicjalizuj liczniki jeśli nie istnieją
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
        var wydzialy = new List<Department>();
        var nazwyDepartmentow = new[]
        {
            "Wydział Informatyki",
            "Wydział Matematyki",
            "Wydział Fizyki",
            "Wydział Chemii",
            "Wydział Biologii"
        };

        foreach (var nazwa in nazwyDepartmentow)
        {
            var wydzial = await _wydzialService.CreateDepartmentAsync(nazwa);
            wydzialy.Add(wydzial);
        }

        return wydzialy;
    }

    private async Task<List<Professor>> GenerateProfessorsAsync(int count)
    {
        var profesors = new List<Professor>();
        var tytuly = new[] { "dr", "dr hab.", "prof. dr hab." };

        var faker = new Faker("pl");

        for (int i = 0; i < count; i++)
        {
            var adres = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode()
            };

            var profesor = await _profesorService.CreateProfessorAsync(
                faker.Name.FirstName(),
                faker.Name.LastName(),
                faker.PickRandom(tytuly),
                adres
            );

            // Przypisz gabinet co drugiemu profesorowi
            if (i % 2 == 0)
            {
                await _profesorService.AssignOfficeAsync(
                    profesor.Id,
                    faker.Random.Number(100, 500).ToString(),
                    faker.PickRandom("A", "B", "C", "D")
                );
            }

            profesors.Add(profesor);
        }

        return profesors;
    }

    private async Task<List<Student>> GenerateStudentsAsync(int count)
    {
        var students = new List<Student>();
        var faker = new Faker("pl");

        for (int i = 0; i < count; i++)
        {
            var adres = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode()
            };

            var student = await _studentService.CreateStudentAsync(
                faker.Name.FirstName(),
                faker.Name.LastName(),
                faker.Random.Number(1, 3),
                adres
            );

            students.Add(student);
        }

        return students;
    }

    private async Task<List<Student>> GenerateMasterStudentsAsync(int count, List<Professor> profesors)
    {
        var students = new List<Student>();
        var faker = new Faker("pl");

        for (int i = 0; i < count; i++)
        {
            var adres = new Address
            {
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode()
            };

            var promotor = faker.PickRandom(profesors);
            
            var student = await _studentService.CreateMasterStudentAsync(
                faker.Name.FirstName(),
                faker.Name.LastName(),
                faker.Random.Number(4, 5),
                adres,
                faker.Lorem.Sentence(5),
                promotor.Id
            );

            students.Add(student);
        }

        return students;
    }

    private async Task<List<Course>> GenerateCoursesAsync(List<Department> wydzialy, List<Professor> profesors)
    {
        var kursy = new List<Course>();
        var faker = new Faker("pl");

        var przedmioty = new[]
        {
            "Programowanie", "Algorytmy", "Bazy Danych", "Sieci Komputerowe",
            "Systemy Operacyjne", "Inżynieria Oprogramowania", "Sztuczna Inteligencja",
            "Analiza Matematyczna", "Algebra Liniowa", "Statystyka",
            "Fizyka", "Chemia Organiczna", "Biologia Molekularna"
        };

        var poziomy = new[] { "I", "II", "Zaawansowane" };

        foreach (var wydzial in wydzialy)
        {
            for (int i = 0; i < 8; i++)
            {
                var przedmiot = faker.PickRandom(przedmioty);
                var poziom = faker.PickRandom(poziomy);
                var nazwa = $"{przedmiot} {poziom}";
                var kod = $"{wydzial.Name.Substring(8, 3).ToUpper()}{faker.Random.Number(100, 999)}";

                var profesor = faker.PickRandom(profesors);

                var kurs = await _kursService.CreateCourseAsync(
                    nazwa,
                    kod,
                    faker.Random.Number(3, 6),
                    wydzial.Id,
                    profesor.Id
                );

                kursy.Add(kurs);
            }
        }

        return kursy;
    }

    private async Task AddPrerequisitesAsync(List<Course> kursy)
    {
        var faker = new Faker();

        // Dodaj losowe prererekwizyty (około 30% kursów ma prererekwizyty)
        foreach (var kurs in kursy.Where(k => faker.Random.Bool(0.3f)))
        {
            var prerequisitesCount = faker.Random.Number(1, 2);
            var potentialPrerequisites = kursy
                .Where(k => k.Id != kurs.Id && k.DepartmentId == kurs.DepartmentId)
                .OrderBy(_ => faker.Random.Number())
                .Take(prerequisitesCount);

            foreach (var prerequisite in potentialPrerequisites)
            {
                try
                {
                    await _kursService.AddPrerequisiteAsync(kurs.Id, prerequisite.Id);
                }
                catch
                {
                    // Ignore if already exists
                }
            }
        }
    }

    private async Task EnrollStudentsAsync(List<Student> students, List<Course> kursy)
    {
        var faker = new Faker();
        var oceny = new[] { 3.0, 3.5, 4.0, 4.5, 5.0 };

        foreach (var student in students)
        {
            // Każdy student zapisany na 3-6 kursów
            var numberOfCourses = faker.Random.Number(3, 6);
            var selectedCourses = kursy
                .OrderBy(_ => faker.Random.Number())
                .Take(numberOfCourses);

            foreach (var kurs in selectedCourses)
            {
                try
                {
                    var enrollment = await _enrollmentService.EnrollStudentAsync(
                        student.Id,
                        kurs.Id,
                        faker.Random.Number(1, 2)
                    );

                    // 80% studentów ma ocenę
                    if (faker.Random.Bool(0.8f))
                    {
                        await _enrollmentService.UpdateGradeAsync(
                            enrollment.Id,
                            faker.PickRandom(oceny)
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
