using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}