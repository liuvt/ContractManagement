using ContractManagement.Application.Admins.DriverProfiles;

namespace ContractManagement.Application.Abstractions;

public interface IDriverProfileService
{
    Task<Guid> CreateAsync(
        CreateDriverProfileRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateDriverProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<DriverProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DriverProfileDto>> GetListAsync(
        DriverProfileFilter filter,
        CancellationToken cancellationToken = default);


    // Cập thông tin profile nằm trên user
    Task<Guid> UpsertByUserIdAsync(
        string userId,
        UpdateDriverProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<DriverProfileDto?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
