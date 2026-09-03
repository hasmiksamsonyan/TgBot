using Telegram.Bot;
using TgBot.Core.DataAccess;
using TgBot.Core.Services;

namespace TgBot.BackgroundTasks;

public class DeadlineBackgroundTask : BackgroundTask
{
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IToDoRepository _toDoRepository;

    public DeadlineBackgroundTask(
        INotificationService notificationService,
        IUserRepository userRepository,
        IToDoRepository toDoRepository)
        : base(
            TimeSpan.FromHours(1),
            nameof(DeadlineBackgroundTask))
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
        _toDoRepository = toDoRepository;
    }

    protected override async Task Execute(
        CancellationToken ct)
    {
        var users =
            await _userRepository.GetUsers(ct);

        var now = DateTime.UtcNow;

        foreach (var user in users)
        {
            var tasks =
                await _toDoRepository.GetActiveWithDeadline(
                    user.UserId,
                    DateTime.UtcNow.AddDays(-1).Date,
                    DateTime.UtcNow.Date,
                    ct);

            foreach (var task in tasks)
            {
                await _notificationService.ScheduleNotification(
                    user.UserId,
                    $"Dealine_{task.Id}",
                    $"Ой! Вы пропустили дедлайн по задаче {task.Name}",
                    now,
                    ct);
            }
        }
    }
}