using System;

namespace TgBot.Core.Services
{
    public interface IToDoReportService
    {
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct);
    }
}
