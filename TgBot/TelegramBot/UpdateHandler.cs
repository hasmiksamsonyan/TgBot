using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Core.Entities;
using TgBot.Core.Services;

namespace TgBot
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;

        public UpdateHandler(
            IUserService userService,
            IToDoService todoService,
            IToDoReportService reportService)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
        }

        public async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken ct)
        {
            try
            {
                if (update.Message == null || update.Message.From == null)
                    return;

                var tgUser = update.Message.From;
                var chat = update.Message.Chat;
                var text = update.Message.Text?.Trim() ?? "";

                var user = await _userService.GetUser(tgUser.Id, ct);

                // Пользователь ещё не зарегистрирован
                if (user == null)
                {
                    // До регистрации доступна только кнопка /start
                    if (text != "/start")
                    {
                        await botClient.SendMessage(
                            chat.Id,
                            "Для начала работы нажмите /start",
                            replyMarkup: GetStartKeyboard(),
                            cancellationToken: ct);

                        return;
                    }

                    // Регистрация по команде /start
                    user = await _userService.RegisterUser(
                        tgUser.Id,
                        tgUser.Username ?? "User",
                        ct);

                    // После регистрации показываем основную клавиатуру
                    await botClient.SendMessage(
                        chat.Id,
                        $"Добро пожаловать, {user.TelegramUserName}!",
                        replyMarkup: GetMainKeyboard(),
                        cancellationToken: ct);

                    return;
                }

                if (text == "/help")
                    await ShowHelp(botClient, chat, ct);
                else if (text == "/info")
                    await ShowInfo(botClient, chat, user, ct);
                else if (text == "/showtasks")
                    await ShowActiveTasks(botClient, chat, user, ct);
                else if (text == "/showalltasks")
                    await ShowAllTasks(botClient, chat, user, ct);
                else if (text == "/report")
                    await ShowReport(botClient, chat, user, ct);
                else if (text.StartsWith("/find"))
                    await FindTasks(botClient, chat, user, text, ct);
                else if (text.StartsWith("/addtask"))
                    await AddTask(botClient, chat, user, text, ct);
                else if (text.StartsWith("/completetask "))
                    await CompleteTask(botClient, chat, text, ct);
                else if (text.StartsWith("/removetask "))
                    await RemoveTask(botClient, chat, user, text, ct);
                else if (text == "/exit")
                {
                    await botClient.SendMessage(
                        chat.Id,
                        "До свидания!",
                        cancellationToken: ct);

                    Environment.Exit(0);
                }
                else
                {
                    await botClient.SendMessage(
                        chat.Id,
                        "Неизвестная команда. Используйте /help",
                        cancellationToken: ct);
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, ct);
            }
        }

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception}");
            return Task.CompletedTask;
        }

        private ReplyKeyboardMarkup GetStartKeyboard()
        {
            return new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton("/start")
                })
            {
                ResizeKeyboard = true
            };
        }

        private ReplyKeyboardMarkup GetMainKeyboard()
        {
            return new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("/showtasks"),
                        new KeyboardButton("/showalltasks")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("/report")
                    }
                })
            {
                ResizeKeyboard = true
            };
        }

        private async Task ShowHelp(
            ITelegramBotClient bot,
            Chat chat,
            CancellationToken ct)
        {
            await bot.SendMessage(
                chat.Id,
                "/help - справка\n" +
                "/info - информация\n" +
                "/report - статистика по задачам\n" +
                "/find [текст] - найти задачи по префиксу\n" +
                "/addtask [название] - добавить задачу\n" +
                "/showtasks - активные задачи\n" +
                "/showalltasks - все задачи\n" +
                "/completetask [id] - выполнить задачу\n" +
                "/removetask [номер] - удалить задачу\n" +
                "/exit - выход",
                cancellationToken: ct);
        }

        private async Task ShowInfo(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            CancellationToken ct)
        {
            var all = await _todoService.GetAllByUserId(user.UserId, ct);
            var active = await _todoService.GetActiveByUserId(user.UserId, ct);

            await bot.SendMessage(
                chat.Id,
                $"Пользователь: {user.TelegramUserName}\n" +
                $"Всего задач: {all.Count}\n" +
                $"Активных: {active.Count}\n" +
                $"Выполненных: {all.Count - active.Count}",
                cancellationToken: ct);
        }

        private async Task ShowActiveTasks(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            CancellationToken ct)
        {
            var tasks = await _todoService.GetActiveByUserId(user.UserId, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(
                    chat.Id,
                    "Активных задач нет",
                    cancellationToken: ct);

                return;
            }

            string msg = "Активные задачи:\n";

            for (int i = 0; i < tasks.Count; i++)
            {
                msg += $"{i + 1}. {tasks[i].Name} - " +
                       $"{tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - " +
                       $"`{tasks[i].Id}`\n";
            }

            await bot.SendMessage(
                chat.Id,
                msg,
                cancellationToken: ct);
        }

        private async Task ShowAllTasks(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            CancellationToken ct)
        {
            var tasks = await _todoService.GetAllByUserId(user.UserId, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(
                    chat.Id,
                    "Задач нет",
                    cancellationToken: ct);

                return;
            }

            string msg = "Все задачи:\n";

            for (int i = 0; i < tasks.Count; i++)
            {
                string state = tasks[i].State == ToDoItemState.Active
                    ? "Активная"
                    : "Выполнена";

                msg += $"{i + 1}. ({state}) {tasks[i].Name} - " +
                       $"{tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - " +
                       $"`{tasks[i].Id}`\n";
            }

            await bot.SendMessage(
                chat.Id,
                msg,
                cancellationToken: ct);
        }

        private async Task ShowReport(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            CancellationToken ct)
        {
            var stats = await _reportService.GetUserStats(user.UserId, ct);

            await bot.SendMessage(
                chat.Id,
                $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
                $"Всего: {stats.total}; " +
                $"Завершенных: {stats.completed}; " +
                $"Активных: {stats.active};",
                cancellationToken: ct);
        }

        private async Task FindTasks(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            string command,
            CancellationToken ct)
        {
            string prefix = command.Substring("/find".Length).Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await bot.SendMessage(
                    chat.Id,
                    "Укажите префикс для поиска. Пример: /find Куп",
                    cancellationToken: ct);

                return;
            }

            var tasks = await _todoService.Find(user, prefix, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(
                    chat.Id,
                    $"Задачи, начинающиеся на '{prefix}', не найдены.",
                    cancellationToken: ct);

                return;
            }

            string msg = $"Найдено задач, начинающихся на '{prefix}':\n";

            for (int i = 0; i < tasks.Count; i++)
            {
                string state = tasks[i].State == ToDoItemState.Active
                    ? "Активная"
                    : "Выполнена";

                msg += $"{i + 1}. ({state}) {tasks[i].Name} - " +
                       $"{tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - " +
                       $"`{tasks[i].Id}`\n";
            }

            await bot.SendMessage(
                chat.Id,
                msg,
                cancellationToken: ct);
        }

        private async Task AddTask(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            string command,
            CancellationToken ct)
        {
            string name = command.Substring("/addtask".Length).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await bot.SendMessage(
                    chat.Id,
                    "Укажите название: /addtask Купить продукты",
                    cancellationToken: ct);

                return;
            }

            try
            {
                var task = await _todoService.Add(user, name, ct);

                await bot.SendMessage(
                    chat.Id,
                    $"Задача '{name}' добавлена. ID: `{task.Id}`",
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    chat.Id,
                    $"Ошибка: {ex.Message}",
                    cancellationToken: ct);
            }
        }

        private async Task CompleteTask(
            ITelegramBotClient bot,
            Chat chat,
            string command,
            CancellationToken ct)
        {
            string idStr = command.Substring("/completetask".Length).Trim();

            if (!Guid.TryParse(idStr, out Guid id))
            {
                await bot.SendMessage(
                    chat.Id,
                    "Укажите корректный ID задачи",
                    cancellationToken: ct);

                return;
            }

            try
            {
                await _todoService.MarkCompleted(id, ct);

                await bot.SendMessage(
                    chat.Id,
                    $"Задача `{id}` выполнена!",
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    chat.Id,
                    $"Ошибка: {ex.Message}",
                    cancellationToken: ct);
            }
        }

        private async Task RemoveTask(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            string command,
            CancellationToken ct)
        {
            string numStr = command.Substring("/removetask".Length).Trim();

            if (!int.TryParse(numStr, out int number) || number < 1)
            {
                await bot.SendMessage(
                    chat.Id,
                    "Укажите корректный номер задачи",
                    cancellationToken: ct);

                return;
            }

            try
            {
                var tasks = await _todoService.GetAllByUserId(user.UserId, ct);

                if (number > tasks.Count)
                {
                    await bot.SendMessage(
                        chat.Id,
                        $"Номер от 1 до {tasks.Count}",
                        cancellationToken: ct);

                    return;
                }

                var task = tasks[number - 1];

                await _todoService.Delete(task.Id, ct);

                await bot.SendMessage(
                    chat.Id,
                    $"Задача '{task.Name}' удалена",
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    chat.Id,
                    $"Ошибка: {ex.Message}",
                    cancellationToken: ct);
            }
        }
    }
}