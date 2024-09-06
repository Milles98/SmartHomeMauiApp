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
		TodoItems.Clear();
		TodoItems.Add(TodoItem);
	}
}
