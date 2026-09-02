using TgBot.Core.Services;
using TgBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgBot.Scenarios
{
    public class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _todoService;

        public DeleteTaskScenario(IToDoService todoService)
        {
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Message message,
            CancellationToken ct)
        {
            return ScenarioResult.Transition;
        }

        public async Task<ScenarioResult> HandleCallbackQueryAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            if (context.CurrentStep == null)
            {
                var dto = ToDoItemCallbackDto.FromString(
                    callbackQuery.Data ?? "");

                var task = await _todoService.Get(
                    dto.ToDoItemId,
                    ct);

                if (task == null)
                {
                    await bot.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Задача не найдена.",
                        cancellationToken: ct);

                    return ScenarioResult.Completed;
                }

                context.Data["Task"] = task;
                context.CurrentStep = "Approve";

                var keyboard = new InlineKeyboardMarkup(
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
                    $"Удалить задачу «{task.Name}»?",
                    replyMarkup: keyboard,
                    cancellationToken: ct);

                return ScenarioResult.Transition;
            }

            if (context.CurrentStep == "Approve")
            {
                if (callbackQuery.Data == "no")
                {
                    await bot.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Удаление отменено.",
                        cancellationToken: ct);

                    return ScenarioResult.Completed;
                }

                if (callbackQuery.Data == "yes")
                {
                    var task = (TgBot.Core.Entities.ToDoItem)
                        context.Data["Task"];

                    await _todoService.Delete(
                        task.Id,
                        ct);

                    await bot.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Задача удалена.",
                        cancellationToken: ct);

                    return ScenarioResult.Completed;
                }
            }

            return ScenarioResult.Transition;
        }
    }
}