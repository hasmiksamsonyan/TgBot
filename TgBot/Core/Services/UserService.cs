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

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var existing = await _userRepository.GetUserByTelegramUserId(telegramUserId, ct);
            if (existing != null)
                throw new InvalidOperationException("Пользователь уже зарегистрирован");

            var user = new ToDoUser(telegramUserId, telegramUserName);
            await _userRepository.Add(user, ct);
            return user;
        }

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct)
        {
            return await _userRepository.GetUserByTelegramUserId(telegramUserId, ct);
        }
    }
}
