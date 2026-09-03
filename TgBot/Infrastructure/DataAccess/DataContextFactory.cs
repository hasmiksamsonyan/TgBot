namespace TgBot.Infrastructure.DataAccess;

public class DataContextFactory : IDataContextFactory<ToDoDataContext>
{
    private readonly string _connectionString;

    public DataContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public ToDoDataContext CreateDataContext()
    {
        return new ToDoDataContext(_connectionString);
    }
}