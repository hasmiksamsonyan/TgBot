using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Scenarios;

namespace TgBot.BackgroundTasks
{
    public class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;

        private readonly IScenarioContextRepository _scenarioRepository;

        private readonly ITelegramBotClient _bot;

        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot)
            : base(
    TimeSpan.FromHours(1),
    nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout =
                resetScenarioTimeout;

            _scenarioRepository =
                scenarioRepository;

            _bot = bot;
        }

        protected override async Task Execute(
            CancellationToken ct)
        {
            var contexts =
                await _scenarioRepository.GetContexts(ct);

            var now = DateTime.UtcNow;

            foreach (var item in contexts)
            {
                var userId = item.UserId;
                var context = item.Context;

                if (now - context.CreatedAt <
                    _resetScenarioTimeout)
                {
                    continue;
                }

                await _scenarioRepository.ResetContext(
                    userId,
                    ct);

                await _bot.SendMessage(
                    userId,
                    $"Сценарий отменен, так как не поступил ответ " +
                    $"в течение {_resetScenarioTimeout}",
                    replyMarkup: GetMainKeyboard(),
                    cancellationToken: ct);
            }
        }

        private static ReplyKeyboardMarkup GetMainKeyboard()
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