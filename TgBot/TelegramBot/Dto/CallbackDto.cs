namespace TgBot.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; } = string.Empty;

        public static CallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');

            return new CallbackDto
            {
                Action = parts[0]
            };
        }

        public override string ToString()
        {
            return Action;
        }
    }
}