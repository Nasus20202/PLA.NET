using System.Threading.Tasks;
using Moq;
using University.Application.Interfaces;

namespace University.Application.Tests.Services;

public abstract class ServiceTestBase
{
    protected readonly Mock<IUnitOfWork> _mockUnit;

    protected ServiceTestBase()
    {
        _mockUnit = new Mock<IUnitOfWork>();
        _mockUnit.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mockUnit.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnit.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnit.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
    }
}
