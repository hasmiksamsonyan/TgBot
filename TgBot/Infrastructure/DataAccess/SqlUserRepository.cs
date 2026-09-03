using LinqToDB;
using LinqToDB.Async;
using TgBot.Core.DataAccess;
using TgBot.Core.DataAccess.Models;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess;

public class SqlUserRepository : IUserRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlUserRepository(
        IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<ToDoUser?> GetUser(
        Guid userId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = await dbContext.ToDoUsers
            .FirstOrDefaultAsync(
                u => u.UserId == userId,
                ct);

        return model == null
            ? null
            : ModelMapper.MapFromModel(model);
    }

    public async Task<ToDoUser?> GetUserByTelegramUserId(
        long telegramUserId,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = await dbContext.ToDoUsers
            .FirstOrDefaultAsync(
                u => u.TelegramUserId == telegramUserId,
                ct);

        return model == null
            ? null
            : ModelMapper.MapFromModel(model);
    }

    public async Task Add(
        ToDoUser user,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var model = ModelMapper.MapToModel(user);

        await dbContext.InsertAsync(
            model,
            token: ct);
    }

    public async Task<IReadOnlyList<ToDoUser>> GetUsers(
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.ToDoUsers
            .ToListAsync(ct);

        return models
            .Select(ModelMapper.MapFromModel)
            .ToList();
    }
}