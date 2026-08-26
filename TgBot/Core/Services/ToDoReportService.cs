using System;
using System.Linq;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _todoRepository;

        public ToDoReportService(IToDoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var allTasks = _todoRepository.GetAllByUserId(userId);
            var total = allTasks.Count;
            var completed = allTasks.Count(t => t.State == ToDoItemState.Completed);
            var active = allTasks.Count(t => t.State == ToDoItemState.Active);

          
            return (total, completed, active, DateTime.Now);
        }
    }
}