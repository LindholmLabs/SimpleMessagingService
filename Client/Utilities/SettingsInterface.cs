using System.Diagnostics;

namespace Client.Utilities;

public interface ISettingsInterface
{
    Task<T?> LoadSettingsAsync<T>();
    Task SaveSettingsAsync<T>(T settings);
}

public class SettingsInterface : ISettingsInterface
{
    private readonly string _settingsPath = "settings.json";

    public async Task<T?> LoadSettingsAsync<T>()
    {
        try
        {
            var json = await System.IO.File.ReadAllTextAsync(_settingsPath);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return default;
        }
    }

    public async Task SaveSettingsAsync<T>(T settings)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            await System.IO.File.WriteAllTextAsync(_settingsPath, json);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
}

public class Settings
{
    public List<Server> Servers { get; set; } = new List<Server>();
    public Guid? SelectedServer { get; set; }
    public int PollingInterval { get; set; } = 1000;
}

public class Server
{
    public Guid serverId { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? User { get; set; }
    public Guid? Key { get; set; }
    public Uri? Url { get; set; }
}