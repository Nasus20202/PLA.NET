using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class OfficeRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddOffice_AssignsToProfessor()
    {
        // Arrange
        using var ctx = NewContext();
        var prof = new Professor { FirstName = "P" };
        ctx.Professors.Add(prof);
        await ctx.SaveChangesAsync();

        var repo = new OfficeRepository(ctx);
        var office = new Office
        {
            ProfessorId = prof.Id,
            OfficeNumber = "101",
            Building = "A",
        };

        // Act
        await repo.AddOfficeAsync(office);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var fetched = ctx2.Offices.FirstOrDefault(o => o.ProfessorId == prof.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("101", fetched!.OfficeNumber);
    }
}
