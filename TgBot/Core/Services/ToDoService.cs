using System;
using System.Collections.Generic;
using System.Linq;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;
using TgBot.Core.Exceptions;

namespace TgBot.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _todoRepository;
        private const int MaxTaskCount = 100;
        private const int MaxTaskLength = 200;

        public ToDoService(IToDoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _todoRepository.GetAllByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _todoRepository.GetActiveByUserId(userId);
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            if (name.Length > MaxTaskLength)
                throw new TaskLengthLimitException(name.Length, MaxTaskLength);

            if (_todoRepository.ExistsByName(user.UserId, name))
                throw new DuplicateTaskException(name);

            if (_todoRepository.CountActive(user.UserId) >= MaxTaskCount)
                throw new TaskCountLimitException(MaxTaskCount);

            var task = new ToDoItem(user, name);
            _todoRepository.Add(task);
            return task;
        }

        public void MarkCompleted(Guid id)
        {
            var task = _todoRepository.Get(id);
            if (task == null)
                throw new ArgumentException("Задача не найдена");

            task.Complete();
            _todoRepository.Update(task);
        }

        public void Delete(Guid id)
        {
            _todoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            if (string.IsNullOrWhiteSpace(namePrefix))
                return new List<ToDoItem>().AsReadOnly();

            
            return _todoRepository.Find(user.UserId, task =>
                task.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
        }
    }
}