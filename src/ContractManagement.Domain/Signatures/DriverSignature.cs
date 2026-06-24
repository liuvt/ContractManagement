using ContractManagement.Domain.Common;
using ContractManagement.Domain.Identity;
namespace ContractManagement.Domain.Signatures;
public class DriverSignature : BaseEntity
{
    public string DriverId { get; set; } = string.Empty;
    public string SignatureFileUrl { get; set; } = string.Empty;
    public string? SignatureVectorJson { get; set; }
    public string SignatureHash { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? RevokedAt { get; set; }
    public ApplicationUser Driver { get; set; } = null!;
}
