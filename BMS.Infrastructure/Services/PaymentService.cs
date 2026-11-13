using BMS.Application.DTOs;
using BMS.Application.Enums;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
        {
            return await _context.Payments
                .Include(p => p.Invoice).ThenInclude(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(p => p.Invoice).ThenInclude(i => i.Lease).ThenInclude(l => l.Unit)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByInvoice(int invoiceId)
        {
            return await _context.Payments
                .Include(p => p.Invoice).ThenInclude(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(p => p.Invoice).ThenInclude(i => i.Lease).ThenInclude(l => l.Unit)
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByLease(int leaseId)
        {
            return await _context.Payments
                .Include(p => p.Invoice).ThenInclude(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(p => p.Invoice).ThenInclude(i => i.Lease).ThenInclude(l => l.Unit)
                .Where(p => p.Invoice.LeaseId == leaseId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PaymentResponseDto> RecordPayment(RecordPaymentDto dto)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .Include(i => i.Lease).ThenInclude(l => l.Tenant)
                .Include(i => i.Lease).ThenInclude(l => l.Unit)
                .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new Exception("Cannot record payment for a cancelled invoice");

            if (dto.AmountPaid <= 0)
                throw new Exception("Payment amount must be greater than zero");

            var alreadyPaid = invoice.Payments?.Sum(p => p.AmountPaid) ?? 0;
            var remaining = invoice.AmountDue - alreadyPaid;

            if (dto.AmountPaid > remaining)
                throw new Exception($"Payment amount ({dto.AmountPaid:C}) exceeds outstanding balance ({remaining:C})");

            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                AmountPaid = dto.AmountPaid,
                PaymentDate = dto.PaymentDate,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Check if invoice is now fully paid
            var newTotal = alreadyPaid + dto.AmountPaid;
            if (newTotal >= invoice.AmountDue)
                invoice.Status = InvoiceStatus.Paid;

            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Id = payment.Id,
                InvoiceId = payment.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod.ToString(),
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                TenantName = invoice.Lease?.Tenant?.OrganizationName ?? string.Empty,
                UnitNumber = invoice.Lease?.Unit?.UnitNumber ?? string.Empty
            };
        }

        private static PaymentResponseDto MapToDto(Payment p) => new PaymentResponseDto
        {
            Id = p.Id,
            InvoiceId = p.InvoiceId,
            InvoiceNumber = p.Invoice?.InvoiceNumber ?? string.Empty,
            AmountPaid = p.AmountPaid,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod.ToString(),
            ReferenceNumber = p.ReferenceNumber,
            Notes = p.Notes,
            TenantName = p.Invoice?.Lease?.Tenant?.OrganizationName ?? string.Empty,
            UnitNumber = p.Invoice?.Lease?.Unit?.UnitNumber ?? string.Empty
        };
    }
}
