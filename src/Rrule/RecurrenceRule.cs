using Rrule.Internal;

namespace Rrule;

/// <summary>
/// An immutable, parsed RFC 5545 <c>RRULE</c>. Build one with <see cref="Parse(string)"/>,
/// then expand it into occurrences with <see cref="Recurrence"/>.
/// </summary>
public sealed class RecurrenceRule
{
    internal RecurrenceRule(
        Frequency frequency,
        int interval,
        int? count,
        DateTime? until,
        bool untilIsDateOnly,
        IReadOnlyList<int> byMonth,
        IReadOnlyList<int> byMonthDay,
        IReadOnlyList<WeekdayNum> byDay,
        IReadOnlyList<int> byYearDay,
        IReadOnlyList<int> byWeekNo,
        IReadOnlyList<int> bySetPos,
        DayOfWeek weekStart)
    {
        Frequency = frequency;
        Interval = interval;
        Count = count;
        Until = until;
        UntilIsDateOnly = untilIsDateOnly;
        ByMonth = byMonth;
        ByMonthDay = byMonthDay;
        ByDay = byDay;
        ByYearDay = byYearDay;
        ByWeekNo = byWeekNo;
        BySetPos = bySetPos;
        WeekStart = weekStart;
    }

    /// <summary>The base repeat unit (<c>FREQ</c>).</summary>
    public Frequency Frequency { get; }

    /// <summary>How many <see cref="Frequency"/> units are between occurrences (<c>INTERVAL</c>, default 1).</summary>
    public int Interval { get; }

    /// <summary>The maximum number of occurrences (<c>COUNT</c>), or <see langword="null"/> if unbounded by count.</summary>
    public int? Count { get; }

    /// <summary>The inclusive upper bound on occurrence dates (<c>UNTIL</c>), or <see langword="null"/>.</summary>
    public DateTime? Until { get; }

    /// <summary>
    /// Whether <see cref="Until"/> was given as a date with no time. When true the bound is
    /// treated as inclusive of the whole day, so occurrences on that date are kept regardless
    /// of their time of day.
    /// </summary>
    public bool UntilIsDateOnly { get; }

    /// <summary>The months (1 to 12) the rule is limited to or expanded over (<c>BYMONTH</c>).</summary>
    public IReadOnlyList<int> ByMonth { get; }

    /// <summary>The days of the month (1 to 31, or -1 to -31 counting from the end) (<c>BYMONTHDAY</c>).</summary>
    public IReadOnlyList<int> ByMonthDay { get; }

    /// <summary>The weekday entries, optionally ordinal-qualified (<c>BYDAY</c>).</summary>
    public IReadOnlyList<WeekdayNum> ByDay { get; }

    /// <summary>The days of the year (1 to 366, or -1 to -366 from the end) (<c>BYYEARDAY</c>).</summary>
    public IReadOnlyList<int> ByYearDay { get; }

    /// <summary>The ISO week numbers (<c>BYWEEKNO</c>). Parsed but deferred; expansion throws if present.</summary>
    public IReadOnlyList<int> ByWeekNo { get; }

    /// <summary>The 1-based positions selected from each period's candidate set (<c>BYSETPOS</c>), negatives count from the end.</summary>
    public IReadOnlyList<int> BySetPos { get; }

    /// <summary>The day the week starts on (<c>WKST</c>, default Monday). Affects <c>WEEKLY</c> period boundaries.</summary>
    public DayOfWeek WeekStart { get; }

    /// <summary>
    /// Parses an RFC 5545 <c>RRULE</c> string (an optional <c>RRULE:</c> prefix is allowed).
    /// </summary>
    /// <param name="rrule">The rule text, for example <c>FREQ=MONTHLY;INTERVAL=2;BYDAY=1MO,-1FR;COUNT=10</c>.</param>
    /// <returns>The parsed rule.</returns>
    /// <exception cref="RruleParseException">The rule is malformed or violates RFC 5545.</exception>
    public static RecurrenceRule Parse(string rrule) => RruleParser.Parse(rrule);
}
