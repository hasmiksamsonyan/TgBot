using System.Text.Json;
using TgBot.Core.DataAccess;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _baseFolder;
        private readonly string _indexFile;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        public FileToDoRepository(string baseFolder)
        {
            _baseFolder = baseFolder;
            _indexFile = Path.Combine(_baseFolder, "index.json");

            if (!Directory.Exists(_baseFolder))
            {
                Directory.CreateDirectory(_baseFolder);
            }
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            string userFolder = Path.Combine(
                _baseFolder,
                item.User.UserId.ToString());

            Directory.CreateDirectory(userFolder);

            string filePath = Path.Combine(
                userFolder,
                $"{item.Id}.json");

            string json = JsonSerializer.Serialize(
                item,
                _jsonOptions);

            await File.WriteAllTextAsync(
                filePath,
                json,
                ct);

            var index = await GetIndex(ct);

            index[item.Id] = item.User.UserId;

            await SaveIndex(index, ct);
        }

        public async Task<ToDoItem?> Get(
            Guid id,
            CancellationToken ct)
        {
            var index = await GetIndex(ct);

            if (!index.TryGetValue(id, out Guid userId))
                return null;

            string filePath = Path.Combine(
                _baseFolder,
                userId.ToString(),
                $"{id}.json");

            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(
                filePath,
                ct);

            return JsonSerializer.Deserialize<ToDoItem>(
                json,
                _jsonOptions);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(
            Guid userId,
            CancellationToken ct)
        {
            string userFolder = Path.Combine(
                _baseFolder,
                userId.ToString());

            if (!Directory.Exists(userFolder))
                return Array.Empty<ToDoItem>();

            var result = new List<ToDoItem>();

            foreach (string filePath in Directory.GetFiles(
                userFolder,
                "*.json"))
            {
                ct.ThrowIfCancellationRequested();

                string json = await File.ReadAllTextAsync(
                    filePath,
                    ct);

                var item = JsonSerializer.Deserialize<ToDoItem>(
                    json,
                    _jsonOptions);

                if (item != null)
                    result.Add(item);
            }

            return result.AsReadOnly();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(
            Guid userId,
            CancellationToken ct)
        {
            var tasks = await GetAllByUserId(userId, ct);

            return tasks
                .Where(t => t.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();
        }

        public async Task Update(
            ToDoItem item,
            CancellationToken ct)
        {
            string userFolder = Path.Combine(
                _baseFolder,
                item.User.UserId.ToString());

            Directory.CreateDirectory(userFolder);

            string filePath = Path.Combine(
                userFolder,
                $"{item.Id}.json");

            string json = JsonSerializer.Serialize(
                item,
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
            var index = await GetIndex(ct);

            if (!index.TryGetValue(id, out Guid userId))
                return;

            string filePath = Path.Combine(
                _baseFolder,
                userId.ToString(),
                $"{id}.json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            index.Remove(id);

            await SaveIndex(index, ct);
        }

        public async Task<bool> ExistsByName(
            Guid userId,
            string name,
            CancellationToken ct)
        {
            var tasks = await GetAllByUserId(userId, ct);

            return tasks.Any(t =>
                t.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> CountActive(
            Guid userId,
            CancellationToken ct)
        {
            var tasks = await GetActiveByUserId(userId, ct);

            return tasks.Count;
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(
            Guid userId,
            Func<ToDoItem, bool> predicate,
            CancellationToken ct)
        {
            var tasks = await GetAllByUserId(userId, ct);

            return tasks
                .Where(predicate)
                .ToList()
                .AsReadOnly();
        }

        private async Task<Dictionary<Guid, Guid>> GetIndex(
            CancellationToken ct)
        {
            if (!File.Exists(_indexFile))
            {
                var index = await BuildIndex(ct);
                await SaveIndex(index, ct);
                return index;
            }

            string json = await File.ReadAllTextAsync(
                _indexFile,
                ct);

            return JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(
                json,
                _jsonOptions) ?? new Dictionary<Guid, Guid>();
        }

        private async Task<Dictionary<Guid, Guid>> BuildIndex(
            CancellationToken ct)
        {
            var index = new Dictionary<Guid, Guid>();

            foreach (string userFolder in Directory.GetDirectories(
                _baseFolder))
            {
                ct.ThrowIfCancellationRequested();

                string folderName = Path.GetFileName(userFolder);

                if (!Guid.TryParse(folderName, out Guid userId))
                    continue;

                foreach (string filePath in Directory.GetFiles(
                    userFolder,
                    "*.json"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(
                        filePath);

                    if (Guid.TryParse(fileName, out Guid taskId))
                    {
                        index[taskId] = userId;
                    }
                }
            }

            return index;
        }

        private async Task SaveIndex(
            Dictionary<Guid, Guid> index,
            CancellationToken ct)
        {
            string json = JsonSerializer.Serialize(
                index,
                _jsonOptions);

            await File.WriteAllTextAsync(
                _indexFile,
                json,
                ct);
        }
    }
}