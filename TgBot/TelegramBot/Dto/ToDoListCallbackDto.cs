using System;

namespace TgBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public static new ToDoListCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');

            Guid? listId = null;

            if (parts.Length > 1 &&
                Guid.TryParse(parts[1], out Guid parsedId))
            {
                listId = parsedId;
            }

            return new ToDoListCallbackDto
            {
                Action = parts[0],
                ToDoListId = listId
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}