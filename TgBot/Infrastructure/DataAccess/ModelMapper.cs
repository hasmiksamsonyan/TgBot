using TgBot.Core.DataAccess.Models;
using TgBot.Core.Entities;

namespace TgBot.Infrastructure.DataAccess;

internal static class ModelMapper
{
    public static ToDoUser MapFromModel(ToDoUserModel model)
    {
        return new ToDoUser
        {
            UserId = model.UserId,
            TelegramUserId = model.TelegramUserId,
            TelegramUserName = model.TelegramUserName,
            RegisteredAt = model.RegisteredAt
        };
    }

    public static ToDoUserModel MapToModel(ToDoUser entity)
    {
        return new ToDoUserModel
        {
            UserId = entity.UserId,
            TelegramUserId = entity.TelegramUserId,
            TelegramUserName = entity.TelegramUserName,
            RegisteredAt = entity.RegisteredAt
        };
    }

    public static ToDoList MapFromModel(ToDoListModel model)
    {
        return new ToDoList
        {
            Id = model.Id,
            User = MapFromModel(model.User),
            Name = model.Name,
            CreatedAt = model.CreatedAt
        };
    }

    public static ToDoListModel MapToModel(ToDoList entity)
    {
        return new ToDoListModel
        {
            Id = entity.Id,
            UserId = entity.User.UserId,
            User = MapToModel(entity.User),
            Name = entity.Name,
            CreatedAt = entity.CreatedAt
        };
    }

    public static ToDoItem MapFromModel(ToDoItemModel model)
    {
        return new ToDoItem
        {
            Id = model.Id,
            User = MapFromModel(model.User),
            List = model.List == null
                ? null
                : MapFromModel(model.List),
            Name = model.Name,
            CreatedAt = model.CreatedAt,
            Deadline = model.Deadline,
            State = model.State,
            StateChangedAt = model.StateChangedAt
        };
    }

    public static ToDoItemModel MapToModel(ToDoItem entity)
    {
        return new ToDoItemModel
        {
            Id = entity.Id,
            UserId = entity.User.UserId,
            User = MapToModel(entity.User),
            ListId = entity.List?.Id,
            List = entity.List == null
                ? null
                : MapToModel(entity.List),
            Name = entity.Name,
            CreatedAt = entity.CreatedAt,
            Deadline = entity.Deadline,
            State = entity.State,
            StateChangedAt = entity.StateChangedAt
        };
    }

    public static Notification MapFromModel(NotificationModel model)
    {
        return new Notification
        {
            Id = model.Id,
            User = MapFromModel(model.User),
            Type = model.Type,
            Text = model.Text,
            ScheduledAt = model.ScheduledAt,
            IsNotified = model.IsNotified,
            NotifiedAt = model.NotifiedAt
        };
    }

    public static NotificationModel MapToModel(Notification entity)
    {
        return new NotificationModel
        {
            Id = entity.Id,
            UserId = entity.User.UserId,
            User = MapToModel(entity.User),
            Type = entity.Type,
            Text = entity.Text,
            ScheduledAt = entity.ScheduledAt,
            IsNotified = entity.IsNotified,
            NotifiedAt = entity.NotifiedAt
        };
    }
}