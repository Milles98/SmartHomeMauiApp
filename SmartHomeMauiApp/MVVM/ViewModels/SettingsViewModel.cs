using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartHomeMauiApp.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
	[ObservableProperty]
	private string _connectionString;

	[ObservableProperty]
	private string _emailAddress;

	public SettingsViewModel()
	{

	}

	[RelayCommand]
	private async Task SaveSettingsAsync()
	{
		//Spara inställningarna
	}
}
