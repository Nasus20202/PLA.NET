using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace University.Infrastructure.Data;

/// <summary>
/// Factory for creating the DbContext at design time (for migrations)
/// </summary>
public class UniversityDbContextFactory : IDesignTimeDbContextFactory<UniversityDbContext>
{
    public UniversityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UniversityDbContext>();

        // We use SQLite with a database file in the project root
        optionsBuilder.UseSqlite("Data Source=university.db");

        return new UniversityDbContext(optionsBuilder.Options);
    }
}
