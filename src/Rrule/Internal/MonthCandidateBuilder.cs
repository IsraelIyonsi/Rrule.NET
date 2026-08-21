namespace Rrule.Internal;

/// <summary>
/// Builds the candidate dates inside a single month from the <c>BYMONTHDAY</c> and
/// <c>BYDAY</c> parts, following RFC 5545 within a monthly context (ordinal BYDAY entries
/// select a specific occurrence in the month; when both parts are present BYDAY limits
/// BYMONTHDAY to matching weekdays). Shared by <c>MONTHLY</c> expansion and by
/// <c>YEARLY</c> expansion restricted to specific months via <c>BYMONTH</c>.
/// </summary>
internal static class MonthCandidateBuilder
{
    public static IEnumerable<DateTime> Build(int year, int month, RecurrenceRule rule, int defaultDay)
    {
        var hasByMonthDay = rule.ByMonthDay.Count > 0;
        var hasByDay = rule.ByDay.Count > 0;

        IEnumerable<DateTime> dates;
        if (hasByMonthDay)
        {
            dates = FromMonthDays(year, month, rule.ByMonthDay);
            if (hasByDay)
            {
                var weekdays = WeekdaySet(rule.ByDay);
                dates = dates.Where(d => weekdays.Contains(d.DayOfWeek));
            }
        }
        else if (hasByDay)
        {
            dates = FromByDay(year, month, rule.ByDay);
        }
        else
        {
            dates = FromDefaultDay(year, month, defaultDay);
        }

        return dates;
    }

    private static IEnumerable<DateTime> FromMonthDays(int year, int month, IReadOnlyList<int> monthDays)
    {
        foreach (var monthDay in monthDays)
        {
            var day = DateHelpers.ResolveMonthDay(year, month, monthDay);
            if (day is not null)
            {
                yield return new DateTime(year, month, day.Value);
            }
        }
    }

    private static IEnumerable<DateTime> FromByDay(int year, int month, IReadOnlyList<WeekdayNum> byDay)
    {
        foreach (var entry in byDay)
        {
            var occurrences = DateHelpers.WeekdayOccurrencesInMonth(year, month, entry.Day);
            if (entry.Ordinal is null)
            {
                foreach (var occurrence in occurrences)
                {
                    yield return occurrence;
                }
            }
            else
            {
                var selected = DateHelpers.SelectByOrdinal(occurrences, entry.Ordinal.Value);
                if (selected is not null)
                {
                    yield return selected.Value;
                }
            }
        }
    }

    private static IEnumerable<DateTime> FromDefaultDay(int year, int month, int defaultDay)
    {
        if (defaultDay <= DateTime.DaysInMonth(year, month))
        {
            yield return new DateTime(year, month, defaultDay);
        }
    }

    private static HashSet<DayOfWeek> WeekdaySet(IReadOnlyList<WeekdayNum> byDay)
    {
        var set = new HashSet<DayOfWeek>();
        foreach (var entry in byDay)
        {
            set.Add(entry.Day);
        }

        return set;
    }
}
