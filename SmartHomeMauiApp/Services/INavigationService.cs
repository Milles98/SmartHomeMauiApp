namespace SmartHomeMauiApp.Services;

public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task ShowAlertAsync(string title, string message, string cancel);
    Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);
}

