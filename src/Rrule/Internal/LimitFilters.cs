namespace Rrule.Internal;

/// <summary>
/// The RFC 5545 "limit" behaviours: parts that narrow an already-expanded candidate
/// date rather than generating new ones. Used by the daily and yearly expanders.
/// </summary>
internal static class LimitFilters
{
    public static bool PassesByMonth(DateTime date, IReadOnlyList<int> byMonth) =>
        byMonth.Count == 0 || byMonth.Contains(date.Month);

    public static bool PassesByMonthDay(DateTime date, IReadOnlyList<int> byMonthDay)
    {
        if (byMonthDay.Count == 0)
        {
            return true;
        }

        foreach (var monthDay in byMonthDay)
        {
            if (DateHelpers.ResolveMonthDay(date.Year, date.Month, monthDay) == date.Day)
            {
                return true;
            }
        }

        return false;
    }

    public static bool PassesByDay(DateTime date, IReadOnlyList<WeekdayNum> byDay)
    {
        if (byDay.Count == 0)
        {
            return true;
        }

        foreach (var entry in byDay)
        {
            if (entry.Day == date.DayOfWeek)
            {
                return true;
            }
        }

        return false;
    }
}
