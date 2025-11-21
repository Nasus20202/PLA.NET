using System.Linq;
using System.Threading.Tasks;
using University.Domain.Entities;
using University.Infrastructure.Data.Repositories;
using Xunit;

namespace University.Infrastructure.Tests.Repositories;

public class OfficeRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddOfficeAsync_AddsOfficeSuccessfully()
    {
        // Arrange
        using var ctx = NewContext();
        var prof = new Professor
        {
            FirstName = "John",
            LastName = "Doe",
            UniversityIndex = "P1",
            AcademicTitle = "Dr",
        };
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
        Assert.Equal("A", fetched.Building);
    }

    [Fact]
    public async Task GetOfficeByIdAsync_ReturnsOffice_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var prof = new Professor
        {
            FirstName = "Jane",
            LastName = "Smith",
            UniversityIndex = "P2",
            AcademicTitle = "Prof",
        };
        ctx.Professors.Add(prof);
        await ctx.SaveChangesAsync();

        var office = new Office
        {
            ProfessorId = prof.Id,
            OfficeNumber = "202",
            Building = "B",
        };
        ctx.Offices.Add(office);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo = new OfficeRepository(ctx2);

        // Act
        var result = await repo.GetOfficeByIdAsync(office.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("202", result!.OfficeNumber);
        Assert.Equal("B", result.Building);
        Assert.NotNull(result.Professor);
    }

    [Fact]
    public async Task GetOfficeByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new OfficeRepository(ctx);

        // Act
        var result = await repo.GetOfficeByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOfficeByProfessorIdAsync_ReturnsOffice_WhenExists()
    {
        // Arrange
        using var ctx = NewContext();
        var prof = new Professor
        {
            FirstName = "Bob",
            LastName = "Johnson",
            UniversityIndex = "P3",
            AcademicTitle = "Dr",
        };
        ctx.Professors.Add(prof);
        await ctx.SaveChangesAsync();

        var office = new Office
        {
            ProfessorId = prof.Id,
            OfficeNumber = "303",
            Building = "C",
        };
        ctx.Offices.Add(office);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo = new OfficeRepository(ctx2);

        // Act
        var result = await repo.GetOfficeByProfessorIdAsync(prof.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("303", result!.OfficeNumber);
        Assert.Equal(prof.Id, result.ProfessorId);
    }

    [Fact]
    public async Task GetOfficeByProfessorIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        using var ctx = NewContext();
        var repo = new OfficeRepository(ctx);

        // Act
        var result = await repo.GetOfficeByProfessorIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOfficesAsync_ReturnsAllOffices()
    {
        // Arrange
        using var ctx = NewContext();
        var prof1 = new Professor
        {
            FirstName = "Alice",
            LastName = "Brown",
            UniversityIndex = "P4",
            AcademicTitle = "Dr",
        };
        var prof2 = new Professor
        {
            FirstName = "Charlie",
            LastName = "Davis",
            UniversityIndex = "P5",
            AcademicTitle = "Prof",
        };
        ctx.Professors.AddRange(prof1, prof2);
        await ctx.SaveChangesAsync();

        var office1 = new Office
        {
            ProfessorId = prof1.Id,
            OfficeNumber = "401",
            Building = "D",
        };
        var office2 = new Office
        {
            ProfessorId = prof2.Id,
            OfficeNumber = "402",
            Building = "D",
        };
        ctx.Offices.AddRange(office1, office2);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var repo = new OfficeRepository(ctx2);

        // Act
        var result = (await repo.GetAllOfficesAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, o => o.OfficeNumber == "401");
        Assert.Contains(result, o => o.OfficeNumber == "402");
    }

    [Fact]
    public async Task UpdateOfficeAsync_UpdatesOfficeSuccessfully()
    {
        // Arrange
        using var ctx = NewContext();
        var prof = new Professor
        {
            FirstName = "Eve",
            LastName = "Wilson",
            UniversityIndex = "P6",
            AcademicTitle = "Dr",
        };
        ctx.Professors.Add(prof);
        await ctx.SaveChangesAsync();

        var office = new Office
        {
            ProfessorId = prof.Id,
            OfficeNumber = "501",
            Building = "E",
        };
        ctx.Offices.Add(office);
        await ctx.SaveChangesAsync();

        // Act
        office.OfficeNumber = "502";
        office.Building = "F";
        var repo = new OfficeRepository(ctx);
        await repo.UpdateOfficeAsync(office);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var updated = await ctx2.Offices.FindAsync(office.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("502", updated!.OfficeNumber);
        Assert.Equal("F", updated.Building);
    }

    [Fact]
    public async Task DeleteOfficeAsync_DeletesOfficeSuccessfully()
    {
        // Arrange
        using var ctx = NewContext();
        var prof = new Professor
        {
            FirstName = "Frank",
            LastName = "Miller",
            UniversityIndex = "P7",
            AcademicTitle = "Prof",
        };
        ctx.Professors.Add(prof);
        await ctx.SaveChangesAsync();

        var office = new Office
        {
            ProfessorId = prof.Id,
            OfficeNumber = "601",
            Building = "G",
        };
        ctx.Offices.Add(office);
        await ctx.SaveChangesAsync();

        var officeId = office.Id;

        // Act
        var repo = new OfficeRepository(ctx);
        await repo.DeleteOfficeAsync(office);
        await ctx.SaveChangesAsync();

        using var ctx2 = NewContext();
        var deleted = await ctx2.Offices.FindAsync(officeId);

        // Assert
        Assert.Null(deleted);
    }
}
