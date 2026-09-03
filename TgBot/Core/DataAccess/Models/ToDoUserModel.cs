using LinqToDB.Mapping;

namespace TgBot.Core.DataAccess.Models;

[Table("ToDoUser")]
public class ToDoUserModel
{
	[PrimaryKey]
	[Column("UserId")]
	public Guid UserId { get; set; }

	[Column("TelegramUserId")]
	public long TelegramUserId { get; set; }

	[Column("TelegramUserName")]
	public string TelegramUserName { get; set; } = null!;

	[Column("RegisteredAt")]
	public DateTime RegisteredAt { get; set; }
}