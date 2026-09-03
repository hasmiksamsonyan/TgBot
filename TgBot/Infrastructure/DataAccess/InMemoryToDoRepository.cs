using System;
using System.Collections.Generic;
using System.Linq;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _tasks = new List<ToDoItem>();

        public Task Add(ToDoItem item, CancellationToken ct)
        {
            _tasks.Add(item);
            return Task.CompletedTask;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            var item = _tasks.FirstOrDefault(t => t.Id == id);

            if (item != null)
                _tasks.Remove(item);

            return Task.CompletedTask;
        }

        public Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            return Task.FromResult(
                _tasks.FirstOrDefault(t => t.Id == id));
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(
            Guid userId,
            CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(
                _tasks
                    .Where(t => t.User.UserId == userId)
                    .ToList()
                    .AsReadOnly());
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(
            Guid userId,
            CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(
                _tasks
                    .Where(t =>
                        t.User.UserId == userId &&
                        t.State == ToDoItemState.Active)
                    .ToList()
                    .AsReadOnly());
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(
            Guid userId,
            DateTime from,
            DateTime to,
            CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(
                _tasks
                    .Where(t =>
                        t.User.UserId == userId &&
                        t.State == ToDoItemState.Active &&
                        t.Deadline >= from &&
                        t.Deadline <= to)
                    .ToList()
                    .AsReadOnly());
        }

        public Task Update(ToDoItem item, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByName(
            Guid userId,
            string name,
            CancellationToken ct)
        {
            return Task.FromResult(
                _tasks.Any(t =>
                    t.User.UserId == userId &&
                    t.Name.Equals(
                        name,
                        StringComparison.OrdinalIgnoreCase)));
        }

        public Task<int> CountActive(
            Guid userId,
            CancellationToken ct)
        {
            return Task.FromResult(
                _tasks.Count(t =>
                    t.User.UserId == userId &&
                    t.State == ToDoItemState.Active));
        }

        public Task<IReadOnlyList<ToDoItem>> Find(
            Guid userId,
            Func<ToDoItem, bool> predicate,
            CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<ToDoItem>>(
                _tasks
                    .Where(t =>
                        t.User.UserId == userId &&
                        predicate(t))
                    .ToList()
                    .AsReadOnly());
        }
    }
}