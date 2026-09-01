using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var existing = _userRepository.GetUserByTelegramUserId(telegramUserId);
            if (existing != null)
                throw new InvalidOperationException("Пользователь уже зарегистрирован");

            var user = new ToDoUser(telegramUserId, telegramUserName);
            _userRepository.Add(user);
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
        }
    }
}