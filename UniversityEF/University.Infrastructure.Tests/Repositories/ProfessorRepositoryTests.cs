using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class ProfessorRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetProfessorByIdAsync_ReturnsProfessor_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professor = new Professor
        {
            FirstName = "John",
            LastName = "Doe",
            UniversityIndex = "P001",
        };
        await repo.AddProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);

        // Act
        var fetched = await repo2.GetProfessorByIdAsync(professor.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("John", fetched!.FirstName);
        Assert.Equal("Doe", fetched.LastName);
    }

    [Fact]
    public async Task GetProfessorByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);

        // Act
        var fetched = await repo.GetProfessorByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllProfessorsAsync_ReturnsAllProfessors()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professor1 = new Professor { FirstName = "Alice", UniversityIndex = "P101" };
        var professor2 = new Professor { FirstName = "Bob", UniversityIndex = "P102" };
        await repo.AddProfessorAsync(professor1);
        await repo.AddProfessorAsync(professor2);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);

        // Act
        var allProfessors = (await repo2.GetAllProfessorsAsync()).ToList();

        // Assert
        Assert.Equal(2, allProfessors.Count);
        Assert.Contains(allProfessors, p => p.FirstName == "Alice");
        Assert.Contains(allProfessors, p => p.FirstName == "Bob");
    }

    [Fact]
    public async Task AddProfessorAsync_AddsProfessor()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professor = new Professor
        {
            FirstName = "Charlie",
            LastName = "Brown",
            UniversityIndex = "P200",
        };

        // Act
        await repo.AddProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);
        var fetched = await repo2.GetProfessorByIdAsync(professor.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Charlie", fetched!.FirstName);
    }

    [Fact]
    public async Task AddProfessorsAsync_AddsMultipleProfessors()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professors = new List<Professor>
        {
            new Professor
            {
                FirstName = "Alice",
                LastName = "Smith",
                UniversityIndex = "P101",
            },
            new Professor
            {
                FirstName = "Bob",
                LastName = "Jones",
                UniversityIndex = "P102",
            },
            new Professor
            {
                FirstName = "Carol",
                LastName = "White",
                UniversityIndex = "P103",
            },
        };

        // Act
        await repo.AddProfessorsAsync(professors);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);
        var allProfessors = (await repo2.GetAllProfessorsAsync()).ToList();

        // Assert
        Assert.Equal(3, allProfessors.Count);
        Assert.Contains(allProfessors, p => p.FirstName == "Alice");
        Assert.Contains(allProfessors, p => p.FirstName == "Bob");
        Assert.Contains(allProfessors, p => p.FirstName == "Carol");
    }

    [Fact]
    public async Task UpdateProfessorAsync_UpdatesProfessor()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professor = new Professor { FirstName = "Original", UniversityIndex = "P300" };
        await repo.AddProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        // Act
        professor.FirstName = "Updated";
        await repo.UpdateProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);
        var fetched = await repo2.GetProfessorByIdAsync(professor.Id);

        // Assert
        Assert.Equal("Updated", fetched!.FirstName);
    }

    [Fact]
    public async Task DeleteProfessorAsync_DeletesProfessor()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professor = new Professor { FirstName = "ToDelete", UniversityIndex = "P400" };
        await repo.AddProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);
        var fetched = await repo2.GetProfessorByIdAsync(professor.Id);

        // Assert
        Assert.Null(fetched);
    }
}
