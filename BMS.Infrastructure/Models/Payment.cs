using BMS.Application.Enums;


namespace BMS.Infrastructure.Models
{
    public class Payment : BaseEntity
    {
        public int InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation property
        public Invoice Invoice { get; set; } = null!;
    }
}
