using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class DeviceDetailPage : ContentPage
{
	public DeviceDetailPage(DeviceDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}