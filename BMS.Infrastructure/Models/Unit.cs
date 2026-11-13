using BMS.Application.Enums;


namespace BMS.Infrastructure.Models
{
    public class Unit : BaseEntity
    {
        public int FloorNumber { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public UnitType UnitType { get; set; }
        public decimal AreaSqMeters { get; set; }
        public decimal MonthlyRent { get; set; }
        public UnitStatus Status { get; set; } = UnitStatus.Available;
        public string Description { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
    }
}
