using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SettingsViewModel viewModel)
        {
            viewModel.ResponseMessage = string.Empty;
            viewModel.ResponseMessageColor = "Red";
            await viewModel.LoadSettingsAsync();
        }
    }
}