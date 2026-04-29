using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WokwiSimulator.Configuration;
using WokwiSimulator.Models;
using WokwiSimulator.Services;

namespace WokwiSimulator.Services;

public class IncidentSender
{
    private readonly HttpClient _httpClient;
    private readonly AppConfig _config;

    public IncidentSender(AppConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        
        // JWT токен опціональний для WOKWI ендпоинту
        // Якщо вказано, додаємо його до заголовків
        if (!string.IsNullOrEmpty(_config.JwtToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _config.JwtToken);
        }
    }

    public async Task<bool> SendIncidentAsync(GeneratedEvent generatedEvent, string deviceGuid)
    {
        try
        {
            var request = new IncidentRequest
            {
                DeviceGuid = deviceGuid,
                IncidentType = generatedEvent.EventType,
                Severity = generatedEvent.Severity,
                Description = generatedEvent.Description,
                Timestamp = DateTime.UtcNow,
                Status = "Active"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Відправка події:");
            Console.WriteLine($"  Device GUID: {deviceGuid}");
            Console.WriteLine($"  Тип: {generatedEvent.EventType}");
            Console.WriteLine($"  Серйозність: {generatedEvent.Severity}");
            Console.WriteLine($"  Опис: {generatedEvent.Description}");
            Console.WriteLine($"  JSON: {json}");
            Console.WriteLine();

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_config.ServerUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✓ Подію успішно відправлено! Код відповіді: {(int)response.StatusCode}");
                if (!string.IsNullOrEmpty(responseBody))
                {
                    Console.WriteLine($"  Відповідь: {responseBody}");
                }
                Console.WriteLine("----------------------------------------");
                return true;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✗ Помилка відправки події. Код: {(int)response.StatusCode}");
                Console.WriteLine($"  Помилка: {errorBody}");
                Console.WriteLine("----------------------------------------");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Виняток при відправці: {ex.Message}");
            Console.WriteLine($"  Деталі: {ex}");
            Console.WriteLine("----------------------------------------");
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

