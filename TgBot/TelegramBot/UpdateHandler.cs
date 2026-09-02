using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Core.Entities;
using TgBot.Core.Services;
using TgBot.Scenarios;
using TgBot.Dto;

namespace TgBot
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _todoListService;
        private readonly IToDoReportService _reportService;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _contextRepository;

        public UpdateHandler(
            IUserService userService,
            IToDoService todoService,
            IToDoReportService reportService,
            IToDoListService todoListService,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
            _todoListService = todoListService;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
        }

        public async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken ct)
        {
            try
            {
                if (update.CallbackQuery != null)
                {
                    await OnCallbackQuery(
                        botClient,
                        update.CallbackQuery,
                        ct);

                    return;
                }

                if (update.Message == null ||
                    update.Message.From == null)
                {
                    return;
                }

                await OnMessage(
                    botClient,
                    update.Message,
                    ct);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(
                    botClient,
                    ex,
                    ct);
            }
        }

        private async Task OnMessage(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken ct)
        {
            var tgUser = message.From!;
            var chat = message.Chat;
            var text = message.Text?.Trim() ?? "";

            var user = await _userService.GetUser(
                tgUser.Id,
                ct);

            if (user == null)
            {
                if (text != "/start")
                {
                    await botClient.SendMessage(
                        chat.Id,
                        "Для начала работы нажмите /start",
                        replyMarkup: GetStartKeyboard(),
                        cancellationToken: ct);

                    return;
                }

                user = await _userService.RegisterUser(
                    tgUser.Id,
                    tgUser.Username ?? "User",
                    ct);

                await botClient.SendMessage(
                    chat.Id,
                    $"Добро пожаловать, {user.TelegramUserName}!",
                    replyMarkup: GetMainKeyboard(),
                    cancellationToken: ct);

                return;
            }

            var context = await _contextRepository.GetContext(
                tgUser.Id,
                ct);

            if (text == "/start")
            {
                await _contextRepository.ResetContext(
                    tgUser.Id,
                    ct);

                await botClient.SendMessage(
                    chat.Id,
                    "Выберите действие:",
                    replyMarkup: GetMainKeyboard(),
                    cancellationToken: ct);

                return;
            }

            if (context != null)
            {
                if (text == "/cancel")
                {
                    await _contextRepository.ResetContext(
                        tgUser.Id,
                        ct);

                    await botClient.SendMessage(
                        chat.Id,
                        "Сценарий отменён.",
                        replyMarkup: GetMainKeyboard(),
                        cancellationToken: ct);

                    return;
                }

                await ProcessScenario(
                    botClient,
                    context,
                    message,
                    ct);

                return;
            }

            if (text == "/help")
            {
                await ShowHelp(
                    botClient,
                    chat,
                    ct);
            }
            else if (text == "/info")
            {
                await ShowInfo(
                    botClient,
                    chat,
                    user,
                    ct);
            }
            else if (text == "/report")
            {
                await ShowReport(
                    botClient,
                    chat,
                    user,
                    ct);
            }
            else if (text.StartsWith("/find"))
            {
                await FindTasks(
                    botClient,
                    chat,
                    user,
                    text,
                    ct);
            }
            else if (text == "/addtask")
            {
                var scenarioContext =
                    new ScenarioContext(ScenarioType.AddTask);

                await _contextRepository.SetContext(
                    tgUser.Id,
                    scenarioContext,
                    ct);

                await botClient.SendMessage(
                    chat.Id,
                    "Добавление задачи",
                    replyMarkup: GetCancelKeyboard(),
                    cancellationToken: ct);

                await ProcessScenario(
                    botClient,
                    scenarioContext,
                    message,
                    ct);
            }
            else if (text == "/show")
            {
                await ShowLists(
                    botClient,
                    chat,
                    user,
                    ct);
            }
            else if (text.StartsWith("/completetask "))
            {
                await CompleteTask(
                    botClient,
                    chat,
                    text,
                    ct);
            }
            else if (text.StartsWith("/removetask "))
            {
                await RemoveTask(
                    botClient,
                    chat,
                    user,
                    text,
                    ct);
            }
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

        private async Task OnCallbackQuery(
            ITelegramBotClient botClient,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            var user = await _userService.GetUser(
                callbackQuery.From.Id,
                ct);

            if (user == null)
                return;

            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                cancellationToken: ct);

            var context = await _contextRepository.GetContext(
                callbackQuery.From.Id,
                ct);

            if (context != null)
            {
                var scenario = GetScenario(
                    context.CurrentScenario);

                var result = await scenario.HandleCallbackQueryAsync(
                    botClient,
                    context,
                    callbackQuery,
                    ct);

                if (result == ScenarioResult.Completed)
                {
                    await _contextRepository.ResetContext(
                        callbackQuery.From.Id,
                        ct);
                }
                else
                {
                    await _contextRepository.SetContext(
                        callbackQuery.From.Id,
                        context,
                        ct);
                }

                return;
            }

            var dto = CallbackDto.FromString(
                callbackQuery.Data ?? "");

            if (dto.Action == "show")
            {
                var listDto = ToDoListCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                await ShowTasksByList(
                    botClient,
                    callbackQuery.Message!.Chat,
                    user,
                    listDto.ToDoListId,
                    ct);
            }
            else if (dto.Action == "addlist")
            {
                var scenarioContext =
                    new ScenarioContext(ScenarioType.AddList);

                scenarioContext.Data["User"] = user;

                await _contextRepository.SetContext(
                    callbackQuery.From.Id,
                    scenarioContext,
                    ct);

                var scenario = GetScenario(
                    ScenarioType.AddList);

                var result = await scenario.HandleMessageAsync(
                    botClient,
                    scenarioContext,
                    callbackQuery.Message!,
                    ct);

                if (result == ScenarioResult.Completed)
                {
                    await _contextRepository.ResetContext(
                        callbackQuery.From.Id,
                        ct);
                }
                else
                {
                    await _contextRepository.SetContext(
                        callbackQuery.From.Id,
                        scenarioContext,
                        ct);
                }
            }
            else if (dto.Action == "deletelist")
            {
                var scenarioContext =
                    new ScenarioContext(ScenarioType.DeleteList);

                scenarioContext.Data["User"] = user;

                await _contextRepository.SetContext(
                    callbackQuery.From.Id,
                    scenarioContext,
                    ct);

                var scenario = GetScenario(
                    ScenarioType.DeleteList);

                var result = await scenario.HandleMessageAsync(
                    botClient,
                    scenarioContext,
                    callbackQuery.Message!,
                    ct);

                if (result == ScenarioResult.Completed)
                {
                    await _contextRepository.ResetContext(
                        callbackQuery.From.Id,
                        ct);

                    await botClient.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Готово!",
                        replyMarkup: GetMainKeyboard(),
                        cancellationToken: ct);
                }
                else
                {
                    await _contextRepository.SetContext(
                        callbackQuery.From.Id,
                        scenarioContext,
                        ct);
                }
            }
        }

        public IScenario GetScenario(
            ScenarioType scenario)
        {
            var result = _scenarios.FirstOrDefault(
                s => s.CanHandle(scenario));

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Сценарий {scenario} не найден.");
            }

            return result;
        }

        public async Task ProcessScenario(
            ITelegramBotClient botClient,
            ScenarioContext context,
            Message msg,
            CancellationToken ct)
        {
            var scenario = GetScenario(
                context.CurrentScenario);

            var result = await scenario.HandleMessageAsync(
                botClient,
                context,
                msg,
                ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(
                    msg.From!.Id,
                    ct);

                await botClient.SendMessage(
                    msg.Chat.Id,
                    "Готово!",
                    replyMarkup: GetMainKeyboard(),
                    cancellationToken: ct);
            }
            else
            {
                await _contextRepository.SetContext(
                    msg.From!.Id,
                    context,
                    ct);
            }
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

        private ReplyKeyboardMarkup GetCancelKeyboard()
        {
            return new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton("/cancel")
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
                        new KeyboardButton("/addtask"),
                        new KeyboardButton("/show")
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

        private async Task ShowLists(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            CancellationToken ct)
        {
            var lists = await _todoListService.GetUserLists(
                user.UserId,
                ct);

            var buttons = new List<InlineKeyboardButton>();

            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    "📌 Без списка",
                    new ToDoListCallbackDto
                    {
                        Action = "show",
                        ToDoListId = null
                    }.ToString()));

            foreach (var list in lists)
            {
                buttons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        list.Name,
                        new ToDoListCallbackDto
                        {
                            Action = "show",
                            ToDoListId = list.Id
                        }.ToString()));
            }

            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    "🆕 Добавить",
                    "addlist"));

            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    "❌ Удалить",
                    "deletelist"));

            await bot.SendMessage(
                chat.Id,
                "Выберите список",
                replyMarkup: new InlineKeyboardMarkup(buttons),
                cancellationToken: ct);
        }

        private async Task ShowTasksByList(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            Guid? listId,
            CancellationToken ct)
        {
            var tasks = await _todoService.GetByUserIdAndList(
                user.UserId,
                listId,
                ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(
                    chat.Id,
                    "Задач в этом списке нет.",
                    cancellationToken: ct);

                return;
            }

            string msg = "Задачи:\n";

            for (int i = 0; i < tasks.Count; i++)
            {
                string state =
                    tasks[i].State == ToDoItemState.Active
                        ? "Активная"
                        : "Выполнена";

                msg +=
                    $"{i + 1}. ({state}) {tasks[i].Name} - " +
                    $"{tasks[i].Deadline:dd.MM.yyyy} - " +
                    $"`{tasks[i].Id}`\n";
            }

            await bot.SendMessage(
                chat.Id,
                msg,
                cancellationToken: ct);
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
                "/addtask - добавить задачу\n" +
                "/show - показать списки задач\n" +
                "/cancel - отменить текущий сценарий\n" +
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
            var all = await _todoService.GetAllByUserId(
                user.UserId,
                ct);

            var active = await _todoService.GetActiveByUserId(
                user.UserId,
                ct);

            await bot.SendMessage(
                chat.Id,
                $"Пользователь: {user.TelegramUserName}\n" +
                $"Всего задач: {all.Count}\n" +
                $"Активных: {active.Count}\n" +
                $"Выполненных: {all.Count - active.Count}",
                cancellationToken: ct);
        }

        private async Task ShowReport(
            ITelegramBotClient bot,
            Chat chat,
            ToDoUser user,
            CancellationToken ct)
        {
            var stats = await _reportService.GetUserStats(
                user.UserId,
                ct);

            await bot.SendMessage(
                chat.Id,
                $"Статистика по задачам на " +
                $"{stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
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
            string prefix = command
                .Substring("/find".Length)
                .Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await bot.SendMessage(
                    chat.Id,
                    "Укажите префикс для поиска. Пример: /find Куп",
                    cancellationToken: ct);

                return;
            }

            var tasks = await _todoService.Find(
                user,
                prefix,
                ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(
                    chat.Id,
                    $"Задачи, начинающиеся на '{prefix}', не найдены.",
                    cancellationToken: ct);

                return;
            }

            string msg =
                $"Найдено задач, начинающихся на '{prefix}':\n";

            for (int i = 0; i < tasks.Count; i++)
            {
                string state =
                    tasks[i].State == ToDoItemState.Active
                        ? "Активная"
                        : "Выполнена";

                msg +=
                    $"{i + 1}. ({state}) {tasks[i].Name} - " +
                    $"{tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - " +
                    $"`{tasks[i].Id}`\n";
            }

            await bot.SendMessage(
                chat.Id,
                msg,
                cancellationToken: ct);
        }

        private async Task CompleteTask(
            ITelegramBotClient bot,
            Chat chat,
            string command,
            CancellationToken ct)
        {
            string idStr = command
                .Substring("/completetask".Length)
                .Trim();

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
                await _todoService.MarkCompleted(
                    id,
                    ct);

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
            string numStr = command
                .Substring("/removetask".Length)
                .Trim();

            if (!int.TryParse(numStr, out int number) ||
                number < 1)
            {
                await bot.SendMessage(
                    chat.Id,
                    "Укажите корректный номер задачи",
                    cancellationToken: ct);

                return;
            }

            try
            {
                var tasks = await _todoService.GetAllByUserId(
                    user.UserId,
                    ct);

                if (number > tasks.Count)
                {
                    await bot.SendMessage(
                        chat.Id,
                        $"Номер от 1 до {tasks.Count}",
                        cancellationToken: ct);

                    return;
                }

                var task = tasks[number - 1];

                await _todoService.Delete(
                    task.Id,
                    ct);

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

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken ct)
        {
            Console.WriteLine(
                $"HandleError: {exception}");

            return Task.CompletedTask;
        }
    }
}