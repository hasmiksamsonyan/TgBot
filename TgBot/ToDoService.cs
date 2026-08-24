using System;
using System.Collections.Generic;

namespace TelegramBot
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _tasks = new List<ToDoItem>();
        private const int MaxTaskCount = 100;
        private const int MaxTaskLength = 200;

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.FindAll(t => t.User.UserId == userId).AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.FindAll(t => t.User.UserId == userId && t.State == ToDoItemState.Active).AsReadOnly();
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            if (name.Length > MaxTaskLength)
                throw new ArgumentException($"Длина задачи превышает {MaxTaskLength} символов");

            var userTasks = _tasks.FindAll(t => t.User.UserId == user.UserId);

            if (userTasks.Exists(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Задача '{name}' уже существует");

            if (userTasks.Count >= MaxTaskCount)
                throw new InvalidOperationException($"Максимум {MaxTaskCount} задач");

            var task = new ToDoItem(user, name);
            _tasks.Add(task);
            return task;
        }

        public void MarkCompleted(Guid id)
        {
            var task = _tasks.Find(t => t.Id == id);
            if (task == null)
                throw new ArgumentException("Задача не найдена");

            task.Complete();
        }

        public void Delete(Guid id)
        {
            var task = _tasks.Find(t => t.Id == id);
            if (task == null)
                throw new ArgumentException("Задача не найдена");

            _tasks.Remove(task);
        }
    }
}