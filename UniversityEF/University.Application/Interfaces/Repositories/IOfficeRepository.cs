using University.Domain.Entities;

namespace University.Application.Interfaces.Repositories;

public interface IOfficeRepository
{
    Task AddOfficeAsync(Office office);
}
