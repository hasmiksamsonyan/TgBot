using LinqToDB.Mapping;

namespace TgBot.Core.DataAccess.Models;

[Table("Notification")]
public class NotificationModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("Type")]
    public string Type { get; set; } = null!;

    [Column("Text")]
    public string Text { get; set; } = null!;

    [Column("ScheduledAt")]
    public DateTime ScheduledAt { get; set; }

    [Column("IsNotified")]
    public bool IsNotified { get; set; }

    [Column("NotifiedAt")]
    public DateTime? NotifiedAt { get; set; }

    [Association(
        ThisKey = nameof(UserId),
        OtherKey = nameof(ToDoUserModel.UserId),
        CanBeNull = false)]
    public ToDoUserModel User { get; set; } = null!;
}