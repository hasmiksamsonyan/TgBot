using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Core.DataAccess;
using TgBot.Core.Services;
using TgBot.Infrastructure.DataAccess;
using TgBot.Scenarios;

namespace TgBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(
    
            using var cts = new CancellationTokenSource();

            var userRepository =
                new FileUserRepository("Data/Users");

            var todoRepository =
                new FileToDoRepository("Data/Tasks");

            var todoListRepository =
                new FileToDoListRepository("Data/Lists");

            var userService =
                new UserService(userRepository);

            var todoService =
                new ToDoService(todoRepository);

            var todoListService =
                new ToDoListService(todoListRepository);

            var reportService =
                new ToDoReportService(todoRepository);

            var contextRepository =
                new InMemoryScenarioContextRepository();

            var scenarios = new IScenario[]
{
    new AddTaskScenario(
        userService,
        todoService,
        todoListService),

    new AddListScenario(
        userService,
        todoListService),

    new DeleteListScenario(
        userService,
        todoListService,
        todoService),

    new DeleteTaskScenario(
        todoService)
};

            var handler = new UpdateHandler(
                userService,
                todoService,
                reportService,
                todoListService,
                scenarios,
                contextRepository);

            var botClient =
                new TelegramBotClient("ТОКЕН");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates =
                [
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                ],
                DropPendingUpdates = true
            };

            await botClient.SetMyCommands(
                new[]
                {
                    new BotCommand
                    {
                        Command = "start",
                        Description = "Начать работу"
                    },
                    new BotCommand
                    {
                        Command = "addtask",
                        Description = "Добавить задачу"
                    },
                    new BotCommand
                    {
                        Command = "show",
                        Description = "Показать списки задач"
                    },
                    new BotCommand
                    {
                        Command = "report",
                        Description = "Показать статистику"
                    }
                },
                cancellationToken: cts.Token);

            botClient.StartReceiving(
                handler.HandleUpdateAsync,
                handler.HandleErrorAsync,
                receiverOptions,
                cts.Token);

            var me = await botClient.GetMe(cts.Token);

            Console.WriteLine(
                $"{me.FirstName} запущен!");

            Console.WriteLine(
                "Нажмите клавишу A для выхода.");

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.A)
                {
                    cts.Cancel();
                    break;
                }

                Console.WriteLine(
                    $"Бот: {me.FirstName}, username: @{me.Username}");
            }
        }
    }
}
