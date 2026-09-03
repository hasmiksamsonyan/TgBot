using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot.Core.Entities;
using TgBot.Core.Services;

namespace TgBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public AddTaskScenario(
            IUserService userService,
            IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
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
            ArgumentNullException.ThrowIfNull(bot);
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(message);

            ct.ThrowIfCancellationRequested();

            if (message.From == null)
            {
                throw new InvalidOperationException(
                    "Не удалось определить пользователя Telegram.");
            }

            switch (context.CurrentStep)
            {
                case null:
                    {
                        var user = await _userService.GetUser(
                            message.From.Id,
                            ct);

                        if (user == null)
                            throw new InvalidOperationException(
                                "Пользователь не найден.");

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

                        await bot.SendMessage(
                            message.Chat.Id,
                            "Введите дату выполнения в формате dd.MM.yyyy:",
                            cancellationToken: ct);

                        context.CurrentStep = "Deadline";

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

                        var task = await _todoService.Add(
                            user,
                            name,
                            deadline,
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
    }
}