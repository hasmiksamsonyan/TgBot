using System;
using System.IO;
using System.Text.Json;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _basePath;

        public FileUserRepository(string basePath)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            string filePath = Path.Combine(
                _basePath,
                $"{user.UserId}.json");

            string json = JsonSerializer.Serialize(user);

            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task<ToDoUser?> GetUser(
            Guid userId,
            CancellationToken ct)
        {
            string filePath = Path.Combine(
                _basePath,
                $"{userId}.json");

            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(filePath, ct);

            return JsonSerializer.Deserialize<ToDoUser>(json);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(
            long telegramUserId,
            CancellationToken ct)
        {
            if (!Directory.Exists(_basePath))
                return null;

            foreach (string filePath in Directory.GetFiles(_basePath, "*.json"))
            {
                ct.ThrowIfCancellationRequested();

                string json = await File.ReadAllTextAsync(filePath, ct);

                var user = JsonSerializer.Deserialize<ToDoUser>(json);

                if (user != null &&
                    user.TelegramUserId == telegramUserId)
                {
                    return user;
                }
            }

            return null;
        }
    }
}