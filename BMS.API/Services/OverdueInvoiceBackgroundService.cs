using BMS.Application.Enums;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Services
{
    public class OverdueInvoiceBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueInvoiceBackgroundService> _logger;

        public OverdueInvoiceBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OverdueInvoiceBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotifications(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Daily invoice and expiry notification processing failed.");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ProcessNotifications(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var now = DateTime.UtcNow;
            var today = now.Date;

            var overdueInvoices = await context.Invoices
                .Include(i => i.Payments)
                .Include(i => i.Lease).ThenInclude(l => l.Unit)
                .Where(i => (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.Overdue)
                    && i.DueDate < today)
                .ToListAsync(cancellationToken);

            var statusChanged = false;
            foreach (var invoice in overdueInvoices)
            {
                if (invoice.Status == InvoiceStatus.Issued)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                    invoice.UpdatedAt = now;
                    statusChanged = true;
                }
            }
            if (statusChanged)
                await context.SaveChangesAsync(cancellationToken);

            var users = await userManager.Users
                .Where(user => user.IsActive)
                .ToListAsync(cancellationToken);
            if (users.Count == 0)
                return;

            var notifications = new List<Notification>();
            foreach (var invoice in overdueInvoices.Where(i => i.AmountDue > i.Payments.Sum(p => p.AmountPaid)))
            {
                var title = "Invoice overdue";
                var message = $"Invoice {invoice.InvoiceNumber} for unit {invoice.Lease.Unit.UnitNumber} is overdue.";
                await AddForUsersIfNew(context, users, notifications, NotificationType.PaymentOverdue, title, message, cancellationToken);
            }

            var leaseLimit = today.AddDays(30);
            var expiringLeases = await context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= leaseLimit)
                .ToListAsync(cancellationToken);
            foreach (var lease in expiringLeases)
            {
                var title = "Lease expiring soon";
                var message = $"Lease for {lease.Tenant.OrganizationName} in unit {lease.Unit.UnitNumber} expires on {lease.EndDate:yyyy-MM-dd}.";
                await AddForUsersIfNew(context, users, notifications, NotificationType.LeaseExpiry, title, message, cancellationToken);
            }

            var expiringDocuments = await context.LegalDocuments
                .Include(document => document.Tenant)
                .Where(document => document.ExpiryDate.HasValue
                    && document.ExpiryDate.Value >= today
                    && document.ExpiryDate.Value <= leaseLimit)
                .ToListAsync(cancellationToken);
            foreach (var document in expiringDocuments)
            {
                var title = "Document expiring soon";
                var message = $"{document.DocumentType} for {document.Tenant.OrganizationName} expires on {document.ExpiryDate:yyyy-MM-dd}.";
                await AddForUsersIfNew(context, users, notifications, NotificationType.DocumentExpiry, title, message, cancellationToken);
            }

            if (notifications.Count > 0)
            {
                context.Notifications.AddRange(notifications);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task AddForUsersIfNew(
            AppDbContext context,
            IEnumerable<AppUser> users,
            ICollection<Notification> notifications,
            NotificationType type,
            string title,
            string message,
            CancellationToken cancellationToken)
        {
            foreach (var user in users)
            {
                var exists = await context.Notifications.AnyAsync(
                    notification => notification.UserId == user.Id
                        && notification.NotificationType == type
                        && notification.Message == message,
                    cancellationToken);
                if (!exists)
                {
                    notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Title = title,
                        Message = message,
                        NotificationType = type,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
