namespace Rrule.Internal;

/// <summary>
/// Expands a <c>FREQ=MONTHLY</c> rule. Each period is one month; <c>BYMONTHDAY</c> and
/// <c>BYDAY</c> expand it (delegated to <see cref="MonthCandidateBuilder"/>) and
/// <c>BYMONTH</c> limits which months take part.
/// </summary>
internal sealed class MonthlyExpander : IPeriodExpander
{
    public DateTime InitialAnchor(RecurrenceRule rule, DateTime start) =>
        new(start.Year, start.Month, Constants.FirstDayOfMonth);

    public DateTime NextAnchor(RecurrenceRule rule, DateTime anchor) => anchor.AddMonths(rule.Interval);

    public IEnumerable<DateTime> Candidates(RecurrenceRule rule, DateTime anchor, DateTime start)
    {
        if (rule.ByMonth.Count > 0 && !rule.ByMonth.Contains(anchor.Month))
        {
            return Array.Empty<DateTime>();
        }

        return MonthCandidateBuilder.Build(anchor.Year, anchor.Month, rule, start.Day);
    }
}
