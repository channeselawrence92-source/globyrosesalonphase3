


using System.Text.Json;

namespace SalonManager.Services;

public class JsonStorageService : IStorageService
{
    private readonly string _path;
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public JsonStorageService(string path = "appstate.json")
    {
        _path = path;
    }

    public async Task<AppState> LoadAsync()
    {
        if (!File.Exists(_path)) return new AppState();
        using var fs = File.OpenRead(_path);
        var state = await JsonSerializer.DeserializeAsync<AppState>(fs, _opts);
        return state ?? new AppState();
    }

    public async Task SaveAsync(AppState state)
    {
        using var fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, state, _opts);
    }
}
