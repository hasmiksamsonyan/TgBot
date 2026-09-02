using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Core.Entities;
using TgBot.Core.Services;
using TgBot.Scenarios;
using TgBot.Dto;
using System.Linq;
using TgBot.Helpers;

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

        private static int _pageSize = 5;

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
                var listDto = PagedListCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                await ShowTasksByList(
                    botClient,
                    callbackQuery.Message!.Chat,
                    user,
                    listDto.ToDoListId,
                    listDto.Page,
                    callbackQuery.Message.MessageId,
                    ct);
            }
            else if (dto.Action == "show_completed")
            {
                var listDto = PagedListCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                await ShowCompletedTasks(
                    botClient,
                    callbackQuery.Message!.Chat,
                    user,
                    listDto.Page,
                    callbackQuery.Message.MessageId,
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
            else if (dto.Action == "deletetask")
            {
                var scenarioContext =
                    new ScenarioContext(ScenarioType.DeleteTask);

                await _contextRepository.SetContext(
                    callbackQuery.From.Id,
                    scenarioContext,
                    ct);

                var scenario = GetScenario(
                    ScenarioType.DeleteTask);

                var result = await scenario.HandleCallbackQueryAsync(
                    botClient,
                    scenarioContext,
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
                        scenarioContext,
                        ct);
                }
            }
            else if (dto.Action == "showtask")
            {
                var itemDto = ToDoItemCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                var task = await _todoService.Get(
                    itemDto.ToDoItemId,
                    ct);

                if (task == null)
                {
                    await botClient.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Задача не найдена.",
                        cancellationToken: ct);

                    return;
                }

                var text =
                    $"Задача: {task.Name}\n" +
                    $"Срок: {task.Deadline:dd.MM.yyyy}\n" +
                    $"Статус: {task.State}";

                var buttons = new List<InlineKeyboardButton>();

                if (task.State == ToDoItemState.Active)
                {
                    buttons.Add(
                        InlineKeyboardButton.WithCallbackData(
                            "✅Выполнить",
                            new ToDoItemCallbackDto
                            {
                                Action = "completetask",
                                ToDoItemId = task.Id
                            }.ToString()));
                }

                buttons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        "❌Удалить",
                        new ToDoItemCallbackDto
                        {
                            Action = "deletetask",
                            ToDoItemId = task.Id
                        }.ToString()));

                var keyboard = new InlineKeyboardMarkup(
                    new[] { buttons.ToArray() });

                await botClient.SendMessage(
                    callbackQuery.Message!.Chat.Id,
                    text,
                    replyMarkup: keyboard,
                    cancellationToken: ct);
            }
            else if (dto.Action == "completetask")
            {
                var itemDto = ToDoItemCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                await _todoService.MarkCompleted(
                    itemDto.ToDoItemId,
                    ct);

                await botClient.EditMessageText(
                    callbackQuery.Message!.Chat.Id,
                    callbackQuery.Message.MessageId,
                    "Задача выполнена.",
                    cancellationToken: ct);
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
                    new PagedListCallbackDto
                    {
                        Action = "show",
                        ToDoListId = null,
                        Page = 0
                    }.ToString()));

            foreach (var list in lists)
            {
                buttons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        list.Name,
                        new PagedListCallbackDto
                        {
                            Action = "show",
                            ToDoListId = list.Id,
                            Page = 0
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

            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    "☑️ Посмотреть выполненные",
                    "show_completed"));

            await bot.SendMessage(
                chat.Id,
                "Выберите список",
                replyMarkup: new InlineKeyboardMarkup(buttons),
                cancellationToken: ct);
        }

        private async Task ShowTasksByList(
            ITelegramBotClient botClient,
            Chat chat,
            ToDoUser user,
            Guid? listId,
            int page,
            int messageId,
            CancellationToken ct)
        {
            var tasks = await _todoService.GetByUserIdAndList(
                user.UserId,
                listId,
                ct);

            var activeTasks = tasks
                .Where(task => task.State == ToDoItemState.Active)
                .ToList();

            if (activeTasks.Count == 0)
            {
                await botClient.EditMessageText(
                    chat.Id,
                    messageId,
                    "Активных задач в этом списке нет.",
                    cancellationToken: ct);

                return;
            }

            var callbackData = activeTasks
                .Select(task =>
                    new KeyValuePair<string, string>(
                        task.Name,
                        new ToDoItemCallbackDto
                        {
                            Action = "showtask",
                            ToDoItemId = task.Id
                        }.ToString()))
                .ToList();

            var listDto = new PagedListCallbackDto
            {
                Action = "show",
                ToDoListId = listId,
                Page = page
            };

            var keyboard = BuildPagedButtons(
                callbackData,
                listDto);

            await botClient.EditMessageText(
                chat.Id,
                messageId,
                "Задачи:",
                replyMarkup: keyboard,
                cancellationToken: ct);
        }

        private async Task ShowCompletedTasks(
            ITelegramBotClient botClient,
            Chat chat,
            ToDoUser user,
            int page,
            int messageId,
            CancellationToken ct)
        {
            var tasks = await _todoService.GetAllByUserId(
                user.UserId,
                ct);

            var completedTasks = tasks
                .Where(task => task.State == ToDoItemState.Completed)
                .ToList();

            if (completedTasks.Count == 0)
            {
                await botClient.EditMessageText(
                    chat.Id,
                    messageId,
                    "Выполненных задач нет.",
                    cancellationToken: ct);

                return;
            }

            var callbackData = completedTasks
                .Select(task =>
                    new KeyValuePair<string, string>(
                        task.Name,
                        new ToDoItemCallbackDto
                        {
                            Action = "showtask",
                            ToDoItemId = task.Id
                        }.ToString()))
                .ToList();

            var listDto = new PagedListCallbackDto
            {
                Action = "show_completed",
                ToDoListId = null,
                Page = page
            };

            var keyboard = BuildPagedButtons(
                callbackData,
                listDto);

            await botClient.EditMessageText(
                chat.Id,
                messageId,
                "Выполненные задачи:",
                replyMarkup: keyboard,
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

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken ct)
        {
            Console.WriteLine(
                $"HandleError: {exception}");

            return Task.CompletedTask;
        }

        private static InlineKeyboardMarkup BuildPagedButtons(
            IReadOnlyList<KeyValuePair<string, string>> callbackData,
            PagedListCallbackDto listDto)
        {
            var buttons = callbackData
                .GetBatchByNumber(
                    _pageSize,
                    listDto.Page)
                .Select(item =>
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            item.Key,
                            item.Value)
                    })
                .ToList();

            var totalPages =
                (int)Math.Ceiling(
                    (double)callbackData.Count / _pageSize);

            var navigationButtons =
                new List<InlineKeyboardButton>();

            if (listDto.Page > 0)
            {
                navigationButtons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        "⬅️",
                        new PagedListCallbackDto
                        {
                            Action = listDto.Action,
                            ToDoListId = listDto.ToDoListId,
                            Page = listDto.Page - 1
                        }.ToString()));
            }

            if (listDto.Page < totalPages - 1)
            {
                navigationButtons.Add(
                    InlineKeyboardButton.WithCallbackData(
                        "➡️",
                        new PagedListCallbackDto
                        {
                            Action = listDto.Action,
                            ToDoListId = listDto.ToDoListId,
                            Page = listDto.Page + 1
                        }.ToString()));
            }

            if (navigationButtons.Count > 0)
            {
                buttons.Add(navigationButtons.ToArray());
            }

            return new InlineKeyboardMarkup(buttons);
        }
    }
}