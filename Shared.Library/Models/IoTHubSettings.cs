using SQLite;

namespace Shared.Library.Models;

public class IoTHubSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string ConnectionString { get; set; } = null!;
}
