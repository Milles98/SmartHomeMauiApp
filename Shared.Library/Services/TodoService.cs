using Shared.Library.Models;

namespace Shared.Library.Services;

public class TodoService
{
	private readonly List<TodoItem> _todos = [];

	public IEnumerable<TodoItem> GetTodos() => _todos;

	public bool AddToList(TodoItem todoItem)
	{
		try
		{
			if (!string.IsNullOrEmpty(todoItem.Activity))
			{
				todoItem.Id = Guid.NewGuid().ToString();
				todoItem.Created = DateTime.Now;

				_todos.Add(todoItem);
				return true;
			}
		}
		catch { }
		return false;
	}

	public bool RemoveFromList(string id)
	{
		try
		{
			var todoItem = _todos.FirstOrDefault(x => x.Id == id);
			if (todoItem != null)
			{
				_todos.Remove(todoItem);
				return true;
			}
		}
		catch { }
		return false;
	}
}
