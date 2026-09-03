using LinqToDB;
using LinqToDB.Async;
using TgBot.Core.DataAccess;
using TgBot.Core.DataAccess.Models;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess;

public class SqlToDoListRepository : IToDoListRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlToDoListRepository(
        IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<ToDoList?> Get(
        Guid id,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = await dbContext.ToDoLists
            .LoadWith(l => l.User)
            .FirstOrDefaultAsync(
                l => l.Id == id,
                ct);

        return model == null
            ? null
            : ModelMapper.MapFromModel(model);
    }

    public async Task<IReadOnlyList<ToDoList>> GetByUserId(
        Guid userId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.ToDoLists
            .LoadWith(l => l.User)
            .Where(l => l.UserId == userId)
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .ToList();
    }

    public async Task Add(
        ToDoList list,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = ModelMapper.MapToModel(list);

        await dbContext.InsertAsync(
            model,
            token: ct);
    }

    public async Task Delete(
        Guid id,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        await dbContext.ToDoLists
            .Where(l => l.Id == id)
            .DeleteAsync(token: ct);
    }

    public async Task<bool> ExistsByName(
        Guid userId,
        string name,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var normalizedName = name.ToLower();

        return await dbContext.ToDoLists
            .AnyAsync(
                l => l.UserId == userId &&
                     l.Name.ToLower() == normalizedName,
                ct);
    }
}