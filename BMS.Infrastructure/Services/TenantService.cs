using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;

        public TenantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TenantResponseDto>> GetAllTenants()
        {
            return await _context.Tenants
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TenantResponseDto> GetTenantById(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) throw new Exception("Tenant not found");
            return MapToDto(tenant);
        }

        public async Task<TenantResponseDto> CreateTenant(CreateTenantDto dto)
        {
            var existing = await _context.Tenants
                .AnyAsync(t => t.TinNumber == dto.TinNumber);
            if (existing)
                throw new Exception("A tenant with this TIN number already exists");

            var tenant = new Tenant
            {
                OrganizationName = dto.OrganizationName,
                TinNumber = dto.TinNumber,
                Phone = dto.Phone,
                BusinessType = dto.BusinessType,
                ContactPersonName = dto.ContactPersonName,
                ContactEmail = dto.ContactEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return MapToDto(tenant);
        }

        public async Task<TenantResponseDto> UpdateTenant(int id, UpdateTenantDto dto)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) throw new Exception("Tenant not found");

            tenant.OrganizationName = dto.OrganizationName;
            tenant.Phone = dto.Phone;
            tenant.BusinessType = dto.BusinessType;
            tenant.ContactPersonName = dto.ContactPersonName;
            tenant.ContactEmail = dto.ContactEmail;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(tenant);
        }

        public async Task DeactivateTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) throw new Exception("Tenant not found");

            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TenantResponseDto>> SearchTenants(string searchTerm)
        {
            return await _context.Tenants
                .Where(t => t.OrganizationName.Contains(searchTerm) ||
                            t.TinNumber.Contains(searchTerm))
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        private static TenantResponseDto MapToDto(Tenant t) => new TenantResponseDto
        {
            Id = t.Id,
            OrganizationName = t.OrganizationName,
            TinNumber = t.TinNumber,
            Phone = t.Phone,
            BusinessType = t.BusinessType.ToString(),
            ContactPersonName = t.ContactPersonName,
            ContactEmail = t.ContactEmail,
            IsActive = t.IsActive
        };
    }
}
