using BMS.Application.Enums;


namespace BMS.Infrastructure.Models
{
    public class Invoice : BaseEntity
    {
        public int LeaseId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime IssueDate { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }

        // Navigation properties
        public Lease Lease { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
