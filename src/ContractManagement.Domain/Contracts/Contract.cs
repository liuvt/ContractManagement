using ContractManagement.Domain.Common;
using ContractManagement.Domain.Customers;
using ContractManagement.Domain.Enums;
using ContractManagement.Domain.Identity;
using ContractManagement.Domain.Signatures;
namespace ContractManagement.Domain.Contracts;
public class Contract : BaseEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid ContractTypeId { get; set; }
    public Guid ContractTemplateId { get; set; }
    public Guid CustomerId { get; set; }
    public string DriverId { get; set; } = string.Empty;
    public ContractStatus Status { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public string? VehiclePlate { get; set; }
    public string? VehicleCode { get; set; }
    public string? PickupLocation { get; set; }
    public string? DropoffLocation { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? ContractValue { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Note { get; set; }
    public string ContractContentSnapshot { get; set; } = string.Empty;
    public string ContractDataJson { get; set; } = "{}";
    public string? ContractHash { get; set; }
    public string? PdfFileUrl { get; set; }
    public DateTime? CustomerSignedAt { get; set; }
    public DateTime? DriverSignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public ContractType ContractType { get; set; } = null!;
    public ContractTemplate ContractTemplate { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ApplicationUser Driver { get; set; } = null!;
    public ICollection<ContractSignature> Signatures { get; set; } = [];
    public ICollection<ContractAttachment> Attachments { get; set; } = [];
    public ICollection<ContractAuditLog> AuditLogs { get; set; } = [];
}
