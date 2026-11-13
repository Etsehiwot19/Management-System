using BMS.Application.Enums;


namespace BMS.Infrastructure.Models
{
    public class Lease : BaseEntity
    {
        public int UnitId { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal DepositAmount { get; set; }
        public LeaseStatus Status { get; set; } = LeaseStatus.Active;
        public string TerminationReason { get; set; } = string.Empty;

        // Navigation properties
        public Unit Unit { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
