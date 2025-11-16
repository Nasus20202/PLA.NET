using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Application.Services;
using University.Infrastructure.Data;
using University.Infrastructure.Data.Repositories;

namespace University.UI.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        ConfigureDatabase(services);
        ConfigureRepositories(services);
        ConfigureApplicationServices(services);
    }

    private static void ConfigureDatabase(IServiceCollection services)
    {
        services.AddDbContext<UniversityDbContext>(options =>
            options.UseSqlite("Data Source=university.db")
        );
    }

    private static void ConfigureRepositories(IServiceCollection services)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IProfessorRepository, ProfessorRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IIndexCounterRepository, IndexCounterRepository>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<IQueryRepository, QueryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IIndexCounterService, IndexCounterService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IProfessorService, ProfessorService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IQueryService, QueryService>();
        services.AddScoped<IDataGeneratorService, DataGeneratorService>();
    }
}
