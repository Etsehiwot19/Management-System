using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface ILeaseService
    {
        Task<IEnumerable<LeaseResponseDto>> GetAllLeases();
        Task<LeaseResponseDto> GetLeaseById(int id);
        Task<IEnumerable<LeaseResponseDto>> GetLeasesByTenant(int tenantId);
        Task<IEnumerable<LeaseResponseDto>> GetLeasesByUnit(int unitId);
        Task<LeaseResponseDto> CreateLease(CreateLeaseDto dto);
        Task<LeaseResponseDto> RenewLease(int id, RenewLeaseDto dto);
        Task<LeaseResponseDto> TerminateLease(int id, TerminateLeaseDto dto);
    }
}
