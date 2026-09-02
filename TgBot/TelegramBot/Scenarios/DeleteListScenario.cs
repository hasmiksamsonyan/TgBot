using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Core.Entities;
using TgBot.Core.Services;
using TgBot.Dto;

namespace TgBot.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _todoListService;
        private readonly IToDoService _todoService;

        public DeleteListScenario(
            IUserService userService,
            IToDoListService todoListService,
            IToDoService todoService)
        {
            _userService = userService;
            _todoListService = todoListService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
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
                        ToDoUser user;

                        if (context.Data.TryGetValue("User", out var storedUser))
                        {
                            user = (ToDoUser)storedUser;
                        }
                        else
                        {
                            var foundUser = await _userService.GetUser(
                                message.From!.Id,
                                ct);

                            if (foundUser == null)
                                throw new InvalidOperationException(
                                    "Пользователь не найден.");

                            user = foundUser;
                            context.Data["User"] = user;
                        }

                        var lists = await _todoListService.GetUserLists(
                            user.UserId,
                            ct);

                        if (lists.Count == 0)
                        {
                            await bot.SendMessage(
                                message.Chat.Id,
                                "Списков для удаления нет.",
                                cancellationToken: ct);

                            return ScenarioResult.Completed;
                        }

                        var buttons = new List<InlineKeyboardButton>();

                        foreach (var list in lists)
                        {
                            buttons.Add(
                                InlineKeyboardButton.WithCallbackData(
                                    list.Name,
                                    new ToDoListCallbackDto
                                    {
                                        Action = "deletelist",
                                        ToDoListId = list.Id
                                    }.ToString()));
                        }

                        await bot.SendMessage(
                        message.Chat.Id,
                        "Сценарий удаления списка. Для отмены нажмите /cancel.",
                        replyMarkup: GetCancelKeyboard(),
                        cancellationToken: ct);

                        await bot.SendMessage(
                            message.Chat.Id,
                            "Выберите список для удаления:",
                            replyMarkup: new InlineKeyboardMarkup(buttons),
                            cancellationToken: ct);

                        context.CurrentStep = "Approve";

                        return ScenarioResult.Transition;
                    }

                case "Approve":
                    {
                        throw new InvalidOperationException(
                            "Выбор списка для удаления должен обрабатываться через CallbackQuery.");
                    }

                case "Delete":
                    {
                        throw new InvalidOperationException(
                            "Подтверждение удаления должно обрабатываться через CallbackQuery.");
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
            if (context.CurrentStep == "Approve")
            {
                var dto = ToDoListCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                if (dto.Action != "deletelist" ||
                    dto.ToDoListId == null)
                {
                    return ScenarioResult.Transition;
                }

                var list = await _todoListService.Get(
                    dto.ToDoListId.Value,
                    ct);

                if (list == null)
                    throw new InvalidOperationException(
                        "Список не найден.");

                context.Data["List"] = list;

                var buttons = new InlineKeyboardMarkup(
                    new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "✅ Да",
                                "yes"),

                            InlineKeyboardButton.WithCallbackData(
                                "❌ Нет",
                                "no")
                        }
                    });

                await bot.SendMessage(
                    callbackQuery.Message!.Chat.Id,
                    $"Подтверждаете удаление списка «{list.Name}» и всех его задач?",
                    replyMarkup: buttons,
                    cancellationToken: ct);

                context.CurrentStep = "Delete";

                return ScenarioResult.Transition;
            }

            if (context.CurrentStep == "Delete")
            {
                if (callbackQuery.Data == "no")
                {
                    var list = (ToDoList)context.Data["List"];

                    await bot.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        $"Удаление списка «{list.Name}» отменено.",
                        replyMarkup: GetMainKeyboard(),
                        cancellationToken: ct);

                    return ScenarioResult.Completed;
                }

                if (callbackQuery.Data == "yes")
                {
                    var user = (ToDoUser)context.Data["User"];
                    var list = (ToDoList)context.Data["List"];

                    var tasks =
                        await _todoService.GetByUserIdAndList(
                            user.UserId,
                            list.Id,
                            ct);

                    foreach (var task in tasks)
                    {
                        await _todoService.Delete(
                            task.Id,
                            ct);
                    }

                    await _todoListService.Delete(
                        list.Id,
                        ct);

                    await bot.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        $"Список «{list.Name}» и все его задачи удалены.",
                        replyMarkup: GetMainKeyboard(),
                        cancellationToken: ct);

                    return ScenarioResult.Completed;
                }

                return ScenarioResult.Transition;
            }

            return ScenarioResult.Transition;
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
    }
}