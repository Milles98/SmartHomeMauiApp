using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class AddDevicePage : ContentPage
{
    public AddDevicePage(AddDeviceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AddDeviceViewModel viewModel)
        {
            viewModel.ResponseMessage = string.Empty;
            viewModel.ResponseMessageColor = "Red";
        }
    }

}