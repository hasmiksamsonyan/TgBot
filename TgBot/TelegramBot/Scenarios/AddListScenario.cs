using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Core.Entities;
using TgBot.Core.Services;

namespace TgBot.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _todoListService;

        public AddListScenario(
            IUserService userService,
            IToDoListService todoListService)
        {
            _userService = userService;
            _todoListService = todoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddList;
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

                        await bot.SendMessage(
                            message.Chat.Id,
                            "Введите название списка:",
                            replyMarkup: GetCancelKeyboard(),
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
                                "Название списка не может быть пустым. Введите название списка:",
                                replyMarkup: GetCancelKeyboard(),
                                cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }

                        var user = (ToDoUser)context.Data["User"];
                        var name = message.Text.Trim();

                        try
                        {
                            await _todoListService.Add(
                                user,
                                name,
                                ct);

                            await bot.SendMessage(
                                message.Chat.Id,
                                $"Список «{name}» добавлен.",
                                replyMarkup: GetMainKeyboard(),
                                cancellationToken: ct);

                            return ScenarioResult.Completed;
                        }
                        catch (Exception ex)
                        {
                            await bot.SendMessage(
                                message.Chat.Id,
                                $"Ошибка: {ex.Message}\nВведите другое название списка:",
                                replyMarkup: GetCancelKeyboard(),
                                cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }
                    }

                default:
                    throw new InvalidOperationException(
                        $"Неизвестный шаг сценария: {context.CurrentStep}");
            }
        }

        public Task<ScenarioResult> HandleCallbackQueryAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            return Task.FromResult(ScenarioResult.Transition);
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
    }
}