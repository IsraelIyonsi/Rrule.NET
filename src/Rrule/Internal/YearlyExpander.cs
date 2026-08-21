namespace Rrule.Internal;

/// <summary>
/// Expands a <c>FREQ=YEARLY</c> rule. Each period is one year. The RFC 5545 "Note 2"
/// interactions are resolved by precedence: <c>BYYEARDAY</c> expands then the other parts
/// limit; otherwise <c>BYMONTH</c> drives per-month expansion; otherwise a lone
/// <c>BYMONTHDAY</c> repeats across every month; otherwise a lone <c>BYDAY</c> expands
/// across the whole year (ordinals count within the year); otherwise the start month and
/// day recur.
/// </summary>
internal sealed class YearlyExpander : IPeriodExpander
{
    public DateTime InitialAnchor(RecurrenceRule rule, DateTime start) =>
        new(start.Year, Constants.JanuaryMonth, Constants.FirstDayOfMonth);

    public DateTime NextAnchor(RecurrenceRule rule, DateTime anchor) => anchor.AddYears(rule.Interval);

    public IEnumerable<DateTime> Candidates(RecurrenceRule rule, DateTime anchor, DateTime start)
    {
        var year = anchor.Year;

        if (rule.ByYearDay.Count > 0)
        {
            return FromYearDays(year, rule);
        }

        if (rule.ByMonth.Count > 0)
        {
            return FromMonths(year, rule, start);
        }

        if (rule.ByMonthDay.Count > 0)
        {
            return FromMonthDaysEveryMonth(year, rule);
        }

        if (rule.ByDay.Count > 0)
        {
            return FromYearByDay(year, rule);
        }

        return FromStartMonthDay(year, start);
    }

    private static IEnumerable<DateTime> FromYearDays(int year, RecurrenceRule rule)
    {
        foreach (var yearDay in rule.ByYearDay)
        {
            var date = DateHelpers.ResolveYearDay(year, yearDay);
            if (date is null)
            {
                continue;
            }

            if (LimitFilters.PassesByMonth(date.Value, rule.ByMonth)
                && LimitFilters.PassesByMonthDay(date.Value, rule.ByMonthDay)
                && LimitFilters.PassesByDay(date.Value, rule.ByDay))
            {
                yield return date.Value;
            }
        }
    }

    private static IEnumerable<DateTime> FromMonths(int year, RecurrenceRule rule, DateTime start)
    {
        foreach (var month in rule.ByMonth)
        {
            foreach (var date in MonthCandidateBuilder.Build(year, month, rule, start.Day))
            {
                yield return date;
            }
        }
    }

    private static IEnumerable<DateTime> FromMonthDaysEveryMonth(int year, RecurrenceRule rule)
    {
        var weekdays = rule.ByDay.Count > 0 ? WeekdaySet(rule.ByDay) : null;
        for (var month = Constants.JanuaryMonth; month <= Constants.MonthsInYear; month++)
        {
            foreach (var monthDay in rule.ByMonthDay)
            {
                var day = DateHelpers.ResolveMonthDay(year, month, monthDay);
                if (day is null)
                {
                    continue;
                }

                var date = new DateTime(year, month, day.Value);
                if (weekdays is null || weekdays.Contains(date.DayOfWeek))
                {
                    yield return date;
                }
            }
        }
    }

    private static IEnumerable<DateTime> FromYearByDay(int year, RecurrenceRule rule)
    {
        foreach (var entry in rule.ByDay)
        {
            var occurrences = DateHelpers.WeekdayOccurrencesInYear(year, entry.Day);
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

    private static IEnumerable<DateTime> FromStartMonthDay(int year, DateTime start)
    {
        if (start.Day <= DateTime.DaysInMonth(year, start.Month))
        {
            yield return new DateTime(year, start.Month, start.Day);
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
