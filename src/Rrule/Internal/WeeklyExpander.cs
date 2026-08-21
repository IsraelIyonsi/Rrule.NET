namespace Rrule.Internal;

/// <summary>
/// Expands a <c>FREQ=WEEKLY</c> rule. Each period is one week aligned to <c>WKST</c>;
/// <c>BYDAY</c> expands the week (plain weekdays, ordinals are not meaningful here) and
/// <c>BYMONTH</c> limits it. With no <c>BYDAY</c>, the occurrence keeps the start weekday.
/// </summary>
internal sealed class WeeklyExpander : IPeriodExpander
{
    public DateTime InitialAnchor(RecurrenceRule rule, DateTime start) =>
        DateHelpers.StartOfWeek(start.Date, rule.WeekStart);

    public DateTime NextAnchor(RecurrenceRule rule, DateTime anchor) =>
        anchor.AddDays((double)rule.Interval * Constants.DaysInWeek);

    public IEnumerable<DateTime> Candidates(RecurrenceRule rule, DateTime anchor, DateTime start)
    {
        for (var offset = 0; offset < Constants.DaysInWeek; offset++)
        {
            var day = anchor.AddDays(offset);
            if (IsSelectedWeekday(day, rule, start) && LimitFilters.PassesByMonth(day, rule.ByMonth))
            {
                yield return day;
            }
        }
    }

    private static bool IsSelectedWeekday(DateTime day, RecurrenceRule rule, DateTime start) =>
        rule.ByDay.Count > 0
            ? rule.ByDay.Any(entry => entry.Day == day.DayOfWeek)
            : day.DayOfWeek == start.DayOfWeek;
}
