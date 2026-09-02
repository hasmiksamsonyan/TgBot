using System;

namespace TgBot.Dto
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public static new PagedListCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');

            Guid? listId = null;

            if (parts.Length > 1 &&
                Guid.TryParse(parts[1], out Guid parsedId))
            {
                listId = parsedId;
            }

            int page = 0;

            if (parts.Length > 2 &&
                int.TryParse(parts[2], out int parsedPage))
            {
                page = parsedPage;
            }

            return new PagedListCallbackDto
            {
                Action = parts[0],
                ToDoListId = listId,
                Page = page
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}