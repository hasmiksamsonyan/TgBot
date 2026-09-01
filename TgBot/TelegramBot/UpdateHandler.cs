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

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                if (update.Message == null || update.Message.From == null)
                    return;

                var tgUser = update.Message.From;
                var chat = update.Message.Chat;
                var text = update.Message.Text?.Trim() ?? "";

                var user = _userService.GetUser(tgUser.Id);
                if (user == null)
                {
                    user = _userService.RegisterUser(tgUser.Id, tgUser.Username ?? "User");
                    botClient.SendMessage(chat, $"Добро пожаловать, {user.TelegramUserName}!");
                    return;
                }

                if (text == "/help")
                    ShowHelp(botClient, chat);
                else if (text == "/info")
                    ShowInfo(botClient, chat, user);
                else if (text == "/showtasks")
                    ShowActiveTasks(botClient, chat, user);
                else if (text == "/showalltasks")
                    ShowAllTasks(botClient, chat, user);
                else if (text == "/report")
                    ShowReport(botClient, chat, user);
                else if (text.StartsWith("/find"))
                    FindTasks(botClient, chat, user, text);
                else if (text.StartsWith("/addtask"))
                    AddTask(botClient, chat, user, text);
                else if (text.StartsWith("/completetask "))
                    CompleteTask(botClient, chat, text);
                else if (text.StartsWith("/removetask "))
                    RemoveTask(botClient, chat, user, text);
                else if (text == "/exit")
                {
                    botClient.SendMessage(chat, "До свидания!");
                    Environment.Exit(0);
                }
                else
                {
                    botClient.SendMessage(chat, "Неизвестная команда. Используйте /help");
                }
            }
            catch (Exception ex)
            {
                botClient.SendMessage(update.Message?.Chat ?? new Chat(), $"Ошибка: {ex.Message}");
            }
        }

        

        private void ShowHelp(ITelegramBotClient bot, Chat chat)
        {
            bot.SendMessage(chat,
                "/help - справка\n" +
                "/info - информация\n" +
                "/report - статистика по задачам\n" +
                "/find [текст] - найти задачи по префиксу\n" +
                "/addtask [название] - добавить задачу\n" +
                "/showtasks - активные задачи\n" +
                "/showalltasks - все задачи\n" +
                "/completetask [id] - выполнить задачу\n" +
                "/removetask [номер] - удалить задачу\n" +
                "/exit - выход");
        }

        private void ShowInfo(ITelegramBotClient bot, Chat chat, ToDoUser user)
        {
            var all = _todoService.GetAllByUserId(user.UserId);
            var active = _todoService.GetActiveByUserId(user.UserId);

            bot.SendMessage(chat,
                $"Пользователь: {user.TelegramUserName}\n" +
                $"Всего задач: {all.Count}\n" +
                $"Активных: {active.Count}\n" +
                $"Выполненных: {all.Count - active.Count}");
        }

        private void ShowActiveTasks(ITelegramBotClient bot, Chat chat, ToDoUser user)
        {
            var tasks = _todoService.GetActiveByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                bot.SendMessage(chat, "Активных задач нет");
                return;
            }

            string msg = "Активные задачи:\n";
            for (int i = 0; i < tasks.Count; i++)
                msg += $"{i + 1}. {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";

            bot.SendMessage(chat, msg);
        }

        private void ShowAllTasks(ITelegramBotClient bot, Chat chat, ToDoUser user)
        {
            var tasks = _todoService.GetAllByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                bot.SendMessage(chat, "Задач нет");
                return;
            }

            string msg = "Все задачи:\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                string state = tasks[i].State == ToDoItemState.Active ? "Активная" : "Выполнена";
                msg += $"{i + 1}. ({state}) {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";
            }

            bot.SendMessage(chat, msg);
        }

        private void ShowReport(ITelegramBotClient bot, Chat chat, ToDoUser user)
        {
            var stats = _reportService.GetUserStats(user.UserId);

            bot.SendMessage(chat,
                $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}.\n" +
                $"Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};");
        }

        private void FindTasks(ITelegramBotClient bot, Chat chat, ToDoUser user, string command)
        {
            string prefix = command.Substring("/find".Length).Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                bot.SendMessage(chat, "Укажите префикс для поиска. Пример: /find Куп");
                return;
            }

            var tasks = _todoService.Find(user, prefix);

            if (tasks.Count == 0)
            {
                bot.SendMessage(chat, $"Задачи, начинающиеся на '{prefix}', не найдены.");
                return;
            }

            string msg = $"Найдено задач, начинающихся на '{prefix}':\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                string state = tasks[i].State == ToDoItemState.Active ? "Активная" : "Выполнена";
                msg += $"{i + 1}. ({state}) {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";
            }

            bot.SendMessage(chat, msg);
        }

        private void AddTask(ITelegramBotClient bot, Chat chat, ToDoUser user, string command)
        {
            string name = command.Substring("/addtask".Length).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                bot.SendMessage(chat, "Укажите название: /addtask Купить продукты");
                return;
            }

            try
            {
                var task = _todoService.Add(user, name);
                bot.SendMessage(chat, $"Задача '{name}' добавлена. ID: {task.Id}");
            }
            catch (Exception ex)
            {
                bot.SendMessage(chat, $"Ошибка: {ex.Message}");
            }
        }

        private void CompleteTask(ITelegramBotClient bot, Chat chat, string command)
        {
            string idStr = command.Substring("/completetask".Length).Trim();

            if (!Guid.TryParse(idStr, out Guid id))
            {
                bot.SendMessage(chat, "Укажите корректный ID задачи");
                return;
            }

            try
            {
                _todoService.MarkCompleted(id);
                bot.SendMessage(chat, $"Задача {id} выполнена!");
            }
            catch (Exception ex)
            {
                bot.SendMessage(chat, $"Ошибка: {ex.Message}");
            }
        }

        private void RemoveTask(ITelegramBotClient bot, Chat chat, ToDoUser user, string command)
        {
            string numStr = command.Substring("/removetask".Length).Trim();

            if (!int.TryParse(numStr, out int number) || number < 1)
            {
                bot.SendMessage(chat, "Укажите корректный номер задачи");
                return;
            }

            try
            {
                var tasks = _todoService.GetAllByUserId(user.UserId);

                if (number > tasks.Count)
                {
                    bot.SendMessage(chat, $"Номер от 1 до {tasks.Count}");
                    return;
                }

                var task = tasks[number - 1];
                _todoService.Delete(task.Id);
                bot.SendMessage(chat, $"Задача '{task.Name}' удалена");
            }
            catch (Exception ex)
            {
                bot.SendMessage(chat, $"Ошибка: {ex.Message}");
            }
        }
    }
}