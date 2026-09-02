using System;

namespace TgBot.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; }
        public ToDoUser User { get; }
        public ToDoList? List { get; }
        public string Name { get; }
        public DateTime CreatedAt { get; }
        public DateTime Deadline { get; }
        public ToDoItemState State { get; private set; }
        public DateTime? StateChangedAt { get; private set; }

        public ToDoItem(
    ToDoUser user,
    string name,
    ToDoList? list,
    DateTime deadline)
        {
            Id = Guid.NewGuid();
            User = user;
            List = list;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            Deadline = deadline;
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