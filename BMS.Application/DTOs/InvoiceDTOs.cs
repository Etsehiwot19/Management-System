namespace BMS.Application.DTOs
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime IssueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
    }

    public class GenerateInvoicesDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
