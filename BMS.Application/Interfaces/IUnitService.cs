using BMS.Application.DTOs;
using BMS.Application.Enums;


namespace BMS.Application.Interfaces
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitResponseDto>> GetAllUnits();
        Task<UnitResponseDto> GetUnitById(int id);
        Task<UnitResponseDto> CreateUnit(CreateUnitDto dto);
        Task<UnitResponseDto> UpdateUnit(int id, UpdateUnitDto dto);
        Task DeleteUnit(int id);
        Task ChangeUnitStatus(int id, UnitStatus status);
        Task<IEnumerable<UnitResponseDto>> GetUnitsByFloor(int floorNumber);
    }
}
