namespace SmartHomeMauiApp.Services;

public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    public async Task ShowAlertAsync(string title, string message, string cancel)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        return false;
    }
}
