using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotifications(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType.ToString(),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public Task<int> GetUnreadCount(string userId) =>
            _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<bool> MarkAsRead(int id, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task MarkAllAsRead(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var notification in unread)
                notification.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }
}
