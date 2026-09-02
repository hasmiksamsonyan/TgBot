using System;

namespace TgBot.Dto
{
    public class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');

            Guid itemId = Guid.Empty;

            if (parts.Length > 1 &&
                Guid.TryParse(parts[1], out Guid parsedId))
            {
                itemId = parsedId;
            }

            return new ToDoItemCallbackDto
            {
                Action = parts[0],
                ToDoItemId = itemId
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }
    }
}