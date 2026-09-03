namespace TgBot.Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public long TelegramUserId { get; set; }
        public string TelegramUserName { get; set; } = null!;
        public DateTime RegisteredAt { get; set; }
    }
}