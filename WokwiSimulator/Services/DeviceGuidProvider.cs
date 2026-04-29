using System.Text.Json;
using WokwiSimulator.Configuration;

namespace WokwiSimulator.Services;

public class DeviceGuidProvider : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly AppConfig _config;
    private DateTime _lastRefreshUtc = DateTime.MinValue;
    private List<string> _cachedGuids = new();

    public DeviceGuidProvider(AppConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public async Task<IReadOnlyList<string>> GetDeviceGuidsAsync()
    {
        if (string.IsNullOrWhiteSpace(_config.DevicesApiUrl))
        {
            return _cachedGuids;
        }

        var elapsed = DateTime.UtcNow - _lastRefreshUtc;
        if (_cachedGuids.Count > 0 && elapsed.TotalSeconds < _config.DevicesRefreshSeconds)
        {
            return _cachedGuids;
        }

        try
        {
            var response = await _httpClient.GetAsync(_config.DevicesApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return _cachedGuids;
            }

            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<DeviceGuidApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var guids = parsed?.Data?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            _cachedGuids = guids;
            _lastRefreshUtc = DateTime.UtcNow;
            return _cachedGuids;
        }
        catch
        {
            return _cachedGuids;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private class DeviceGuidApiResponse
    {
        public int StatusCode { get; set; }
        public List<string>? Data { get; set; }
    }
}
