using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetNotifications(string userId);
        Task<int> GetUnreadCount(string userId);
        Task<bool> MarkAsRead(int id, string userId);
        Task MarkAllAsRead(string userId);
    }
}
