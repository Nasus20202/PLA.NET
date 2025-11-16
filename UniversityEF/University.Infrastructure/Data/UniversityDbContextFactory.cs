using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace University.Infrastructure.Data;

public class UniversityDbContextFactory : IDesignTimeDbContextFactory<UniversityDbContext>
{
    public UniversityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UniversityDbContext>();

        optionsBuilder.UseSqlite("Data Source=university.db");

        return new UniversityDbContext(optionsBuilder.Options);
    }
}
