using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class DepartmentRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetDepartmentByIdAsync_ReturnsDepartment_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var department = new Department { Name = "Physics" };
        await repo.AddDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);

        // Act
        var fetched = await repo2.GetDepartmentByIdAsync(department.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Physics", fetched!.Name);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);

        // Act
        var fetched = await repo.GetDepartmentByIdAsync(99999);

        // Assert
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var department1 = new Department { Name = "Math" };
        var department2 = new Department { Name = "CS" };
        await repo.AddDepartmentAsync(department1);
        await repo.AddDepartmentAsync(department2);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);

        // Act
        var allDepartments = (await repo2.GetAllDepartmentsAsync()).ToList();

        // Assert
        Assert.Equal(2, allDepartments.Count);
        Assert.Contains(allDepartments, d => d.Name == "Math");
        Assert.Contains(allDepartments, d => d.Name == "CS");
    }

    [Fact]
    public async Task AddDepartmentAsync_AddsDepartment()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var department = new Department { Name = "Chemistry" };

        // Act
        await repo.AddDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);
        var fetched = await repo2.GetDepartmentByIdAsync(department.Id);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal("Chemistry", fetched!.Name);
    }

    [Fact]
    public async Task AddDepartmentsAsync_AddsMultipleDepartments()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var departments = new List<Department>
        {
            new Department { Name = "Computer Science" },
            new Department { Name = "Mathematics" },
            new Department { Name = "Physics" },
        };

        // Act
        await repo.AddDepartmentsAsync(departments);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);
        var allDepartments = (await repo2.GetAllDepartmentsAsync()).ToList();

        // Assert
        Assert.Equal(3, allDepartments.Count);
        Assert.Contains(allDepartments, d => d.Name == "Computer Science");
        Assert.Contains(allDepartments, d => d.Name == "Mathematics");
        Assert.Contains(allDepartments, d => d.Name == "Physics");
    }

    [Fact]
    public async Task UpdateDepartmentAsync_UpdatesDepartment()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var department = new Department { Name = "Original Name" };
        await repo.AddDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        // Act
        department.Name = "Updated Name";
        await repo.UpdateDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);
        var fetched = await repo2.GetDepartmentByIdAsync(department.Id);

        // Assert
        Assert.Equal("Updated Name", fetched!.Name);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_DeletesDepartment()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new DepartmentRepository(ctx);
        var department = new Department { Name = "To Delete" };
        await repo.AddDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        // Act
        await repo.DeleteDepartmentAsync(department);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo2 = new DepartmentRepository(ctx2);
        var fetched = await repo2.GetDepartmentByIdAsync(department.Id);

        // Assert
        Assert.Null(fetched);
    }
}
