using Telegram.Bot;
using TgBot.Core.DataAccess;
using TgBot.Core.Services;

namespace TgBot.BackgroundTasks;

public class TodayBackgroundTask : BackgroundTask
{
	private readonly INotificationService _notificationService;
	private readonly IUserRepository _userRepository;
	private readonly IToDoRepository _toDoRepository;

	public TodayBackgroundTask(
		INotificationService notificationService,
		IUserRepository userRepository,
		IToDoRepository toDoRepository)
		: base(
			TimeSpan.FromDays(1),
			nameof(TodayBackgroundTask))
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

		var today = DateTime.UtcNow.Date;
		var tomorrow = today.AddDays(1).AddTicks(-1);

		foreach (var user in users)
		{
			var tasks =
				await _toDoRepository.GetActiveWithDeadline(
					user.UserId,
					today,
					tomorrow,
					ct);

			var text = tasks.Count == 0
				? "На сегодня задач нет."
				: "Задачи на сегодня:\n" +
				  string.Join(
					  "\n",
					  tasks.Select(task => $"• {task.Name}"));

			await _notificationService.ScheduleNotification(
				user.UserId,
				$"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}",
				text,
				DateTime.UtcNow,
				ct);
		}
	}
}