namespace Shared.Library.Models;

public class TodoItem
{
	public string Id { get; set; } = null!;
	public DateTime Created { get; set; }
	public string Activity { get; set; } = null!;
}
