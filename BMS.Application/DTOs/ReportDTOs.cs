namespace BMS.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalUnits { get; set; }
        public int AvailableUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int ActiveLeases { get; set; }
        public int ActiveTenants { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int OverdueInvoiceCount { get; set; }
        public List<RevenueReportItemDto> Revenue { get; set; } = new();
    }

    public class ReportFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int ExpiryWithinDays { get; set; } = 30;
    }

    public class OccupancyReportDto
    {
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int AvailableUnits { get; set; }
        public decimal OccupancyRate { get; set; }
    }

    public class RevenueReportItemDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class ArrearsReportItemDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class LeaseExpiryReportItemDto
    {
        public int LeaseId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
    }

    public class DocumentExpiryReportItemDto
    {
        public int DocumentId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsVerified { get; set; }
    }

    public class ReportsResponseDto
    {
        public OccupancyReportDto Occupancy { get; set; } = new();
        public List<RevenueReportItemDto> Revenue { get; set; } = new();
        public List<ArrearsReportItemDto> Arrears { get; set; } = new();
        public List<LeaseExpiryReportItemDto> LeaseExpiries { get; set; } = new();
        public List<DocumentExpiryReportItemDto> DocumentExpiries { get; set; } = new();
    }

    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
