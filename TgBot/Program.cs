using System;
using System.Collections.Generic;

namespace TelegramBot
{
   
    public enum ToDoItemState
    {
        Active,      
        Completed    
    }

    
    public class ToDoUser
    {
        
        public Guid UserId { get; }
      
        public string TelegramUserName { get; }

        public DateTime RegisteredAt { get; }

     
        public ToDoUser(string telegramUserName)
        {
            
            if (string.IsNullOrWhiteSpace(telegramUserName))
            {
                throw new ArgumentException("Имя пользователя не может быть пустым!");
            }

            UserId = Guid.NewGuid();

            TelegramUserName = telegramUserName;

            RegisteredAt = DateTime.UtcNow;
        }


        public override string ToString()
        {
            return $"Пользователь: {TelegramUserName} (ID: {UserId}, Зарегистрирован: {RegisteredAt:yyyy-MM-dd HH:mm:ss})";
        }
    }


    public class ToDoItem
    {

        public Guid Id { get; }

        public ToDoUser User { get; }

        public string Name { get; }

        public DateTime CreatedAt { get; }

        public ToDoItemState State { get; private set; } 

        public DateTime? StateChangedAt { get; private set; } 


        public ToDoItem(ToDoUser user, string name)
        {
            if (user == null)
            {
                throw new ArgumentException("Пользователь не может быть null!");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Название задачи не может быть пустым!");
            }

            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;

            State = ToDoItemState.Active;
            StateChangedAt = null; 
        }


        public void Complete()
        {
            if (State == ToDoItemState.Completed)
            {
                throw new InvalidOperationException("Задача уже выполнена!");
            }

            State = ToDoItemState.Completed;
            StateChangedAt = DateTime.UtcNow;
        }


        public string GetDisplayString(bool showState = false)
        {
            string statePrefix = showState ? $"({State}) " : "";
            return $"{statePrefix}{Name} - {CreatedAt:yyyy-MM-dd HH:mm:ss} - {Id}";
        }


        public override string ToString()
        {
            return GetDisplayString(true);
        }
    }

    class Program
    {
        
        static ToDoUser currentUser = null;          
        static List<ToDoItem> tasks = new List<ToDoItem>(); 


        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Добро пожаловать в Телеграм бот!");
                Console.WriteLine("Доступные команды:");
                Console.WriteLine("/start - начать работу с ботом");
                Console.WriteLine("/help - получить справку");
                Console.WriteLine("/info - информация о программе");
                Console.WriteLine("/echo [текст] - повторить ваш текст (доступно после /start)");
                Console.WriteLine("/addtask - добавить новую задачу");
                Console.WriteLine("/showtasks - показать активные задачи");
                Console.WriteLine("/showalltasks - показать все задачи");
                Console.WriteLine("/completetask [id] - отметить задачу как выполненную");
                Console.WriteLine("/removetask - удалить задачу по номеру");
                Console.WriteLine("/exit - выйти из программы");
                Console.WriteLine();

                bool isRunning = true;
                while (isRunning)
                {
                    Console.Write("Введите команду: ");
                    string input = Console.ReadLine().Trim();

                    try
                    {
                        if (input == "/start")
                        {
                            ProcessStartCommand();
                        }
                        else if (input == "/help")
                        {
                            ProcessHelpCommand();
                        }
                        else if (input == "/info")
                        {
                            ProcessInfoCommand();
                        }
                        else if (input == "/exit")
                        {
                            Console.WriteLine("До свидания!");
                            isRunning = false;
                        }
                        else if (input.StartsWith("/echo "))
                        {
                            ProcessEchoCommand(input);
                        }
                        else if (input == "/addtask")
                        {
                            ProcessAddTaskCommand();
                        }
                        else if (input == "/showtasks")
                        {
                            ProcessShowTasksCommand();
                        }
                        else if (input == "/showalltasks")
                        {
                            ProcessShowAllTasksCommand();
                        }
                        else if (input.StartsWith("/completetask "))
                        {
                            ProcessCompleteTaskCommand(input);
                        }
                        else if (input == "/removetask")
                        {
                            ProcessRemoveTaskCommand();
                        }
                        else
                        {
                            Console.WriteLine("Неизвестная команда. Используйте /help для списка команд.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        static void ProcessStartCommand()
        {
            Console.Write("Введите ваше имя: ");
            string name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Имя не может быть пустым!");
                return;
            }


            currentUser = new ToDoUser(name);

            Console.WriteLine($"Привет, {currentUser.TelegramUserName}!");
            Console.WriteLine($"Ваш ID: {currentUser.UserId}");
            Console.WriteLine($"Дата регистрации: {currentUser.RegisteredAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("Я ваш бот. Чем могу помочь?");
        }


        static void ProcessHelpCommand()
        {
            Console.WriteLine("Справка по командам:");
            Console.WriteLine("/start - начать работу и ввести имя");
            Console.WriteLine("/help - показать эту справку");
            Console.WriteLine("/info - показать информацию о программе");
            Console.WriteLine("/echo [текст] - повторить ваш текст");
            Console.WriteLine("/addtask - добавить новую задачу");
            Console.WriteLine("/showtasks - показать активные задачи");
            Console.WriteLine("/showalltasks - показать все задачи (включая выполненные)");
            Console.WriteLine("/completetask [id] - отметить задачу как выполненную");
            Console.WriteLine("/removetask - удалить задачу по номеру");
            Console.WriteLine("/exit - выйти из программы");
        }

        static void ProcessInfoCommand()
        {
            Console.WriteLine("Версия программы: 4.0.0");
            Console.WriteLine("Язык: C#");
            Console.WriteLine($"Всего задач: {tasks.Count}");

            int activeCount = tasks.FindAll(t => t.State == ToDoItemState.Active).Count;
            int completedCount = tasks.FindAll(t => t.State == ToDoItemState.Completed).Count;

            Console.WriteLine($"Активных задач: {activeCount}");
            Console.WriteLine($"Выполненных задач: {completedCount}");

            if (currentUser != null)
            {
                Console.WriteLine($"Текущий пользователь: {currentUser.TelegramUserName}");
                Console.WriteLine($"ID пользователя: {currentUser.UserId}");
            }
        }


        static void ProcessEchoCommand(string fullCommand)
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            string echoText = fullCommand.Substring(6);

            if (string.IsNullOrWhiteSpace(echoText))
            {
                Console.WriteLine($"{currentUser.TelegramUserName}, вы не ввели текст для повтора.");
            }
            else
            {
                Console.WriteLine($"{currentUser.TelegramUserName}, вы сказали: {echoText}");
            }
        }


        static void ProcessAddTaskCommand()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            Console.Write("Пожалуйста, введите описание задачи: ");
            string taskDescription = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                Console.WriteLine("Описание задачи не может быть пустым.");
                return;
            }

            ToDoItem newTask = new ToDoItem(currentUser, taskDescription);
            tasks.Add(newTask);

            Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
            Console.WriteLine($"ID задачи: {newTask.Id}");
            Console.WriteLine($"Дата создания: {newTask.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Всего задач: {tasks.Count}");
        }


        static void ProcessShowTasksCommand()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            List<ToDoItem> activeTasks = tasks.FindAll(t => t.State == ToDoItemState.Active);

            if (activeTasks.Count == 0)
            {
                Console.WriteLine("Активных задач нет.");
                return;
            }

            Console.WriteLine("Ваши активные задачи:");
            for (int i = 0; i < activeTasks.Count; i++)
            {

                Console.WriteLine($"{i + 1}. {activeTasks[i].GetDisplayString()}");
            }
        }


        static void ProcessShowAllTasksCommand()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            if (tasks.Count == 0)
            {
                Console.WriteLine("Задач нет.");
                return;
            }

            Console.WriteLine("Все задачи:");
            for (int i = 0; i < tasks.Count; i++)
            {

                Console.WriteLine($"{i + 1}. {tasks[i].GetDisplayString(showState: true)}");
            }
        }


        static void ProcessCompleteTaskCommand(string fullCommand)
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }


            string idString = fullCommand.Substring("/completetask ".Length).Trim();

            if (string.IsNullOrWhiteSpace(idString))
            {
                Console.WriteLine("Укажите ID задачи. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e");
                return;
            }


            if (!Guid.TryParse(idString, out Guid taskId))
            {
                Console.WriteLine("Ошибка! Некорректный формат ID. ID должен быть в формате GUID.");
                return;
            }


            ToDoItem taskToComplete = tasks.Find(t => t.Id == taskId);

            if (taskToComplete == null)
            {
                Console.WriteLine($"Задача с ID '{taskId}' не найдена.");
                return;
            }


            if (taskToComplete.State == ToDoItemState.Completed)
            {
                Console.WriteLine($"Задача '{taskToComplete.Name}' уже выполнена.");
                return;
            }


            taskToComplete.Complete();

            Console.WriteLine($"Задача '{taskToComplete.Name}' отмечена как выполненная!");
            Console.WriteLine($"Дата выполнения: {taskToComplete.StateChangedAt:yyyy-MM-dd HH:mm:ss}");
        }


        static void ProcessRemoveTaskCommand()
        {
            if (currentUser == null)
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            if (tasks.Count == 0)
            {
                Console.WriteLine("Список задач пуст. Нечего удалять.");
                return;
            }


            Console.WriteLine("Ваш список задач:");
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tasks[i].GetDisplayString(showState: true)}");
            }

            bool isValidNumber = false;
            do
            {
                Console.Write("Введите номер задачи для удаления: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int taskNumber))
                {
                    if (taskNumber >= 1 && taskNumber <= tasks.Count)
                    {
                        ToDoItem removedTask = tasks[taskNumber - 1];
                        tasks.RemoveAt(taskNumber - 1);
                        Console.WriteLine($"Задача \"{removedTask.Name}\" удалена.");
                        isValidNumber = true;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка! Введите число от 1 до {tasks.Count}.");
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка! Введите корректное число.");
                }
            } while (!isValidNumber);
        }
    }
}