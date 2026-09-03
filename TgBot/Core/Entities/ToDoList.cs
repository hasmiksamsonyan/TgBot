namespace TgBot.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ToDoUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}