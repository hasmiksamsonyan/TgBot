using LinqToDB;
using LinqToDB.Data;
using TgBot.Core.DataAccess.Models;

namespace TgBot.Infrastructure.DataAccess;

public class ToDoDataContext : DataConnection
{
    public ToDoDataContext(string connectionString)
        : base(
            new DataOptions()
                .UseConnectionString(
                    ProviderName.PostgreSQL,
                    connectionString))
    {
    }

    public ITable<ToDoUserModel> ToDoUsers =>
        DataExtensions.GetTable<ToDoUserModel>(this);

    public ITable<ToDoListModel> ToDoLists =>
        DataExtensions.GetTable<ToDoListModel>(this);

    public ITable<ToDoItemModel> ToDoItems =>
        DataExtensions.GetTable<ToDoItemModel>(this);

    public ITable<NotificationModel> Notifications =>
        DataExtensions.GetTable<NotificationModel>(this);
}