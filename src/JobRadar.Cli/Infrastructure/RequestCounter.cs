using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JobRadar.Cli.Infrastructure;

/// <summary>
/// Persists API request counts keyed by a SHA-256 hash of the API key.
/// The raw key is never stored or logged.
/// </summary>
public sealed class RequestCounter
{
    private readonly string _storePath;
    private readonly int _limit;

    public RequestCounter(string storePath, int limit)
    {
        _storePath = storePath;
        _limit = limit;
        Directory.CreateDirectory(Path.GetDirectoryName(storePath)!);
    }

    public int GetCount(string apiKey)
    {
        var hash = Hash(apiKey);
        var data = Load();
        return data.TryGetValue(hash, out var count) ? count : 0;
    }

    public int Increment(string apiKey)
    {
        var hash = Hash(apiKey);
        var data = Load();
        var newCount = (data.TryGetValue(hash, out var c) ? c : 0) + 1;
        data[hash] = newCount;
        Save(data);
        return newCount;
    }

    public bool IsLimitReached(string apiKey) => GetCount(apiKey) >= _limit;

    public bool IsNearLimit(string apiKey, double thresholdPercent = 0.9) =>
        GetCount(apiKey) >= _limit * thresholdPercent;

    private Dictionary<string, int> Load()
    {
        if (!File.Exists(_storePath))
            return [];

        try
        {
            var json = File.ReadAllText(_storePath);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void Save(Dictionary<string, int> data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
        File.WriteAllText(_storePath, json);
    }

    private static string Hash(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes)[..16]; // first 16 hex chars is enough for a key
    }
}
