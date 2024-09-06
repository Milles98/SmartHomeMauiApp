using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Library.Models;
using Shared.Library.Services;
using System.Collections.ObjectModel;

namespace TodoApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
	private readonly TodoService _todoService;

	[ObservableProperty]
	private TodoItem _todoItem = new();

	[ObservableProperty]
	private ObservableCollection<TodoItem> _todoItems = new();

	[RelayCommand]
	private void SaveTodo()
	{
		var result = _todoService.AddToList(TodoItem);
		if (result)
		{
			TodoItem = new TodoItem();
		}
	}

	[RelayCommand]
	private void RemoveTodoItem(TodoItem todoItem)
	{
		var result = _todoService.RemoveFromList(todoItem.Id);
		if (result)
		{

		}
	}

	public MainViewModel(TodoService todoService)
	{
		_todoService = todoService;
	}
}
