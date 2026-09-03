using System.Threading;

namespace TgBot.Scenarios
{
    public interface IScenarioContextRepository
    {
        ValueTask<ScenarioContext?> GetContext(
            long userId,
            CancellationToken ct);

        ValueTask SetContext(
            long userId,
            ScenarioContext context,
            CancellationToken ct);

        ValueTask ResetContext(
            long userId,
            CancellationToken ct);
    }
}