using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class DeviceDetailPage : ContentPage
{
    public DeviceDetailPage(DeviceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DeviceDetailViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is DeviceDetailViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}