using LinqToDB.Mapping;

namespace TgBot.Core.DataAccess.Models;

[Table("ToDoList")]
public class ToDoListModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("Name")]
    public string Name { get; set; } = null!;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Association(
        ThisKey = nameof(UserId),
        OtherKey = nameof(ToDoUserModel.UserId),
        CanBeNull = false)]
    public ToDoUserModel User { get; set; } = null!;
}