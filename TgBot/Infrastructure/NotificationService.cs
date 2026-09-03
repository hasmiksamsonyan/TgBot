using LinqToDB;
using LinqToDB.Async;
using TgBot.Core.Entities;
using TgBot.Core.Services;
using TgBot.Infrastructure.DataAccess;

namespace TgBot.Infrastructure;

public class NotificationService : INotificationService
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public NotificationService(
        IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> ScheduleNotification(
        Guid userId,
        string type,
        string text,
        DateTime scheduledAt,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var exists = await dbContext.Notifications
            .AnyAsync(
                n => n.UserId == userId &&
                     n.Type == type,
                ct);

        if (exists)
            return false;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            User = new ToDoUser
            {
                UserId = userId
            },
            Type = type,
            Text = text,
            ScheduledAt = scheduledAt,
            IsNotified = false,
            NotifiedAt = null
        };

        var model = ModelMapper.MapToModel(notification);

        await dbContext.InsertAsync(
            model,
            token: ct);

        return true;
    }

    public async Task<IReadOnlyList<Notification>> GetScheduledNotification(
        DateTime scheduledBefore,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.Notifications
            .LoadWith(n => n.User)
            .Where(n =>
                !n.IsNotified &&
                n.ScheduledAt <= scheduledBefore)
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .ToList();
    }

    public async Task MarkNotified(
        Guid notificationId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        await dbContext.Notifications
            .Where(n => n.Id == notificationId)
            .Set(n => n.IsNotified, true)
            .Set(n => n.NotifiedAt, DateTime.UtcNow)
            .UpdateAsync(token: ct);
    }
}