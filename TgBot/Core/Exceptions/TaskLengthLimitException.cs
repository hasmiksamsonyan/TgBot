using System;

namespace TgBot.Core.Exceptions
{
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int length, int limit)
            : base($"Длина задачи '{length}' превышает максимально допустимое значение {limit}")
        {
        }
    }
}