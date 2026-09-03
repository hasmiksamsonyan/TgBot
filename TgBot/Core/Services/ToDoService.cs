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

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(
            Guid userId,
            CancellationToken ct)
        {
            return await _todoRepository.GetAllByUserId(userId, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(
            Guid userId,
            CancellationToken ct)
        {
            return await _todoRepository.GetActiveByUserId(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(
    Guid userId,
    Guid? listId,
    CancellationToken ct)
        {
            var tasks = await _todoRepository.GetAllByUserId(
                userId,
                ct);

            return tasks
                .Where(task =>
                    task.List?.Id == listId)
                .ToList()
                .AsReadOnly();
        }

        public async Task<ToDoItem> Add(
    ToDoUser user,
    string name,
    DateTime deadline,
    ToDoList? list,
    CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            if (name.Length > MaxTaskLength)
                throw new TaskLengthLimitException(
                    name.Length,
                    MaxTaskLength);

            if (await _todoRepository.ExistsByName(
                user.UserId,
                name,
                ct))
            {
                throw new DuplicateTaskException(name);
            }

            if (await _todoRepository.CountActive(
                user.UserId,
                ct) >= MaxTaskCount)
            {
                throw new TaskCountLimitException(MaxTaskCount);
            }

            var task = new ToDoItem
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                List = list,
                CreatedAt = DateTime.UtcNow,
                Deadline = deadline,
                State = ToDoItemState.Active,
                StateChangedAt = null
            }; ;

            await _todoRepository.Add(task, ct);

            return task;
        }

        public async Task MarkCompleted(
            Guid id,
            CancellationToken ct)
        {
            var task = await _todoRepository.Get(id, ct);

            if (task == null)
                throw new ArgumentException("Задача не найдена");

            if (task.State == ToDoItemState.Completed)
            {
                throw new InvalidOperationException("Задача уже выполнена");
            }

            task.State = ToDoItemState.Completed;
            task.StateChangedAt = DateTime.UtcNow;

            await _todoRepository.Update(task, ct);
        }

        public async Task Delete(
            Guid id,
            CancellationToken ct)
        {
            await _todoRepository.Delete(id, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(
            ToDoUser user,
            string namePrefix,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(namePrefix))
                return new List<ToDoItem>().AsReadOnly();

            return await _todoRepository.Find(
                user.UserId,
                task => task.Name.StartsWith(
                    namePrefix,
                    StringComparison.OrdinalIgnoreCase),
                ct);
        }
        public async Task<ToDoItem?> Get(
            Guid toDoItemId,CancellationToken ct)
                {
                    return await _todoRepository.Get(toDoItemId,ct);
                }
    }
}