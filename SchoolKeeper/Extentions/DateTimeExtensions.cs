namespace SchoolKeeper.Extentions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Конвертує DateOnly в DateTime з Kind=UTC для PostgreSQL
    /// </summary>
    public static DateTime ToUtcDateTime(this DateOnly date, TimeOnly time)
    {
        var dateTime = date.ToDateTime(time);
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    /// <summary>
    /// Конвертує DateOnly в DateTime (початок дня) з Kind=UTC для PostgreSQL
    /// </summary>
    public static DateTime ToUtcDateTimeStart(this DateOnly date)
    {
        return date.ToUtcDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Конвертує DateOnly в DateTime (кінець дня) з Kind=UTC для PostgreSQL
    /// </summary>
    public static DateTime ToUtcDateTimeEnd(this DateOnly date)
    {
        return date.ToUtcDateTime(TimeOnly.MaxValue);
    }
}

