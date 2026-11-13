using BMS.Application.Enums;

namespace BMS.Application.DTOs
{
    public class CreateTenantDto
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public BusinessType BusinessType { get; set; }
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

    public class UpdateTenantDto
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public BusinessType BusinessType { get; set; }
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

    public class TenantResponseDto
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
