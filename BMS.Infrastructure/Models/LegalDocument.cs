using BMS.Application.Enums;


namespace BMS.Infrastructure.Models
{
    public class LegalDocument : BaseEntity
    {
        public int TenantId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsVerified { get; set; } = false;

        // Navigation property
        public Tenant Tenant { get; set; } = null!;
    }
}
