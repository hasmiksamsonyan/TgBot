using System;
using System.Collections.Generic;

namespace TelegramBot
{
    class Program
    {
        static string userName = null;
        static List<string> tasks = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в Телеграм бот!");
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("/start - начать работу с ботом");
            Console.WriteLine("/help - получить справку");
            Console.WriteLine("/info - информация о программе");
            Console.WriteLine("/echo [текст] - повторить ваш текст (доступно после /start)");
            Console.WriteLine("/addtask - добавить новую задачу");
            Console.WriteLine("/showtasks - показать все задачи");
            Console.WriteLine("/removetask - удалить задачу по номеру");
            Console.WriteLine("/exit - выйти из программы");

            bool isRunning = true;
            while (isRunning)
            {
                Console.Write("Введите команду: ");
                string input = Console.ReadLine().Trim();

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
                else if (input == "/removetask")
                {
                    ProcessRemoveTaskCommand();
                }
                else
                {
                    Console.WriteLine("Неизвестная команда. Используйте /help для списка команд.");
                }
            }
        }

        static void ProcessStartCommand()
        {
            Console.Write("Введите ваше имя: ");
            userName = Console.ReadLine();

            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("Вы не ввели имя. Попробуйте еще раз.");
                userName = null;
            }
            else
            {
                Console.WriteLine($"Привет, {userName}! Я ваш бот. Чем могу помочь?");
            }
        }

        static void ProcessHelpCommand()
        {
            Console.WriteLine("Справка по командам:");
            Console.WriteLine("/start - начать работу и ввести имя");
            Console.WriteLine("/help - показать эту справку");
            Console.WriteLine("/info - показать информацию о программе");
            Console.WriteLine("/echo [текст] - повторить ваш текст (доступно после /start)");
            Console.WriteLine("/addtask - добавить новую задачу в список");
            Console.WriteLine("/showtasks - показать все добавленные задачи");
            Console.WriteLine("/removetask - удалить задачу по номеру");
            Console.WriteLine("/exit - выйти из программы");
        }

        static void ProcessInfoCommand()
        {
            Console.WriteLine("Версия программы: 2.0.0");
            Console.WriteLine("Язык: C#");
            Console.WriteLine($"Количество задач в списке: {tasks.Count}");
        }

        static void ProcessEchoCommand(string fullCommand)
        {
            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            string echoText = fullCommand.Substring(6);

            if (string.IsNullOrWhiteSpace(echoText))
            {
                Console.WriteLine($"{userName}, вы не ввели текст для повтора. Используйте: /echo [текст]");
            }
            else
            {
                Console.WriteLine($"{userName}, вы сказали: {echoText}");
            }
        }

        static void ProcessAddTaskCommand()
        {
            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            Console.Write("Пожалуйста, введите описание задачи: ");
            string taskDescription = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                Console.WriteLine("Описание задачи не может быть пустым. Попробуйте снова.");
                return;
            }

            tasks.Add(taskDescription);
            Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
            Console.WriteLine($"Всего задач: {tasks.Count}");
        }

        static bool ProcessShowTasksCommand()
        {
            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return false;
            }

            if (tasks.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return false;
            }

            Console.WriteLine("Ваш список задач:");
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tasks[i]}");
            }

            return true;
        }

        static void ProcessRemoveTaskCommand()
        {
            if (!ProcessShowTasksCommand())
            {
                return;
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
                        string removedTask = tasks[taskNumber - 1];
                        tasks.RemoveAt(taskNumber - 1);
                        Console.WriteLine($"Задача \"{removedTask}\" удалена.");
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