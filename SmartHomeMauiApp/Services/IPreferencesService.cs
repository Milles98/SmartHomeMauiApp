namespace SmartHomeMauiApp.Services;

public interface IPreferencesService
{
    void Set(string key, string value);
    string Get(string key, string defaultValue);
}
