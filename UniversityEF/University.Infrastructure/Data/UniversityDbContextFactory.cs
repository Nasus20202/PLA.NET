using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace University.Infrastructure.Data;

/// <summary>
/// Factory do tworzenia DbContext w czasie designu (dla migracji)
/// </summary>
public class UniversityDbContextFactory : IDesignTimeDbContextFactory<UniversityDbContext>
{
    public UniversityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UniversityDbContext>();
        
        // Używamy SQLite z bazą w głównym folderze projektu
        optionsBuilder.UseSqlite("Data Source=university.db");

        return new UniversityDbContext(optionsBuilder.Options);
    }
}
