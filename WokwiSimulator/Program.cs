using WokwiSimulator.Configuration;
using WokwiSimulator.Services;

namespace WokwiSimulator;

class Program
{
    private static AppConfig _config = new();
    private static EventGenerator? _eventGenerator;
    private static IncidentSender? _incidentSender;
    private static DeviceGuidProvider? _deviceGuidProvider;
    private static readonly List<string> _configuredDeviceGuids = new();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        Console.WriteLine("========================================");
        Console.WriteLine("SchoolKeeper WOKWI Simulator");
        Console.WriteLine("Імітатор IoT пристрою для тестування");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Завантаження конфігурації
        LoadConfiguration(args);

        Console.WriteLine("Конфігурація:");
        Console.WriteLine($"  URL сервера: {_config.ServerUrl}");
        Console.WriteLine($"  URL девайсів: {_config.DevicesApiUrl}");
        Console.WriteLine($"  Device GUIDs: {string.Join(", ", _configuredDeviceGuids)}");
        Console.WriteLine($"  Оновлення списку девайсів: {_config.DevicesRefreshSeconds} с");
        Console.WriteLine($"  Інтервал: {_config.MinDelaySeconds}-{_config.MaxDelaySeconds} секунд");
        Console.WriteLine($"  JWT токен: {(_config.JwtToken.Length > 0 ? "*** (опціонально)" : "НЕ ВИКОРИСТОВУЄТЬСЯ (WOKWI ендпоинт без авторизації)")}");
        Console.WriteLine();

        // Ініціалізація сервісів
        _eventGenerator = new EventGenerator();
        _incidentSender = new IncidentSender(_config);
        _deviceGuidProvider = new DeviceGuidProvider(_config);

        // Генерація першого інтервалу
        var random = new Random();
        var nextDelay = random.Next(_config.MinDelaySeconds, _config.MaxDelaySeconds + 1);
        var lastEventTime = DateTime.Now;

        Console.WriteLine("Система готова!");
        Console.WriteLine($"Перша подія через: {nextDelay} секунд");
        Console.WriteLine();
        Console.WriteLine("Натисніть Ctrl+C для зупинки");
        Console.WriteLine();

        // Основний цикл
        try
        {
            while (true)
            {
                var elapsed = (DateTime.Now - lastEventTime).TotalSeconds;

                if (elapsed >= nextDelay)
                {
                    // Генерація та відправка події
                    var generatedEvent = _eventGenerator.GenerateRandomEvent();
                    var randomDeviceGuid = await SelectRandomDeviceGuidAsync(random);
                    await _incidentSender.SendIncidentAsync(generatedEvent, randomDeviceGuid);

                    // Генерація нового інтервалу
                    nextDelay = random.Next(_config.MinDelaySeconds, _config.MaxDelaySeconds + 1);
                    lastEventTime = DateTime.Now;

                    Console.WriteLine($"Наступна подія через: {nextDelay} секунд");
                    Console.WriteLine();
                }

                await Task.Delay(1000); // Перевірка кожну секунду
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критична помилка: {ex.Message}");
        }
        finally
        {
            _deviceGuidProvider?.Dispose();
            _incidentSender?.Dispose();
        }
    }

    static void LoadConfiguration(string[] args)
    {
        _configuredDeviceGuids.Clear();

        // 1) Завантаження з env (базова конфігурація для docker-compose)
        var envUrl = Environment.GetEnvironmentVariable("WOKWI_SERVER_URL");
        if (!string.IsNullOrWhiteSpace(envUrl))
        {
            _config.ServerUrl = envUrl;
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WOKWI_DEVICES_API_URL")))
            {
                _config.DevicesApiUrl = DeriveDevicesApiUrl(envUrl);
            }
        }

        var envDevicesApiUrl = Environment.GetEnvironmentVariable("WOKWI_DEVICES_API_URL");
        if (!string.IsNullOrWhiteSpace(envDevicesApiUrl))
            _config.DevicesApiUrl = envDevicesApiUrl;

        var envToken = Environment.GetEnvironmentVariable("WOKWI_JWT_TOKEN");
        if (!string.IsNullOrWhiteSpace(envToken))
            _config.JwtToken = envToken;

        var envDeviceGuids = Environment.GetEnvironmentVariable("WOKWI_DEVICE_GUIDS");
        if (!string.IsNullOrWhiteSpace(envDeviceGuids))
            AddDeviceGuids(envDeviceGuids);

        var envMinDelay = Environment.GetEnvironmentVariable("WOKWI_MIN_DELAY_SECONDS");
        if (int.TryParse(envMinDelay, out int parsedMinDelay))
            _config.MinDelaySeconds = parsedMinDelay;

        var envMaxDelay = Environment.GetEnvironmentVariable("WOKWI_MAX_DELAY_SECONDS");
        if (int.TryParse(envMaxDelay, out int parsedMaxDelay))
            _config.MaxDelaySeconds = parsedMaxDelay;

        var envDevicesRefresh = Environment.GetEnvironmentVariable("WOKWI_DEVICES_REFRESH_SECONDS");
        if (int.TryParse(envDevicesRefresh, out int parsedRefreshSeconds))
            _config.DevicesRefreshSeconds = parsedRefreshSeconds;

        // 2) CLI аргументи перекривають env
        // Парсинг аргументів командного рядка
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--url":
                case "-u":
                    if (i + 1 < args.Length)
                    {
                        _config.ServerUrl = args[++i];
                        _config.DevicesApiUrl = DeriveDevicesApiUrl(_config.ServerUrl);
                    }
                    break;

                case "--devices-url":
                    if (i + 1 < args.Length)
                        _config.DevicesApiUrl = args[++i];
                    break;

                case "--devices-refresh":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int refreshSeconds))
                        _config.DevicesRefreshSeconds = refreshSeconds;
                    break;

                case "--token":
                case "-t":
                    if (i + 1 < args.Length)
                        _config.JwtToken = args[++i];
                    break;

                case "--device-guid":
                case "-d":
                    if (i + 1 < args.Length)
                        AddDeviceGuids(args[++i]);
                    break;

                case "--device-guids":
                    if (i + 1 < args.Length)
                        AddDeviceGuids(args[++i]);
                    break;

                case "--min-delay":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int minDelay))
                        _config.MinDelaySeconds = minDelay;
                    break;

                case "--max-delay":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int maxDelay))
                        _config.MaxDelaySeconds = maxDelay;
                    break;

                case "--help":
                case "-h":
                    ShowHelp();
                    Environment.Exit(0);
                    break;
            }
        }

        if (_config.MinDelaySeconds <= 0)
            _config.MinDelaySeconds = 1;

        if (_config.MaxDelaySeconds < _config.MinDelaySeconds)
            _config.MaxDelaySeconds = _config.MinDelaySeconds;

        if (_config.DevicesRefreshSeconds <= 0)
            _config.DevicesRefreshSeconds = 10;

        if (_configuredDeviceGuids.Count == 0)
        {
            _configuredDeviceGuids.Add(Guid.NewGuid().ToString());
        }

        _config.DeviceGuids = _configuredDeviceGuids;

        // Тут можна додати завантаження з appsettings.json або змінних середовища
        // Наприклад:
        // _config.JwtToken = Environment.GetEnvironmentVariable("JWT_TOKEN") ?? _config.JwtToken;
    }

    static void ShowHelp()
    {
        Console.WriteLine("Використання:");
        Console.WriteLine("  WokwiSimulator [опції]");
        Console.WriteLine();
        Console.WriteLine("Опції:");
        Console.WriteLine("  --url, -u <URL>           URL сервера (за замовчуванням: http://localhost:8080/api/Incident/wokwi)");
        Console.WriteLine("  --devices-url <URL>       URL API для списку девайсів (за замовчуванням: /api/Incident/wokwi/devices)");
        Console.WriteLine("  --devices-refresh <SEC>   Інтервал оновлення списку девайсів (за замовчуванням: 30)");
        Console.WriteLine("  --token, -t <TOKEN>       JWT токен (опціонально, не потрібен для WOKWI ендпоинту)");
        Console.WriteLine("  --device-guid, -d <GUID>  GUID пристрою (можна вказати кілька разів або через кому)");
        Console.WriteLine("  --device-guids <LIST>     Список GUID через кому/крапку з комою/пробіл");
        Console.WriteLine("  --min-delay <SECONDS>     Мінімальний інтервал між подіями (за замовчуванням: 5)");
        Console.WriteLine("  --max-delay <SECONDS>     Максимальний інтервал між подіями (за замовчуванням: 30)");
        Console.WriteLine("  --help, -h                Показати цю довідку");
        Console.WriteLine();
        Console.WriteLine("Приклад:");
        Console.WriteLine("  WokwiSimulator --url http://localhost:8080/api/Incident/wokwi");
        Console.WriteLine("  WokwiSimulator --device-guid \"550e8400-e29b-41d4-a716-446655440000\" --device-guid \"123e4567-e89b-12d3-a456-426614174000\"");
    }

    static async Task<string> SelectRandomDeviceGuidAsync(Random random)
    {
        var discoveredGuids = _deviceGuidProvider is null
            ? new List<string>()
            : (await _deviceGuidProvider.GetDeviceGuidsAsync()).ToList();

        var activePool = discoveredGuids.Count > 0 ? discoveredGuids : _configuredDeviceGuids;
        if (activePool.Count == 0)
        {
            var generated = Guid.NewGuid().ToString();
            _configuredDeviceGuids.Add(generated);
            activePool = _configuredDeviceGuids;
        }

        return activePool[random.Next(activePool.Count)];
    }

    static string DeriveDevicesApiUrl(string serverUrl)
    {
        var normalized = serverUrl.TrimEnd('/');
        const string suffix = "/api/Incident/wokwi";
        if (normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return $"{normalized}/devices";
        }

        return $"{normalized}/api/Incident/wokwi/devices";
    }

    static void AddDeviceGuids(string rawValue)
    {
        var tokens = rawValue
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            if (!_configuredDeviceGuids.Contains(token, StringComparer.OrdinalIgnoreCase))
            {
                _configuredDeviceGuids.Add(token);
            }
        }
    }
}

