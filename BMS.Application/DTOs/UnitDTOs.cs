using BMS.Application.Enums;

namespace BMS.Application.DTOs
{
    public class CreateUnitDto
    {
        public int FloorNumber { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public UnitType UnitType { get; set; }
        public decimal AreaSqMeters { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateUnitDto
    {
        public string UnitNumber { get; set; } = string.Empty;
        public UnitType UnitType { get; set; }
        public decimal AreaSqMeters { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UnitResponseDto
    {
        public int Id { get; set; }
        public int FloorNumber { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public decimal AreaSqMeters { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
