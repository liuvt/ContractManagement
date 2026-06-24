using ContractManagement.Application.Contracts;
namespace ContractManagement.Application.Abstractions;
public interface IContractService
{
    Task<IReadOnlyList<ContractListItemDto>> GetDriverContractsAsync(string driverId, CancellationToken cancellationToken = default);
}
