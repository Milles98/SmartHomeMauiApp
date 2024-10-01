namespace SmartHomeMauiApp.Services;

public class PreferencesService : IPreferencesService
{
    public void Set(string key, string value)
    {
        Preferences.Set(key, value);
    }

    public string Get(string key, string defaultValue)
    {
        return Preferences.Get(key, defaultValue);
    }
}
