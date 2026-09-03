using TgBot.Core.Entities;

namespace TgBot.Core.Services;

public interface INotificationService
{
   
    Task<bool> ScheduleNotification(
        Guid userId,
        string type,
        string text,
        DateTime scheduledAt,
        CancellationToken ct);

    
    Task<IReadOnlyList<Notification>> GetScheduledNotification(
        DateTime scheduledBefore,
        CancellationToken ct);

    Task MarkNotified(
        Guid notificationId,
        CancellationToken ct);
}