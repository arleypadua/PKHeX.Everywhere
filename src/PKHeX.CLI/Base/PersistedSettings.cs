using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace PKHeX.CLI.Base;

public class PersistedSettings(string? lastSaveFilePath = null)
{
    public string? LastSaveFilePath
    {
        get => lastSaveFilePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                lastSaveFilePath = null;
                return;
            }
            
            var fullPath = Path.GetFullPath(value);
            lastSaveFilePath = fullPath;
        }
    }
    
    public void Save()
    {
        var serialized = JsonSerializer.Serialize(this, PersistedSettingsContext.Default.PersistedSettings);
        File.WriteAllText(SettingsFilePath, serialized);
    }

    public static PersistedSettings Load()
    {
        var settingsFileExists = File.Exists(SettingsFilePath);
        return settingsFileExists
            ? SafeLoad()
            : new PersistedSettings();
    }

    private static PersistedSettings SafeLoad()
    {
        try
        {
            return JsonSerializer.Deserialize(File.ReadAllText(SettingsFilePath),
                       PersistedSettingsContext.Default.PersistedSettings)
                   ?? new PersistedSettings();
        }
        catch (Exception e)
        {
            AnsiConsole.WriteLine($"[red]Error loading settings: {e.Message}[/]");
        }
        
        return new PersistedSettings();
    }

    private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PersistedSettings))]
[JsonSerializable(typeof(string))]
public partial class PersistedSettingsContext : JsonSerializerContext
{
}