using WokwiSimulator.Models;

namespace WokwiSimulator.Services;

public class EventGenerator
{
    private static readonly string[] EventTypes = 
    {
        "MotionSensor",
        "AlarmButton",
        "AccessControl"
    };

    private static readonly string[] MotionDescriptions = 
    {
        "Рух виявлено в забороненій зоні",
        "Підозріла активність виявлена",
        "Несанкціонований доступ до приміщення"
    };

    private readonly Random _random = new();

    public GeneratedEvent GenerateRandomEvent()
    {
        var eventType = EventTypes[_random.Next(EventTypes.Length)];
        string severity;
        string description;

        switch (eventType)
        {
            case "AlarmButton":
                severity = "Critical";
                description = "Натиснуто кнопку тривоги!";
                break;

            case "AccessControl":
                severity = "Low";
                var cardUID = GenerateRandomCardUID();
                description = $"Спроба доступу з карткою: {cardUID}";
                break;

            case "MotionSensor":
            default:
                severity = _random.Next(2) == 0 ? "Medium" : "High";
                description = MotionDescriptions[_random.Next(MotionDescriptions.Length)];
                break;
        }

        return new GeneratedEvent
        {
            EventType = eventType,
            Severity = severity,
            Description = description
        };
    }

    private string GenerateRandomCardUID()
    {
        const string hexChars = "0123456789ABCDEF";
        var uid = new char[8];
        for (int i = 0; i < 8; i++)
        {
            uid[i] = hexChars[_random.Next(hexChars.Length)];
        }
        return new string(uid);
    }
}

public class GeneratedEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

