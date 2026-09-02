using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgBot.Scenarios
{
    public interface IScenario
    {
        bool CanHandle(ScenarioType scenario);

        Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Message message,
            CancellationToken ct);

        Task<ScenarioResult> HandleCallbackQueryAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct);
    }
}