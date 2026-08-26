using System;

namespace TgBot.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; }
        public ToDoUser User { get; }
        public string Name { get; }
        public DateTime CreatedAt { get; }
        public ToDoItemState State { get; private set; }
        public DateTime? StateChangedAt { get; private set; }

        public ToDoItem(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            StateChangedAt = null;
        }

        public void Complete()
        {
            if (State == ToDoItemState.Completed)
                throw new InvalidOperationException("Задача уже выполнена");

            State = ToDoItemState.Completed;
            StateChangedAt = DateTime.UtcNow;
        }
    }
}