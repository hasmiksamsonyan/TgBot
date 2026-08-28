using System;
using Otus.ToDoList.ConsoleBot.Types;

namespace Otus.ToDoList.ConsoleBot
{
    public class ConsoleBotClient : ITelegramBotClient
    {
        private readonly Chat _chat;
        private readonly User _user;

        public ConsoleBotClient()
        {
            _chat = new Chat { Id = Random.Shared.Next() };
            _user = new User { Id = Random.Shared.Next(), Username = $"ConsoleUser_{Guid.NewGuid()}" };
        }

        public void SendMessage(Chat chat, string text)
        {
            ArgumentNullException.ThrowIfNull(chat, nameof(chat));
            ArgumentNullException.ThrowIfNull(text, nameof(text));
            if (_chat.Id != chat.Id)
                throw new ArgumentException($"Invalid chat.Id. Support {_chat.Id}, but was {chat.Id}");

            WriteLineColor($"Бот: {text}", ConsoleColor.Blue);
        }

        public void StartReceiving(IUpdateHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler, nameof(handler));

            try
            {
                WriteLineColor("Бот запущен. Введите сообщение", ConsoleColor.Magenta);
                var counter = 0;

                while (true)
                {
                    var input = Console.ReadLine();
                    if (input is null)
                        break;

                    var update = new Update
                    {
                        Message = new Message
                        {
                            Id = Interlocked.Increment(ref counter),
                            Text = input,
                            Chat = _chat,
                            From = _user
                        }
                    };

                    try
                    {
                        handler.HandleUpdateAsync(this, update);
                    }
                    catch (Exception ex)
                    {
                        handler.HandleErrorAsync(this, ex);
                    }
                }
            }
            finally
            {
                WriteLineColor("Бот остановлен", ConsoleColor.Magenta);
            }
        }

        private static void WriteLineColor(string text, ConsoleColor color)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = currentColor;
        }
    }
}