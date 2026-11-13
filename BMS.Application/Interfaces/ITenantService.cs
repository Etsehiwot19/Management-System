using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<TenantResponseDto>> GetAllTenants();
        Task<TenantResponseDto> GetTenantById(int id);
        Task<TenantResponseDto> CreateTenant(CreateTenantDto dto);
        Task<TenantResponseDto> UpdateTenant(int id, UpdateTenantDto dto);
        Task DeactivateTenant(int id);
        Task<IEnumerable<TenantResponseDto>> SearchTenants(string searchTerm);
    }
}
