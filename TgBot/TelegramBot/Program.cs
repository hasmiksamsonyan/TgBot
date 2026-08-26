using System;
using Otus.ToDoList.ConsoleBot;
using TgBot.Core.DataAccess;
using TgBot.Core.Services;
using TgBot.Infrastructure.DataAccess;

namespace TgBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                var userRepository = new InMemoryUserRepository();
                var todoRepository = new InMemoryToDoRepository();

                
                var userService = new UserService(userRepository);
                var todoService = new ToDoService(todoRepository);
                var reportService = new ToDoReportService(todoRepository);

                
                var handler = new UpdateHandler(userService, todoService, reportService);

                
                var botClient = new ConsoleBotClient();
                botClient.StartReceiving(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}