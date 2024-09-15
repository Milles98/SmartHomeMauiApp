using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class AddDevicePage : ContentPage
{
    public AddDevicePage(AddDeviceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}