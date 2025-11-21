using University.Application.Interfaces;
using University.Application.Interfaces.Repositories;
using University.Domain.Entities;

namespace University.Application.Services;

public class OfficeService : IOfficeService
{
    private readonly IOfficeRepository _repository;
    private readonly IProfessorRepository _professorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OfficeService(
        IOfficeRepository repository,
        IProfessorRepository professorRepository,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _professorRepository = professorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Office> CreateOfficeAsync(
        int professorId,
        string officeNumber,
        string building
    )
    {
        var professor = await _professorRepository.GetProfessorByIdAsync(professorId);
        if (professor == null)
            throw new InvalidOperationException($"Professor with ID {professorId} does not exist.");

        var existingOffice = await _repository.GetOfficeByProfessorIdAsync(professorId);
        if (existingOffice != null)
            throw new InvalidOperationException(
                $"Professor with ID {professorId} already has an assigned office."
            );

        var office = new Office
        {
            ProfessorId = professorId,
            OfficeNumber = officeNumber,
            Building = building,
        };

        await _repository.AddOfficeAsync(office);
        await _unitOfWork.SaveChangesAsync();

        return office;
    }

    public async Task<Office?> GetOfficeByIdAsync(int id)
    {
        return await _repository.GetOfficeByIdAsync(id);
    }

    public async Task<Office?> GetOfficeByProfessorIdAsync(int professorId)
    {
        return await _repository.GetOfficeByProfessorIdAsync(professorId);
    }

    public async Task<IEnumerable<Office>> GetAllOfficesAsync()
    {
        return await _repository.GetAllOfficesAsync();
    }

    public async Task UpdateOfficeAsync(Office office)
    {
        var existingOffice = await _repository.GetOfficeByIdAsync(office.Id);
        if (existingOffice == null)
            throw new InvalidOperationException($"Office with ID {office.Id} does not exist.");

        await _repository.UpdateOfficeAsync(office);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteOfficeAsync(int id)
    {
        var office = await _repository.GetOfficeByIdAsync(id);
        if (office == null)
            throw new InvalidOperationException($"Office with ID {id} does not exist.");

        await _repository.DeleteOfficeAsync(office);
        await _unitOfWork.SaveChangesAsync();
    }
}
