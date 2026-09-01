using System;

namespace TgBot.Core.Exceptions
{
    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string taskName)
            : base($"Задача '{taskName}' уже существует")
        {
        }
    }
}