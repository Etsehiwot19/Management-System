using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Enums;
using BMS.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class UnitService : IUnitService
    {
        private readonly AppDbContext _context;

        public UnitService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UnitResponseDto>> GetAllUnits()
        {
            return await _context.Units
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        public async Task<UnitResponseDto> GetUnitById(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) throw new Exception("Unit not found");
            return MapToDto(unit);
        }

        public async Task<UnitResponseDto> CreateUnit(CreateUnitDto dto)
        {
            var unit = new Unit
            {
                FloorNumber = dto.FloorNumber,
                UnitNumber = dto.UnitNumber,
                UnitType = dto.UnitType,
                AreaSqMeters = dto.AreaSqMeters,
                MonthlyRent = dto.MonthlyRent,
                Description = dto.Description,
                Status = UnitStatus.Available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return MapToDto(unit);
        }

        public async Task<UnitResponseDto> UpdateUnit(int id, UpdateUnitDto dto)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) throw new Exception("Unit not found");

            unit.UnitNumber = dto.UnitNumber;
            unit.UnitType = dto.UnitType;
            unit.AreaSqMeters = dto.AreaSqMeters;
            unit.MonthlyRent = dto.MonthlyRent;
            unit.Description = dto.Description;
            unit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(unit);
        }

        public async Task DeleteUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) throw new Exception("Unit not found");

            var hasLease = await _context.Leases
                .AnyAsync(l => l.UnitId == id);
            if (hasLease)
                throw new Exception("Cannot delete a unit with existing leases");

            unit.IsDeleted = true;
            unit.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeUnitStatus(int id, UnitStatus status)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) throw new Exception("Unit not found");

            unit.Status = status;
            unit.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UnitResponseDto>> GetUnitsByFloor(int floorNumber)
        {
            return await _context.Units
                .Where(u => u.FloorNumber == floorNumber)
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        private static UnitResponseDto MapToDto(Unit u) => new UnitResponseDto
        {
            Id = u.Id,
            FloorNumber = u.FloorNumber,
            UnitNumber = u.UnitNumber,
            UnitType = u.UnitType.ToString(),
            AreaSqMeters = u.AreaSqMeters,
            MonthlyRent = u.MonthlyRent,
            Status = u.Status.ToString(),
            Description = u.Description
        };
    }
}
