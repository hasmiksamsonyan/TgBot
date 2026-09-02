using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Core.Entities;
using TgBot.Core.Services;
using TgBot.Dto;

namespace TgBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _todoListService;

        public AddTaskScenario(
            IUserService userService,
            IToDoService todoService,
            IToDoListService todoListService)
        {
            _userService = userService;
            _todoService = todoService;
            _todoListService = todoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Message message,
            CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetUser(
                            message.From!.Id,
                            ct);

                        if (user == null)
                        {
                            throw new InvalidOperationException(
                                "Пользователь не найден.");
                        }

                        context.Data["User"] = user;

                        await bot.SendMessage(
                            message.Chat.Id,
                            "Введите название задачи:",
                            cancellationToken: ct);

                        context.CurrentStep = "Name";

                        return ScenarioResult.Transition;
                    }

                case "Name":
                    {
                        if (string.IsNullOrWhiteSpace(message.Text))
                        {
                            await bot.SendMessage(
                                message.Chat.Id,
                                "Название не может быть пустым. Введите название задачи:",
                                cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }

                        context.Data["Name"] = message.Text.Trim();

                        var user = (ToDoUser)context.Data["User"];

                        var lists = await _todoListService.GetUserLists(
                            user.UserId,
                            ct);

                        var buttons = new List<InlineKeyboardButton>();

                        buttons.Add(
                            InlineKeyboardButton.WithCallbackData(
                                "📌 Без списка",
                                new ToDoListCallbackDto
                                {
                                    Action = "addtask",
                                    ToDoListId = null
                                }.ToString()));

                        foreach (var list in lists)
                        {
                            buttons.Add(
                                InlineKeyboardButton.WithCallbackData(
                                    list.Name,
                                    new ToDoListCallbackDto
                                    {
                                        Action = "addtask",
                                        ToDoListId = list.Id
                                    }.ToString()));
                        }

                        await bot.SendMessage(
                            message.Chat.Id,
                            "Выберите список:",
                            replyMarkup: new InlineKeyboardMarkup(buttons),
                            cancellationToken: ct);

                        context.CurrentStep = "List";

                        return ScenarioResult.Transition;
                    }

                case "Deadline":
                    {
                        if (!DateTime.TryParseExact(
                            message.Text?.Trim(),
                            "dd.MM.yyyy",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime deadline))
                        {
                            await bot.SendMessage(
                                message.Chat.Id,
                                "Неверный формат даты. Введите дату в формате dd.MM.yyyy:",
                                cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }

                        var user = (ToDoUser)context.Data["User"];
                        var name = (string)context.Data["Name"];
                        var list = context.Data.TryGetValue("List", out var storedList)
    ? storedList as ToDoList
    : null;

                        var task = await _todoService.Add(
                            user,
                            name,
                            deadline,
                            list,
                            ct);

                        await bot.SendMessage(
                            message.Chat.Id,
                            $"Задача '{task.Name}' добавлена. ID: `{task.Id}`",
                            cancellationToken: ct);

                        return ScenarioResult.Completed;
                    }

                default:
                    throw new InvalidOperationException(
                        $"Неизвестный шаг сценария: {context.CurrentStep}");
            }
        }

        public async Task<ScenarioResult> HandleCallbackQueryAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            if (context.CurrentStep != "List")
            {
                return ScenarioResult.Transition;
            }

            var dto = ToDoListCallbackDto.FromString(
                callbackQuery.Data ?? "");

            if (dto.Action != "addtask")
            {
                return ScenarioResult.Transition;
            }

            if (dto.ToDoListId == null)
            {
                context.Data.Remove("List");
            }
            else
            {
                var list = await _todoListService.Get(
                    dto.ToDoListId.Value,
                    ct);

                if (list == null)
                {
                    await bot.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Список не найден. Попробуйте ещё раз.",
                        cancellationToken: ct);

                    return ScenarioResult.Transition;
                }

                context.Data["List"] = list;
            }

            await bot.SendMessage(
                callbackQuery.Message!.Chat.Id,
                "Введите дату выполнения в формате dd.MM.yyyy:",
                cancellationToken: ct);

            context.CurrentStep = "Deadline";

            return ScenarioResult.Transition;
        }
    }
}