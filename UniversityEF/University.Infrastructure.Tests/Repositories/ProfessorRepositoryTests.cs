using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class ProfessorRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddProfessor_GetById_ReturnsProfessor()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new ProfessorRepository(ctx);
        var professor = new Professor { FirstName = "John", LastName = "Doe" };

        // Act
        await repo.AddProfessorAsync(professor);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new ProfessorRepository(ctx2);
        var fetched = await repo2.GetProfessorByIdAsync(professor.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("John", fetched!.FirstName);
    }
}
