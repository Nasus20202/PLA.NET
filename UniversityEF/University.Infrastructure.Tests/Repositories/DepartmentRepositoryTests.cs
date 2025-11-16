using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class DepartmentRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddDepartment_GetRoundtrip()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var department = new Department { Name = "Physics" };

        // Act
        await repo.AddDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);
        var fetched = await repo2.GetDepartmentByIdAsync(department.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Physics", fetched!.Name);
    }
}
