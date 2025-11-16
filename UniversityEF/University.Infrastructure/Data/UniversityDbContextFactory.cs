using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace University.Infrastructure.Data;

public class UniversityDbContextFactory : IDesignTimeDbContextFactory<UniversityDbContext>
{
    private static readonly string ConnectionString = "Data Source=university.db";

    public UniversityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UniversityDbContext>();

        optionsBuilder.UseSqlite(ConnectionString);

        return new UniversityDbContext(optionsBuilder.Options);
    }
}
