using LinqToDB;
using LinqToDB.Async;
using TgBot.Core.DataAccess;
using TgBot.Core.DataAccess.Models;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess;

public class SqlToDoRepository : IToDoRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlToDoRepository(
        IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(
        Guid userId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .ToList();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(
        Guid userId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i =>
                i.UserId == userId &&
                i.State == ToDoItemState.Active)
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .ToList();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(
        Guid userId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i =>
                i.UserId == userId &&
                i.State == ToDoItemState.Active &&
                i.Deadline >= from &&
                i.Deadline <= to)
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .ToList();
    }

    public async Task<ToDoItem?> Get(
        Guid id,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .FirstOrDefaultAsync(
                i => i.Id == id,
                ct);

        return model == null
            ? null
            : ModelMapper.MapFromModel(model);
    }

    public async Task Add(
        ToDoItem item,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = ModelMapper.MapToModel(item);

        await dbContext.InsertAsync(
            model,
            token: ct);
    }

    public async Task Update(
        ToDoItem item,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = ModelMapper.MapToModel(item);

        await dbContext.UpdateAsync(
            model,
            token: ct);
    }

    public async Task Delete(
        Guid id,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        await dbContext.ToDoItems
            .Where(i => i.Id == id)
            .DeleteAsync(token: ct);
    }

    public async Task<bool> ExistsByName(
        Guid userId,
        string name,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var normalizedName = name.ToLower();

        return await dbContext.ToDoItems
            .AnyAsync(
                i => i.UserId == userId &&
                     i.Name.ToLower() == normalizedName,
                ct);
    }

    public async Task<int> CountActive(
        Guid userId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        return await dbContext.ToDoItems
            .CountAsync(
                i => i.UserId == userId &&
                     i.State == ToDoItemState.Active,
                ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(
        Guid userId,
        Func<ToDoItem, bool> predicate,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId)
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .Where(predicate)
            .ToList();
    }
}