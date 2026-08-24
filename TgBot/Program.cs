using System;
using Otus.ToDoList.ConsoleBot;

namespace TelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var userService = new UserService();
                var todoService = new ToDoService();
                var handler = new UpdateHandler(userService, todoService);
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