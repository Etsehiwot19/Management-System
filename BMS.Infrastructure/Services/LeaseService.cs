using BMS.Application.DTOs;
using BMS.Application.Enums;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class LeaseService : ILeaseService
    {
        private readonly AppDbContext _context;

        public LeaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaseResponseDto>> GetAllLeases()
        {
            return await _context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        public async Task<LeaseResponseDto> GetLeaseById(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null) throw new Exception("Lease not found");
            return MapToDto(lease);
        }

        public async Task<IEnumerable<LeaseResponseDto>> GetLeasesByTenant(int tenantId)
        {
            return await _context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.TenantId == tenantId)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaseResponseDto>> GetLeasesByUnit(int unitId)
        {
            return await _context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .Where(l => l.UnitId == unitId)
                .Select(l => MapToDto(l))
                .ToListAsync();
        }

        public async Task<LeaseResponseDto> CreateLease(CreateLeaseDto dto)
        {
            // Check if unit exists and is available
            var unit = await _context.Units.FindAsync(dto.UnitId);
            if (unit == null) throw new Exception("Unit not found");
            if (unit.Status != UnitStatus.Available)
                throw new Exception($"Unit is not available. Current status: {unit.Status}");

            // Check tenant exists and is active
            var tenant = await _context.Tenants.FindAsync(dto.TenantId);
            if (tenant == null) throw new Exception("Tenant not found");
            if (!tenant.IsActive) throw new Exception("Tenant is not active");

            if (dto.StartDate >= dto.EndDate)
                throw new Exception("Start date must be before end date");

            var lease = new Lease
            {
                UnitId = dto.UnitId,
                TenantId = dto.TenantId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MonthlyRent = dto.MonthlyRent,
                DepositAmount = dto.DepositAmount,
                Status = LeaseStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Leases.Add(lease);

            // Update unit status to occupied
            unit.Status = UnitStatus.Occupied;
            unit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(lease).Reference(l => l.Unit).LoadAsync();
            await _context.Entry(lease).Reference(l => l.Tenant).LoadAsync();

            return MapToDto(lease);
        }

        public async Task<LeaseResponseDto> RenewLease(int id, RenewLeaseDto dto)
        {
            var lease = await _context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null) throw new Exception("Lease not found");
            if (lease.Status == LeaseStatus.Terminated)
                throw new Exception("Cannot renew a terminated lease");

            if (dto.NewEndDate <= lease.EndDate)
                throw new Exception("New end date must be after the current end date");

            lease.EndDate = dto.NewEndDate;
            lease.MonthlyRent = dto.NewMonthlyRent;
            lease.Status = LeaseStatus.Active;
            lease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(lease);
        }

        public async Task<LeaseResponseDto> TerminateLease(int id, TerminateLeaseDto dto)
        {
            var lease = await _context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null) throw new Exception("Lease not found");
            if (lease.Status == LeaseStatus.Terminated)
                throw new Exception("Lease is already terminated");

            lease.Status = LeaseStatus.Terminated;
            lease.TerminationReason = dto.TerminationReason;
            lease.UpdatedAt = DateTime.UtcNow;

            // Free up the unit
            var unit = await _context.Units.FindAsync(lease.UnitId);
            if (unit != null)
            {
                unit.Status = UnitStatus.Available;
                unit.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(lease);
        }

        private static LeaseResponseDto MapToDto(Lease l) => new LeaseResponseDto
        {
            Id = l.Id,
            UnitId = l.UnitId,
            UnitNumber = l.Unit?.UnitNumber ?? string.Empty,
            TenantId = l.TenantId,
            TenantName = l.Tenant?.OrganizationName ?? string.Empty,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            MonthlyRent = l.MonthlyRent,
            DepositAmount = l.DepositAmount,
            Status = l.Status.ToString(),
            TerminationReason = l.TerminationReason
        };
    }
}
