using System.Collections.Concurrent;

namespace TgBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _contexts =
            new ConcurrentDictionary<long, ScenarioContext>();

        public ValueTask<ScenarioContext?> GetContext(
            long userId,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return new ValueTask<ScenarioContext?>(
                _contexts.TryGetValue(userId, out var context)
                    ? context
                    : null);
        }

        public ValueTask SetContext(
            long userId,
            ScenarioContext context,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            _contexts[userId] = context;

            return ValueTask.CompletedTask;
        }

        public ValueTask ResetContext(
            long userId,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            _contexts.TryRemove(userId, out _);

            return ValueTask.CompletedTask;
        }
    }
}