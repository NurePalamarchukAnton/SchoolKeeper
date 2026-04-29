namespace WokwiSimulator.Configuration;

public class AppConfig
{
    public string ServerUrl { get; set; } = "http://localhost:8080/api/Incident/wokwi";
    public string DevicesApiUrl { get; set; } = "http://localhost:8080/api/Incident/wokwi/devices";
    public int DevicesRefreshSeconds { get; set; } = 30;
    public string JwtToken { get; set; } = string.Empty; // Не обов'язково для WOKWI ендпоинту
    public List<string> DeviceGuids { get; set; } = new();
    public int MinDelaySeconds { get; set; } = 5;
    public int MaxDelaySeconds { get; set; } = 30;
}

