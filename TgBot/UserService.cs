using System.Collections.Generic;

namespace TelegramBot
{
    public class UserService : IUserService
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var existing = GetUser(telegramUserId);
            if (existing != null)
                throw new System.InvalidOperationException("Пользователь уже зарегистрирован");

            var user = new ToDoUser(telegramUserId, telegramUserName);
            _users.Add(user);
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _users.Find(u => u.TelegramUserId == telegramUserId);
        }
    }
}