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

        public void Add(ToDoItem item)
        {
            _tasks.Add(item);
        }

        public void Delete(Guid id)
        {
            var item = Get(id);
            if (item != null)
                _tasks.Remove(item);
        }

        public ToDoItem? Get(Guid id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId).ToList().AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList().AsReadOnly();
        }

        public void Update(ToDoItem item)
        {
           
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _tasks.Any(t => t.User.UserId == userId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public int CountActive(Guid userId)
        {
            return _tasks.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return _tasks.Where(t => t.User.UserId == userId && predicate(t)).ToList().AsReadOnly();
        }
    }
}