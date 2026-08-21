namespace Rrule.Internal;

/// <summary>Expands a <c>FREQ=DAILY</c> rule. Each period is a single day; all BYxxx parts limit it.</summary>
internal sealed class DailyExpander : IPeriodExpander
{
    public DateTime InitialAnchor(RecurrenceRule rule, DateTime start) => start.Date;

    public DateTime NextAnchor(RecurrenceRule rule, DateTime anchor) => anchor.AddDays(rule.Interval);

    public IEnumerable<DateTime> Candidates(RecurrenceRule rule, DateTime anchor, DateTime start)
    {
        if (LimitFilters.PassesByMonth(anchor, rule.ByMonth)
            && LimitFilters.PassesByMonthDay(anchor, rule.ByMonthDay)
            && LimitFilters.PassesByDay(anchor, rule.ByDay))
        {
            yield return anchor;
        }
    }
}
