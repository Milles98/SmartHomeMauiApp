using SQLite;

namespace Shared.Library.Models;

public class UserSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string? EmailAddress { get; set; }
}
