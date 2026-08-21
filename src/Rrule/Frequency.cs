namespace Rrule;

/// <summary>
/// The <c>FREQ</c> rule part: how often the base recurrence repeats.
/// </summary>
/// <remarks>
/// The date-based frequencies (<see cref="Daily"/>, <see cref="Weekly"/>,
/// <see cref="Monthly"/>, <see cref="Yearly"/>) are fully supported by expansion.
/// The sub-daily frequencies are parsed for completeness but are deferred: expanding
/// them throws <see cref="NotSupportedException"/>.
/// </remarks>
public enum Frequency
{
    /// <summary>Repeat every second. Parsed but not expanded (deferred).</summary>
    Secondly,

    /// <summary>Repeat every minute. Parsed but not expanded (deferred).</summary>
    Minutely,

    /// <summary>Repeat every hour. Parsed but not expanded (deferred).</summary>
    Hourly,

    /// <summary>Repeat every day.</summary>
    Daily,

    /// <summary>Repeat every week.</summary>
    Weekly,

    /// <summary>Repeat every month.</summary>
    Monthly,

    /// <summary>Repeat every year.</summary>
    Yearly,
}
