using BMS.Application.DTOs;
using BMS.Application.Enums;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoices()
        {
            return await _context.Invoices
                .Include(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(i => i.Lease).ThenInclude(l => l.Unit)
                .Include(i => i.Payments)
                .OrderByDescending(i => i.IssueDate)
                .Select(i => MapToDto(i))
                .ToListAsync();
        }

        public async Task<InvoiceResponseDto> GetInvoiceById(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(i => i.Lease).ThenInclude(l => l.Unit)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) throw new Exception("Invoice not found");
            return MapToDto(invoice);
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByLease(int leaseId)
        {
            return await _context.Invoices
                .Include(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(i => i.Lease).ThenInclude(l => l.Unit)
                .Include(i => i.Payments)
                .Where(i => i.LeaseId == leaseId)
                .OrderByDescending(i => i.IssueDate)
                .Select(i => MapToDto(i))
                .ToListAsync();
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GenerateMonthlyInvoices(GenerateInvoicesDto dto)
        {
            if (dto.Month < 1 || dto.Month > 12)
                throw new Exception("Invalid month. Must be between 1 and 12");

            // Get all active leases that cover this month/year
            var periodStart = new DateTime(dto.Year, dto.Month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            var activeLeases = await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .Where(l => l.Status == LeaseStatus.Active
                         && l.StartDate <= periodEnd
                         && l.EndDate >= periodStart)
                .ToListAsync();

            var generated = new List<Invoice>();

            foreach (var lease in activeLeases)
            {
                // Skip if invoice already exists for this period
                var exists = await _context.Invoices
                    .AnyAsync(i => i.LeaseId == lease.Id
                               && i.PeriodMonth == dto.Month
                               && i.PeriodYear == dto.Year);

                if (exists) continue;

                var invoiceNumber = $"INV-{dto.Year}{dto.Month:D2}-{lease.Id:D4}";
                var dueDate = new DateTime(dto.Year, dto.Month, 5); // Due on the 5th

                var invoice = new Invoice
                {
                    LeaseId = lease.Id,
                    InvoiceNumber = invoiceNumber,
                    AmountDue = lease.MonthlyRent,
                    DueDate = dueDate,
                    IssueDate = DateTime.UtcNow,
                    Status = InvoiceStatus.Issued,
                    PeriodMonth = dto.Month,
                    PeriodYear = dto.Year,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Invoices.Add(invoice);
                generated.Add(invoice);
            }

            await _context.SaveChangesAsync();

            // Return with full navigation data
            return generated.Select(i =>
            {
                var lease = activeLeases.First(l => l.Id == i.LeaseId);
                return new InvoiceResponseDto
                {
                    Id = i.Id,
                    LeaseId = i.LeaseId,
                    InvoiceNumber = i.InvoiceNumber,
                    AmountDue = i.AmountDue,
                    AmountPaid = 0,
                    OutstandingBalance = i.AmountDue,
                    DueDate = i.DueDate,
                    IssueDate = i.IssueDate,
                    Status = i.Status.ToString(),
                    PeriodMonth = i.PeriodMonth,
                    PeriodYear = i.PeriodYear,
                    TenantName = lease.Tenant?.OrganizationName ?? string.Empty,
                    UnitNumber = lease.Unit?.UnitNumber ?? string.Empty
                };
            });
        }

        public async Task<IEnumerable<OutstandingBalanceDto>> GetOutstandingBalances()
        {
            return await _context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .Include(l => l.Invoices).ThenInclude(i => i.Payments)
                .Where(l => l.Status == LeaseStatus.Active)
                .Select(l => new OutstandingBalanceDto
                {
                    LeaseId = l.Id,
                    TenantId = l.TenantId,
                    TenantName = l.Tenant.OrganizationName,
                    UnitNumber = l.Unit.UnitNumber,
                    TotalDue = l.Invoices
                        .Where(i => i.Status != InvoiceStatus.Cancelled)
                        .Sum(i => i.AmountDue),
                    TotalPaid = l.Invoices
                        .SelectMany(i => i.Payments)
                        .Sum(p => p.AmountPaid),
                    OutstandingBalance = l.Invoices
                        .Where(i => i.Status != InvoiceStatus.Cancelled)
                        .Sum(i => i.AmountDue)
                        - l.Invoices.SelectMany(i => i.Payments).Sum(p => p.AmountPaid),
                    OverdueInvoiceCount = l.Invoices
                        .Count(i => i.Status == InvoiceStatus.Overdue)
                })
                .ToListAsync();
        }

        public async Task MarkOverdueInvoices()
        {
            var today = DateTime.UtcNow.Date;
            var overdueInvoices = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Issued && i.DueDate.Date < today)
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private static InvoiceResponseDto MapToDto(Invoice i)
        {
            var amountPaid = i.Payments?.Sum(p => p.AmountPaid) ?? 0;
            return new InvoiceResponseDto
            {
                Id = i.Id,
                LeaseId = i.LeaseId,
                InvoiceNumber = i.InvoiceNumber,
                AmountDue = i.AmountDue,
                AmountPaid = amountPaid,
                OutstandingBalance = i.AmountDue - amountPaid,
                DueDate = i.DueDate,
                IssueDate = i.IssueDate,
                Status = i.Status.ToString(),
                PeriodMonth = i.PeriodMonth,
                PeriodYear = i.PeriodYear,
                TenantName = i.Lease?.Tenant?.OrganizationName ?? string.Empty,
                UnitNumber = i.Lease?.Unit?.UnitNumber ?? string.Empty
            };
        }
    }
}
