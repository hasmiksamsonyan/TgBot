using System.Collections.Concurrent;

namespace TgBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _contexts =
            new ConcurrentDictionary<long, ScenarioContext>();

        public Task<ScenarioContext?> GetContext(
            long userId,
            CancellationToken ct)
        {
            _contexts.TryGetValue(userId, out var context);

            return Task.FromResult(context);
        }

        public Task SetContext(
            long userId,
            ScenarioContext context,
            CancellationToken ct)
        {
            _contexts[userId] = context;

            return Task.CompletedTask;
        }

        public Task ResetContext(
            long userId,
            CancellationToken ct)
        {
            _contexts.TryRemove(userId, out _);

            return Task.CompletedTask;
        }
    }
}