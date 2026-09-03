using Telegram.Bot;
using TgBot.Core.Services;

namespace TgBot.BackgroundTasks;

public class NotificationBackgroundTask : BackgroundTask
{
    private readonly INotificationService _notificationService;
    private readonly ITelegramBotClient _bot;

    public NotificationBackgroundTask(
        INotificationService notificationService,
        ITelegramBotClient bot)
        : base(
            TimeSpan.FromMinutes(1),
            nameof(NotificationBackgroundTask))
    {
        _notificationService = notificationService;
        _bot = bot;
    }

    protected override async Task Execute(
        CancellationToken ct)
    {
        var notifications =
            await _notificationService.GetScheduledNotification(
                DateTime.UtcNow,
                ct);

        foreach (var notification in notifications)
        {
            await _bot.SendMessage(
                notification.User.TelegramUserId,
                notification.Text,
                cancellationToken: ct);

            await _notificationService.MarkNotified(
                notification.Id,
                ct);
        }
    }
}