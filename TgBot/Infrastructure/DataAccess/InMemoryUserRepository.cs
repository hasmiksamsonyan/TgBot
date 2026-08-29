using System;
using System.Collections.Generic;
using System.Linq;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();

        public Task Add(ToDoUser user, CancellationToken ct)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.UserId == userId));
        }

        public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.TelegramUserId == telegramUserId));
        }
    }
}
