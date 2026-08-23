using System;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace TelegramBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public UpdateHandler(IUserService userService, IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
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
                {
                    botClient.SendMessage(chat,
                        "/help - справка\n" +
                        "/info - информация\n" +
                        "/addtask [название] - добавить задачу\n" +
                        "/showtasks - активные задачи\n" +
                        "/showalltasks - все задачи\n" +
                        "/completetask [id] - выполнить задачу\n" +
                        "/removetask [номер] - удалить задачу\n" +
                        "/exit - выход");
                }
                else if (text == "/info")
                {
                    var all = _todoService.GetAllByUserId(user.UserId);
                    var active = _todoService.GetActiveByUserId(user.UserId);
                    botClient.SendMessage(chat,
                        $"Пользователь: {user.TelegramUserName}\n" +
                        $"Всего задач: {all.Count}\n" +
                        $"Активных: {active.Count}\n" +
                        $"Выполненных: {all.Count - active.Count}");
                }
                else if (text == "/showtasks")
                {
                    var tasks = _todoService.GetActiveByUserId(user.UserId);
                    if (tasks.Count == 0)
                    {
                        botClient.SendMessage(chat, "Активных задач нет");
                        return;
                    }
                    string msg = "Активные задачи:\n";
                    for (int i = 0; i < tasks.Count; i++)
                        msg += $"{i + 1}. {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";
                    botClient.SendMessage(chat, msg);
                }
                else if (text == "/showalltasks")
                {
                    var tasks = _todoService.GetAllByUserId(user.UserId);
                    if (tasks.Count == 0)
                    {
                        botClient.SendMessage(chat, "Задач нет");
                        return;
                    }
                    string msg = "Все задачи:\n";
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        string state = tasks[i].State == ToDoItemState.Active ? "Активная" : "Выполнена";
                        msg += $"{i + 1}. ({state}) {tasks[i].Name} - {tasks[i].CreatedAt:dd.MM.yyyy HH:mm} - {tasks[i].Id}\n";
                    }
                    botClient.SendMessage(chat, msg);
                }
                else if (text.StartsWith("/addtask"))
                {
                    string name = text.Substring("/addtask".Length).Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        botClient.SendMessage(chat, "Укажите название: /addtask Купить продукты");
                        return;
                    }
                    try
                    {
                        var task = _todoService.Add(user, name);
                        botClient.SendMessage(chat, $"Задача '{name}' добавлена. ID: {task.Id}");
                    }
                    catch (Exception ex)
                    {
                        botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
                    }
                }
                else if (text.StartsWith("/completetask "))
                {
                    string idStr = text.Substring("/completetask".Length).Trim();
                    if (!Guid.TryParse(idStr, out Guid id))
                    {
                        botClient.SendMessage(chat, "Укажите корректный ID задачи");
                        return;
                    }
                    try
                    {
                        _todoService.MarkCompleted(id);
                        botClient.SendMessage(chat, $"Задача {id} выполнена!");
                    }
                    catch (Exception ex)
                    {
                        botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
                    }
                }
                else if (text.StartsWith("/removetask "))
                {
                    string numStr = text.Substring("/removetask".Length).Trim();
                    if (!int.TryParse(numStr, out int number) || number < 1)
                    {
                        botClient.SendMessage(chat, "Укажите корректный номер задачи");
                        return;
                    }
                    try
                    {
                        var tasks = _todoService.GetAllByUserId(user.UserId);
                        if (number > tasks.Count)
                        {
                            botClient.SendMessage(chat, $"Номер от 1 до {tasks.Count}");
                            return;
                        }
                        var task = tasks[number - 1];
                        _todoService.Delete(task.Id);
                        botClient.SendMessage(chat, $"Задача '{task.Name}' удалена");
                    }
                    catch (Exception ex)
                    {
                        botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
                    }
                }
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
    }
}