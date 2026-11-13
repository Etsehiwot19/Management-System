using BMS.Application.Enums;

namespace BMS.Application.DTOs
{
    public class CreateLeaseDto
    {
        public int UnitId { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal DepositAmount { get; set; }
    }

    public class RenewLeaseDto
    {
        public DateTime NewEndDate { get; set; }
        public decimal NewMonthlyRent { get; set; }
    }

    public class TerminateLeaseDto
    {
        public string TerminationReason { get; set; } = string.Empty;
    }

    public class LeaseResponseDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal DepositAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TerminationReason { get; set; } = string.Empty;
    }
}
