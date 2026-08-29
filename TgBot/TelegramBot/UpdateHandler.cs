using System;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using TgBot.Core.Entities;
using TgBot.Core.Services;

namespace TgBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;

        public UpdateHandler(IUserService userService, IToDoService todoService, IToDoReportService reportService)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update.Message == null || update.Message.From == null)
                    return;

                var tgUser = update.Message.From;
                var chat = update.Message.Chat;
                var text = update.Message.Text?.Trim() ?? "";

                var user = await _userService.GetUser(tgUser.Id, ct);
                if (user == null)
                {
                    user = await _userService.RegisterUser(tgUser.Id, tgUser.Username ?? "User", ct);
                    await botClient.SendMessage(chat, $"Добро пожаловать, {user.TelegramUserName}!", ct);
                    return;
                }

                if (text == "/help")
                    await ShowHelp(botClient, chat, ct);
                else if (text == "/info")
                    await ShowInfo(botClient, chat, user, ct);
                else if (text == "/showtasks")
                    await ShowActiveTasks(botClient, chat, user, ct);
                else if (text == "/showalltasks")
                    await ShowAllTasks(botClient, chat, user, ct);
                else if (text == "/report")
                    await ShowReport(botClient, chat, user, ct);
                else if (text.StartsWith("/find"))
                    await FindTasks(botClient, chat, user, text, ct);
                else if (text.StartsWith("/addtask"))
                    await AddTask(botClient, chat, user, text, ct);
                else if (text.StartsWith("/completetask "))
                    await CompleteTask(botClient, chat, text, ct);
                else if (text.StartsWith("/removetask "))
                    await RemoveTask(botClient, chat, user, text, ct);
                else if (text == "/exit")
                {
                    await botClient.SendMessage(chat, "До свидания!", ct);
                    Environment.Exit(0);
                }
                else
                {
                    await botClient.SendMessage(chat, "Неизвестная команда. Используйте /help", ct);
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, ct);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception}");
            return Task.CompletedTask;
        }

        private async Task ShowHelp(ITelegramBotClient bot, Chat chat, CancellationToken ct)
        {
            await bot.SendMessage(chat,
                "/help - справка\n" +
                "/info - информация\n" +
                "/report - статистика по задачам\n" +
                "/find [текст] - найти задачи по префиксу\n" +
                "/addtask [название] - добавить задачу\n" +
                "/showtasks - активные задачи\n" +
                "/showalltasks - все задачи\n" +
                "/completetask [id] - выполнить задачу\n" +
                "/removetask [номер] - удалить задачу\n" +
                "/exit - выход", ct);
        }

        private async Task ShowInfo(ITelegramBotClient bot, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var all = await _todoService.GetAllByUserId(user.UserId, ct);
            var active = await _todoService.GetActiveByUserId(user.UserId, ct);

            await bot.SendMessage(chat,
                $"Пользователь: {user.TelegramUserName}\n" +
                $"Всего задач: {all.Count}\n" +
                $"Активных: {active.Count}\n" +
                $"Выполненных: {all.Count - active.Count}", ct);
        }

        private async Task ShowActiveTasks(ITelegramBotClient bot, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var tasks = await _todoService.GetActiveByUserId(user.UserId, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(chat, "Активных задач нет", ct);
                return;
            }

            string msg = "Активные задачи:\n";
            for (int i = 0; i < tasks.Count; i++)
                msg += $"{i + 1}. {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";

            await bot.SendMessage(chat, msg, ct);
        }

        private async Task ShowAllTasks(ITelegramBotClient bot, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var tasks = await _todoService.GetAllByUserId(user.UserId, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(chat, "Задач нет", ct);
                return;
            }

            string msg = "Все задачи:\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                string state = tasks[i].State == ToDoItemState.Active ? "Активная" : "Выполнена";
                msg += $"{i + 1}. ({state}) {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";
            }

            await bot.SendMessage(chat, msg, ct);
        }

        private async Task ShowReport(ITelegramBotClient bot, Chat chat, ToDoUser user, CancellationToken ct)
        {
            var stats = await _reportService.GetUserStats(user.UserId, ct);

            await bot.SendMessage(chat,
                $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
                $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};", ct);
        }

        private async Task FindTasks(ITelegramBotClient bot, Chat chat, ToDoUser user, string command, CancellationToken ct)
        {
            string prefix = command.Substring("/find".Length).Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await bot.SendMessage(chat, "Укажите префикс для поиска. Пример: /find Куп", ct);
                return;
            }

            var tasks = await _todoService.Find(user, prefix, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(chat, $"Задачи, начинающиеся на '{prefix}', не найдены.", ct);
                return;
            }

            string msg = $"Найдено задач, начинающихся на '{prefix}':\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                string state = tasks[i].State == ToDoItemState.Active ? "Активная" : "Выполнена";
                msg += $"{i + 1}. ({state}) {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";
            }

            await bot.SendMessage(chat, msg, ct);
        }

        private async Task AddTask(ITelegramBotClient bot, Chat chat, ToDoUser user, string command, CancellationToken ct)
        {
            string name = command.Substring("/addtask".Length).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                await bot.SendMessage(chat, "Укажите название: /addtask Купить продукты", ct);
                return;
            }

            try
            {
                var task = await _todoService.Add(user, name, ct);
                await bot.SendMessage(chat, $"Задача '{name}' добавлена. ID: {task.Id}", ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chat, $"Ошибка: {ex.Message}", ct);
            }
        }

        private async Task CompleteTask(ITelegramBotClient bot, Chat chat, string command, CancellationToken ct)
        {
            string idStr = command.Substring("/completetask".Length).Trim();

            if (!Guid.TryParse(idStr, out Guid id))
            {
                await bot.SendMessage(chat, "Укажите корректный ID задачи", ct);
                return;
            }

            try
            {
                await _todoService.MarkCompleted(id, ct);
                await bot.SendMessage(chat, $"Задача {id} выполнена!", ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chat, $"Ошибка: {ex.Message}", ct);
            }
        }

        private async Task RemoveTask(ITelegramBotClient bot, Chat chat, ToDoUser user, string command, CancellationToken ct)
        {
            string numStr = command.Substring("/removetask".Length).Trim();

            if (!int.TryParse(numStr, out int number) || number < 1)
            {
                await bot.SendMessage(chat, "Укажите корректный номер задачи", ct);
                return;
            }

            try
            {
                var tasks = await _todoService.GetAllByUserId(user.UserId, ct);

                if (number > tasks.Count)
                {
                    await bot.SendMessage(chat, $"Номер от 1 до {tasks.Count}", ct);
                    return;
                }

                var task = tasks[number - 1];
                await _todoService.Delete(task.Id, ct);
                await bot.SendMessage(chat, $"Задача '{task.Name}' удалена", ct);
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chat, $"Ошибка: {ex.Message}", ct);
            }
        }
    }
}