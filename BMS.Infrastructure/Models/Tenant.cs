using BMS.Application.Enums;


namespace BMS.Infrastructure.Models
{
    public class Tenant : BaseEntity
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public BusinessType BusinessType { get; set; }
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<LegalDocument> LegalDocuments { get; set; } = new List<LegalDocument>();
    }
}
