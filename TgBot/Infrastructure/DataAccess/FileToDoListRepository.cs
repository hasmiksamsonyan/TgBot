using System.Text.Json;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _baseFolder;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        public FileToDoListRepository(string baseFolder)
        {
            _baseFolder = baseFolder;

            if (!Directory.Exists(_baseFolder))
            {
                Directory.CreateDirectory(_baseFolder);
            }
        }

        public async Task<ToDoList?> Get(
            Guid id,
            CancellationToken ct)
        {
            string filePath = Path.Combine(
                _baseFolder,
                $"{id}.json");

            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(
                filePath,
                ct);

            return JsonSerializer.Deserialize<ToDoList>(
                json,
                _jsonOptions);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(
            Guid userId,
            CancellationToken ct)
        {
            var result = new List<ToDoList>();

            foreach (string filePath in Directory.GetFiles(
                _baseFolder,
                "*.json"))
            {
                ct.ThrowIfCancellationRequested();

                string json = await File.ReadAllTextAsync(
                    filePath,
                    ct);

                var list = JsonSerializer.Deserialize<ToDoList>(
                    json,
                    _jsonOptions);

                if (list != null && list.User.UserId == userId)
                {
                    result.Add(list);
                }
            }

            return result.AsReadOnly();
        }

        public async Task Add(
            ToDoList list,
            CancellationToken ct)
        {
            string filePath = Path.Combine(
                _baseFolder,
                $"{list.Id}.json");

            string json = JsonSerializer.Serialize(
                list,
                _jsonOptions);

            await File.WriteAllTextAsync(
                filePath,
                json,
                ct);
        }

        public async Task Delete(
            Guid id,
            CancellationToken ct)
        {
            string filePath = Path.Combine(
                _baseFolder,
                $"{id}.json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> ExistsByName(
            Guid userId,
            string name,
            CancellationToken ct)
        {
            var lists = await GetByUserId(userId, ct);

            return lists.Any(list =>
                list.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}