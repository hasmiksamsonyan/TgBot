using System;
using System.Collections.Generic;

namespace TelegramBot
{
  
    class Program
    {
    
        static string userName = null;
        static List<string> tasks = new List<string>();


        static int maxTaskCount = 0;      
        static int maxTaskLength = 0;     
        static void Main(string[] args)
        {
            try
            {
               
                Console.WriteLine("НАСТРОЙКА БОТА");

                
                Console.Write("Введите максимально допустимое количество задач (1-100): ");
                string maxCountInput = Console.ReadLine();
                maxTaskCount = ParseAndValidateInt(maxCountInput, 1, 100);

                
                Console.Write("Введите максимально допустимую длину задачи (1-100): ");
                string maxLengthInput = Console.ReadLine();
                maxTaskLength = ParseAndValidateInt(maxLengthInput, 1, 100);

                Console.WriteLine($"Настройки применены: макс. задач = {maxTaskCount}, макс. длина = {maxTaskLength}");
                Console.WriteLine();

               
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
                        else if (input == "/removetask")
                        {
                            ProcessRemoveTaskCommand();
                        }
                        else
                        {
                            Console.WriteLine("Неизвестная команда. Используйте /help для списка команд.");
                        }
                    }
                    
                    catch (TaskCountLimitException ex)
                    {
                        
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    catch (TaskLengthLimitException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    catch (DuplicateTaskException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    catch (ArgumentException ex)
                    {
                       
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine("Произошла непредвиденная ошибка:");
                        Console.WriteLine($"Тип: {ex.GetType()}");
                        Console.WriteLine($"Сообщение: {ex.Message}");
                        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                        }
                    }

                    Console.WriteLine();
                }
            }
           
            catch (ArgumentException ex)
            {
               
                Console.WriteLine($"Ошибка настройки: {ex.Message}");
                Console.WriteLine("Перезапустите программу и введите корректные значения.");
            }
            catch (Exception ex)
            {
               
                Console.WriteLine("Произошла непредвиденная ошибка при запуске:");
                Console.WriteLine($"Тип: {ex.GetType()}");
                Console.WriteLine($"Сообщение: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                }
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
            userName = Console.ReadLine();

            
            ValidateString(userName);

            Console.WriteLine($"Привет, {userName}! Я ваш бот. Чем могу помочь?");
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
            Console.WriteLine();
            Console.WriteLine($"Текущие ограничения:");
            Console.WriteLine($"- Максимальное количество задач: {maxTaskCount}");
            Console.WriteLine($"- Максимальная длина задачи: {maxTaskLength}");
        }

      
        static void ProcessInfoCommand()
        {
            Console.WriteLine("Версия программы: 3.0.0");
            Console.WriteLine("Язык: C#");
            Console.WriteLine($"Количество задач в списке: {tasks.Count}");
            Console.WriteLine($"Максимальное количество задач: {maxTaskCount}");
            Console.WriteLine($"Максимальная длина задачи: {maxTaskLength}");
        }

       
        static void ProcessEchoCommand(string fullCommand)
        {
            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("Сначала используйте команду /start, чтобы представиться!");
                return;
            }

            string echoText = fullCommand.Substring(6);

           
            ValidateString(echoText);

            Console.WriteLine($"{userName}, вы сказали: {echoText}");
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

            
            ValidateString(taskDescription);

          
            if (taskDescription.Length > maxTaskLength)
            {
                throw new TaskLengthLimitException(taskDescription.Length, maxTaskLength);
            }

           
            if (tasks.Contains(taskDescription))
            {
                throw new DuplicateTaskException(taskDescription);
            }

           
            if (tasks.Count >= maxTaskCount)
            {
                throw new TaskCountLimitException(maxTaskCount);
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

      
        static int ParseAndValidateInt(string str, int min, int max)
        {
          
            ValidateString(str);

           
            if (!int.TryParse(str, out int result))
            {
                throw new ArgumentException($"Ошибка! '{str}' не является числом.");
            }

          
            if (result < min || result > max)
            {
                throw new ArgumentException($"Ошибка! Число должно быть от {min} до {max}. Вы ввели {result}.");
            }

            return result;
        }

       
        static void ValidateString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Ошибка! Строка не может быть пустой или состоять только из пробелов.");
            }
        }
    }

    
    public class TaskCountLimitException : Exception
    {
       
        public TaskCountLimitException(int taskCountLimit)
            : base($"Превышено максимальное количество задач равное {taskCountLimit}")
        {
        }
    }

  
    public class TaskLengthLimitException : Exception
    {
       
        public TaskLengthLimitException(int taskLength, int taskLengthLimit)
            : base($"Длина задачи '{taskLength}' превышает максимально допустимое значение {taskLengthLimit}")
        {
        }
    }

  
    public class DuplicateTaskException : Exception
    {
       
        public DuplicateTaskException(string task)
            : base($"Задача '{task}' уже существует")
        {
        }
    }
}