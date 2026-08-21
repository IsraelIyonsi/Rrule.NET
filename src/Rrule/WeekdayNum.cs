namespace Rrule;

/// <summary>
/// One entry in a <c>BYDAY</c> list: a weekday, optionally qualified by an ordinal.
/// </summary>
/// <remarks>
/// The ordinal selects a single occurrence of the weekday within the enclosing
/// <c>MONTHLY</c> or <c>YEARLY</c> period. For example <c>1MO</c> is the first Monday,
/// <c>-1FR</c> is the last Friday. A plain weekday such as <c>MO</c> has no ordinal
/// (<see cref="Ordinal"/> is <see langword="null"/>) and matches every occurrence of
/// that weekday in the period.
/// </remarks>
public readonly struct WeekdayNum : IEquatable<WeekdayNum>
{
    /// <summary>Creates a BYDAY entry.</summary>
    /// <param name="day">The weekday.</param>
    /// <param name="ordinal">
    /// The 1-based ordinal (positive counts from the start of the period, negative from
    /// the end), or <see langword="null"/> for every occurrence of the weekday.
    /// </param>
    public WeekdayNum(DayOfWeek day, int? ordinal)
    {
        Day = day;
        Ordinal = ordinal;
    }

    /// <summary>The weekday this entry selects.</summary>
    public DayOfWeek Day { get; }

    /// <summary>
    /// The 1-based ordinal within the period, or <see langword="null"/> for every
    /// occurrence of <see cref="Day"/>.
    /// </summary>
    public int? Ordinal { get; }

    /// <inheritdoc />
    public bool Equals(WeekdayNum other) => Day == other.Day && Ordinal == other.Ordinal;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is WeekdayNum other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Day, Ordinal);
}
