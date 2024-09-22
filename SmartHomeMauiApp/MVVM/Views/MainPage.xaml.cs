using SmartHomeMauiApp.MVVM.ViewModels;

namespace SmartHomeMauiApp.MVVM.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
