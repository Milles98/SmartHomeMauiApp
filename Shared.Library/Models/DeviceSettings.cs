using SQLite;

namespace Shared.Library.Models;

public class DeviceSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string? DeviceId { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public bool DeviceState { get; set; }
    public bool IsConnected { get; set; }
    public string? LastActivityTime { get; set; }
}
