using ContractManagement.Domain.Enums;
namespace ContractManagement.Application.Contracts;
public sealed record ContractListItemDto(Guid Id, string ContractNumber, string CustomerName, string CustomerPhone, string ContractTypeName, ContractStatus Status, decimal? ContractValue, DateTime CreatedAt);
