using System;

namespace TgBot.Core.Exceptions
{
    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int limit)
            : base($"Превышено максимальное количество задач равное {limit}")
        {
        }
    }
}