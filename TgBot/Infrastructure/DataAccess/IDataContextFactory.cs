using LinqToDB.Data;

namespace TgBot.Infrastructure.DataAccess;

public interface IDataContextFactory<TDataContext>
    where TDataContext : DataConnection
{
    TDataContext CreateDataContext();
}