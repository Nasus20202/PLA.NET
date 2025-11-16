using System.Threading.Tasks;

namespace University.Application.Interfaces;

public interface IDataGeneratorService
{
    Task GenerateDataAsync(
        int numberOfProfessors = 10,
        int numberOfStudents = 50,
        int numberOfMasterStudents = 20
    );
}
