using ContractManagement.Domain.Drivers;
using ContractManagement.Domain.Identity;

namespace ContractManagement.Domain.Companies;

public class CompanyProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CompanyName { get; set; } = string.Empty;

    public string TaxCode { get; set; } = string.Empty;

    public string? BusinessLicenseNumber { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }


    // Người đại diện pháp luật
    public string RepresentativeName { get; set; } = string.Empty;
    // Chức vụ của người đại diện pháp luật
    public string? RepresentativePosition { get; set; }
    // Số CMND/CCCD/Hộ chiếu của người đại diện pháp luật
    public string RepresentativeCitizenId { get; set; } = string.Empty;
    // Ngày cấp CMND/CCCD/Hộ chiếu của người đại diện pháp luật
    public DateTime? RepresentativeCitizenIdIssuedDate { get; set; }
    // Nơi cấp CMND/CCCD/Hộ chiếu của người đại diện pháp luật
    public string? RepresentativeCitizenIdIssuedPlace { get; set; }


    // Thông tin thanh toán
    public string? BankAccountNumber { get; set; }

    public string? BankName { get; set; }

    public bool IsActive { get; set; } = true;

    //Hệ thống chỉ có một hồ sơ doanh nghiệp, IsDefault không bắt buộc. Nhưng nếu sau này có nhiều đơn vị hoặc nhiều pháp nhân, IsDefault giúp chọn nhanh hồ sơ mặc định khi tạo hợp đồng
    public bool IsDefault { get; set; }

    // Tài khoản quản lý hồ sơ doanh nghiệp
    public string ManagedByUserId { get; set; } = string.Empty;

    public ApplicationUser ManagedByUser { get; set; } = null!;
    // Một công ty có nhiều tài xế
    public ICollection<DriverProfile> DriverProfiles { get; set; }
        = new List<DriverProfile>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
