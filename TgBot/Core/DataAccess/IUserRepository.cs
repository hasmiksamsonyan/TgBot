using System;
using TgBot.Core.Entities;

namespace TgBot.Core.DataAccess
{
    public interface IUserRepository
    {
        ToDoUser? GetUser(Guid userId);
        ToDoUser? GetUserByTelegramUserId(long telegramUserId);
        void Add(ToDoUser user);
    }
}