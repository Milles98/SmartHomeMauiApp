using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Models;
using System.Collections.ObjectModel;

namespace TodoApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
	[ObservableProperty]
	private TodoItem _todoItem = new();

	[ObservableProperty]
	private ObservableCollection<TodoItem> _todoItems = new();

	[RelayCommand]
	private void SaveTodo()
	{
		TodoItem.Id = Guid.NewGuid().ToString();
		TodoItem.Created = DateTime.Now;

		TodoItems.Add(TodoItem);
		TodoItem = new TodoItem();
	}
}
